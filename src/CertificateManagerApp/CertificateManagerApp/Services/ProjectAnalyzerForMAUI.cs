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
        if (IsMauiApp(path))
        {
            return await Task.Run(() =>
            {
                var projectCollection = new ProjectCollection();
                var globalProperty = new Dictionary<string, string>
                {
                    { "Configuration", "Release" }
                };
                var buildRequest = new BuildRequestData(path, globalProperty, null, ["Build"], null);

                var logger = new ConsoleLogger(LoggerVerbosity.Minimal, line => progress.Report(line), null, null);

                var buildParameters = new BuildParameters(projectCollection) { Loggers = [logger] };
                var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);

                if (buildResult.OverallResult is BuildResultCode.Success)
                {
                    var project = projectCollection.LoadProject(path);
                    string? assemblyName = project.GetPropertyValue("AssemblyName")
                                            ?? Path.GetFileNameWithoutExtension(path);

                    project.SetGlobalProperty("Configuration", "Release");
                    project.ReevaluateIfNecessary();

                    var outputPath = project.GetPropertyValue("OutputPath");

                    if (!string.IsNullOrEmpty(outputPath))
                    {
                        outputPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, outputPath));
                        var outputFiles = Directory.GetFiles(outputPath, "*.dll", SearchOption.AllDirectories);

                        buildOutput.AddRange(outputFiles.Where(file =>
                            Path.GetFileNameWithoutExtension(file).Contains(assemblyName, StringComparison.OrdinalIgnoreCase)));
                    }
                }

                return buildOutput;
            }, cancellationToken);
        }
        return ["E: No es un proyecto .NET MAUI"];
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
