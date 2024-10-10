using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;
using Microsoft.Build.Logging;

namespace CertificateManagerApp.Services;

public interface IProjectAnalyzerForMAUI
{
    Task<IEnumerable<string>> BuildProjectAsync(string path, IProgress<string> progress, CancellationToken cancellationToken);
    string GetProjectName(string path);
    Task<IEnumerable<string>> GetTargetFrameworksAsync(string path, CancellationToken cancellationToken);
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

    public string GetProjectName(string path)
    {
        if (IsMauiApp(path))
        {
            var projectCollection = new ProjectCollection();
            var project = projectCollection.LoadProject(path);

            // Obtener el nombre del ensamblado o nombre del proyecto
            string? projectName = project.GetPropertyValue("AssemblyName")
                                  ?? project.GetPropertyValue("RootNamespace")
                                  ?? Path.GetFileNameWithoutExtension(path);

            return projectName;
        }
        return "E: No es un proyecto .NET MAUI";
    }

    public async Task<IEnumerable<string>> GetTargetFrameworksAsync(string path, CancellationToken cancellationToken)
    {
        var targetFrameworks = new List<string>();
        if (IsMauiApp(path))
        {
            return await Task.Run(() =>
            {
                var projectCollection = new ProjectCollection();
                var project = projectCollection.LoadProject(path);

                // Obtener TargetFrameworks
                string targetFrameworksValue = project.GetPropertyValue("TargetFrameworks");
                if (!string.IsNullOrEmpty(targetFrameworksValue))
                {
                    targetFrameworks.AddRange(targetFrameworksValue.Split(';'));
                }
                else
                {
                    // Obtener TargetFramework
                    string targetFrameworkValue = project.GetPropertyValue("TargetFramework");
                    if (!string.IsNullOrEmpty(targetFrameworkValue))
                    {
                        targetFrameworks.Add(targetFrameworkValue);
                    }
                }

                return targetFrameworks;
            }, cancellationToken);
        }
        return ["E: No es un proyecto .NET MAUI"];
    }
        
    public async Task<IEnumerable<string>> BuildProjectAsync(string path, IProgress<string> progress, CancellationToken cancellationToken)
    {
        var buildOutput = new List<string>();
        if (!IsMauiApp(path))
        {
            return new List<string> { "E: No es un proyecto .NET MAUI" };
        }

        return await Task.Run(() =>
        {
            try
            {
                progress.Report("Iniciando compilación...");

                var projectCollection = new ProjectCollection();
                var project = projectCollection.LoadProject(path);

                project.SetProperty("Configuration", "Release");
                project.SetProperty("RestorePackagesConfig", "false");

                project.ReevaluateIfNecessary();

                string assemblyName = project.GetPropertyValue("AssemblyName") ?? Path.GetFileNameWithoutExtension(path);
                string outputPath = project.GetPropertyValue("OutputPath");

                if (string.IsNullOrEmpty(outputPath))
                {
                    return new List<string> { "E: No se pudo determinar la ruta de salida" };
                }

                string projectDir = Path.GetDirectoryName(path) ?? string.Empty;
                string fullOutputPath = Path.GetFullPath(Path.Combine(projectDir, outputPath));
                progress.Report($"Buscando archivos en: {fullOutputPath}");

                if (Directory.Exists(fullOutputPath))
                {
                    var files = Directory.GetFiles(fullOutputPath, "*.dll", SearchOption.AllDirectories).Where(file => Path.GetFileNameWithoutExtension(file).Contains(assemblyName, StringComparison.OrdinalIgnoreCase)).ToList();

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
            }
            catch (Exception ex)
            {
                buildOutput.Add($"E: Error durante el proceso: {ex.Message}");
            }
            return buildOutput;
        }, cancellationToken);
    }

    #region extra
    bool IsMauiApp(string path)
    {
        var projectCollection = new ProjectCollection();
        var project = projectCollection.LoadProject(path);

        // Verificar si la propiedad UseMaui está presente y es true
        var useMauiProperty = project.AllEvaluatedProperties.FirstOrDefault(p => p.Name == "UseMaui");

        if (useMauiProperty != null && bool.TryParse(useMauiProperty.EvaluatedValue, out bool useMaui) && useMaui)
        {
            return true;
        }

        return false;
    }
    #endregion
}
