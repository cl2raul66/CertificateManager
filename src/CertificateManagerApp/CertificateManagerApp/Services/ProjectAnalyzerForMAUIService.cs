using CertificateManagerApp.Tools;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

namespace CertificateManagerApp.Services;

public interface IProjectAnalyzerForMAUIService
{
    IEnumerable<TypesAppStores> ResultantOperatingSystems { get; }

    Task<IEnumerable<string>> BuildProjectAsync(string path, IProgress<string> progress, CancellationToken cancellationToken);
    string GetApplicationDisplayVersion(string path);
    string GetApplicationId(string path);
    string GetGitHubRepositoryId(string path);
    string GetProjectName(string path);
    string GetProjectNameForCertificate(string path);
}

public class ProjectAnalyzerForMAUIService : IProjectAnalyzerForMAUIService
{
    readonly ProjectCollection projectCollection = new();
    Project? project;

    public IEnumerable<TypesAppStores> ResultantOperatingSystems { get; private set; } = [];

    public string GetProjectName(string path)
    {
        if (IsMauiApp(path))
        {
            string? result = project!.GetPropertyValue("AssemblyName")
                                 ?? project.GetPropertyValue("RootNamespace")
                                 ?? project.GetPropertyValue("OutputName")
                                 ?? Path.GetFileNameWithoutExtension(path); // Fallback

            return result ?? string.Empty;
        }
        return "E: No es un proyecto .NET MAUI";
    }

    public async Task<IEnumerable<string>> BuildProjectAsync(string path, IProgress<string> progress, CancellationToken cancellationToken)
    {
        var buildOutput = new List<string>();
        if (!IsMauiApp(path))
        {
            return ["E: No es un proyecto .NET MAUI"];
        }

        return await Task.Run(() =>
        {
            try
            {
                progress?.Report("Iniciando compilación...");
                project!.SetProperty("Configuration", "Release");
                project!.SetProperty("RestorePackagesConfig", "false");
                project!.ReevaluateIfNecessary();

                string assemblyName = project!.GetPropertyValue("AssemblyName") ?? Path.GetFileNameWithoutExtension(path);
                string outputPath = project!.GetPropertyValue("OutputPath");
                if (string.IsNullOrEmpty(outputPath))
                {
                    return ["E: No se pudo determinar la ruta de salida"];
                }

                string projectDir = Path.GetDirectoryName(path) ?? string.Empty;
                string fullOutputPath = Path.GetFullPath(Path.Combine(projectDir, outputPath));
                progress?.Report($"Buscando archivos en: {fullOutputPath}");

                if (Directory.Exists(fullOutputPath))
                {
                    string[] files = Directory.GetFiles(fullOutputPath, "*.dll", SearchOption.AllDirectories).Where(x => Path.GetFileNameWithoutExtension(x).Contains(assemblyName, StringComparison.OrdinalIgnoreCase)).ToArray();
                    if (files.Any())
                    {
                        buildOutput.AddRange(files);
                    }
                    else
                    {
                        buildOutput.Add($"E: No se encontraron archivos DLL para {assemblyName}");
                    }
                }
                else
                {
                    buildOutput.Add($"E: El directorio de salida no existe: {fullOutputPath}");
                }

                // Obtener TargetFrameworks directamente durante la compilación
                var targetFrameworks = new List<string>();
                var targetFrameworksValue = project.GetPropertyValue("TargetFrameworks");
                if (!string.IsNullOrEmpty(targetFrameworksValue))
                {
                    targetFrameworks.AddRange(targetFrameworksValue.Split(';'));
                }
                else
                {
                    var targetFramework = project.GetPropertyValue("TargetFramework");
                    if (!string.IsNullOrEmpty(targetFramework))
                    {
                        targetFrameworks.Add(targetFramework);
                    }
                }

                // Obtener sistemas operativos basados en TargetFrameworks
                List<TypesAppStores> resultantOperatingSystems = new();
                foreach (var framework in targetFrameworks)
                {
                    var platform = framework switch
                    {
                        string f when f.Contains("ios") => TypesAppStores.iOS,
                        string f when f.Contains("maccatalyst") => TypesAppStores.MacCatalyst,
                        string f when f.Contains("android") => TypesAppStores.Android,
                        string f when f.Contains("windows") => TypesAppStores.Windows,
                        _ => TypesAppStores.NONE
                    };

                    resultantOperatingSystems.Add(platform);
                }

                ResultantOperatingSystems = resultantOperatingSystems;
            }
            catch (Exception ex)
            {
                buildOutput.Add($"E: Error durante el proceso: {ex.Message}");
            }

            return buildOutput;
        }, cancellationToken);
    }

    public string GetProjectNameForCertificate(string path)
    {
        if (IsMauiApp(path))
        {
            // Probar diferentes propiedades en el orden especificado
            string? result = project!.GetPropertyValue("ApplicationTitle")
                                 ?? project!.GetPropertyValue("OutputName")
                                 ?? project!.GetPropertyValue("RootNamespace")
                                 ?? project!.GetPropertyValue("AssemblyName")
                                 ?? Path.GetFileNameWithoutExtension(path); // Fallback

            return result ?? string.Empty;
        }
        return "E: No es un proyecto .NET MAUI";
    }

    public string GetApplicationId(string path)
    {
        if (IsMauiApp(path))
        {
            var result = project!.GetPropertyValue("ApplicationId");

            return result ?? string.Empty;
        }
        return "E: No es un proyecto .NET MAUI";
    }

    public string GetApplicationDisplayVersion(string path)
    {
        if (IsMauiApp(path))
        {
            var result = project!.GetPropertyValue("ApplicationDisplayVersion")
                                      ?? project.GetPropertyValue("Version");

            return result ?? "1.0.0";
        }
        return "E: No es un proyecto .NET MAUI";
    }

    public string GetGitHubRepositoryId(string path)
    {
        if (IsMauiApp(path))
        {
            var result = project!.GetPropertyValue("RepositoryUrl")?.Split('/').LastOrDefault();

            return result ?? string.Empty;
        }
        return "E: No es un proyecto .NET MAUI";
    }

    #region extra
    bool IsMauiApp(string path)
    {
        project = projectCollection.LoadProject(path);

        var useMauiProperty = project.AllEvaluatedProperties.FirstOrDefault(p => p.Name == "UseMaui");

        if (useMauiProperty != null && bool.TryParse(useMauiProperty.EvaluatedValue, out bool useMaui) && useMaui)
        {
            return true;
        }

        return false;
    }
    #endregion
}
