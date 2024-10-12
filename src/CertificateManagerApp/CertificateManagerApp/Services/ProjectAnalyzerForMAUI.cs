using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

namespace CertificateManagerApp.Services;

public interface IProjectAnalyzerForMAUI
{
    IEnumerable<string> ResultantOperatingSystems { get; }
    Task<IEnumerable<string>> BuildProjectAsync(string path, IProgress<string> progress, CancellationToken cancellationToken);
    string GetApplicationId(string path);
    string GetProjectName(string path);
    string GetProjectNameForCertificate(string path);
    //Task<IEnumerable<string>> GetTargetFrameworksAsync(string path, CancellationToken cancellationToken);
}

public class ProjectAnalyzerForMAUI : IProjectAnalyzerForMAUI
{
    public ProjectAnalyzerForMAUI()
    {
        // Registrar el SDK de .NET
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }
    }

    public IEnumerable<string> ResultantOperatingSystems { get; private set; } = [];

    public string GetProjectName(string path)
    {
        if (IsMauiApp(path))
        {
            var projectCollection = new ProjectCollection();
            var project = projectCollection.LoadProject(path);

            string? projectName = project.GetPropertyValue("AssemblyName")
                                 ?? project.GetPropertyValue("RootNamespace")
                                 ?? project.GetPropertyValue("OutputName")
                                 ?? Path.GetFileNameWithoutExtension(path); // Fallback

            return projectName ?? string.Empty;
        }
        return "E: No es un proyecto .NET MAUI";
    }

    //public async Task<IEnumerable<string>> GetTargetFrameworksAsync(string path, CancellationToken cancellationToken)
    //{
    //    var targetFrameworks = new List<string>();
    //    if (IsMauiApp(path))
    //    {
    //        return await Task.Run(() =>
    //        {
    //            var projectCollection = new ProjectCollection();
    //            var project = projectCollection.LoadProject(path);

    //            // Obtener TargetFrameworks
    //            string targetFrameworksValue = project.GetPropertyValue("TargetFrameworks");
    //            if (!string.IsNullOrEmpty(targetFrameworksValue))
    //            {
    //                targetFrameworks.AddRange(targetFrameworksValue.Split(';'));
    //            }
    //            else
    //            {
    //                // Obtener TargetFramework
    //                string targetFrameworkValue = project.GetPropertyValue("TargetFramework");
    //                if (!string.IsNullOrEmpty(targetFrameworkValue))
    //                {
    //                    targetFrameworks.Add(targetFrameworkValue);
    //                }
    //            }

    //            return targetFrameworks;
    //        }, cancellationToken);
    //    }
    //    return ["E: No es un proyecto .NET MAUI"];
    //}

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
                var projectCollection = new ProjectCollection();
                var project = projectCollection.LoadProject(path);

                project.SetProperty("Configuration", "Release");
                project.SetProperty("RestorePackagesConfig", "false");
                project.ReevaluateIfNecessary();

                string assemblyName = project.GetPropertyValue("AssemblyName") ?? Path.GetFileNameWithoutExtension(path);
                string outputPath = project.GetPropertyValue("OutputPath");
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
                List<string> resultantOperatingSystems = new();
                foreach (var framework in targetFrameworks)
                {
                    var platform = framework switch
                    {
                        string f when f.Contains("ios") => "iOS",
                        string f when f.Contains("maccatalyst") => "macOS",
                        string f when f.Contains("android") => "Android",
                        string f when f.Contains("windows") => "Windows",
                        _ => string.Empty
                    };

                    if (!string.IsNullOrEmpty(platform) && !resultantOperatingSystems.Contains(platform))
                    {
                        resultantOperatingSystems.Add(platform);
                    }
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
            var projectCollection = new ProjectCollection();
            var project = projectCollection.LoadProject(path);

            // Probar diferentes propiedades en el orden especificado
            string? projectName = project.GetPropertyValue("ApplicationTitle")
                                 ?? project.GetPropertyValue("OutputName")
                                 ?? project.GetPropertyValue("RootNamespace")
                                 ?? project.GetPropertyValue("AssemblyName")
                                 ?? Path.GetFileNameWithoutExtension(path); // Fallback

            return projectName ?? string.Empty;
        }
        return "E: No es un proyecto .NET MAUI";
    }

    public string GetApplicationId(string path)
    {
        if (IsMauiApp(path))
        {
            var projectCollection = new ProjectCollection();
            var project = projectCollection.LoadProject(path);
            var applicationId = project.GetPropertyValue("ApplicationId");

            return applicationId ?? string.Empty;
        }
        return "E: No es un proyecto .NET MAUI";
    }

    #region extra
    bool IsMauiApp(string path)
    {
        var projectCollection = new ProjectCollection();
        var project = projectCollection.LoadProject(path);

        var useMauiProperty = project.AllEvaluatedProperties.FirstOrDefault(p => p.Name == "UseMaui");

        if (useMauiProperty != null && bool.TryParse(useMauiProperty.EvaluatedValue, out bool useMaui) && useMaui)
        {
            return true;
        }

        return false;
    }
    #endregion
}
