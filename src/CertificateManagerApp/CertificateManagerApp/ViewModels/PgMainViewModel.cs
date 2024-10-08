using CertificateManagerApp.Services;
using CertificateManagerApp.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;

namespace CertificateManagerApp.ViewModels;

public partial class PgMainViewModel : ObservableRecipient
{
    readonly IProjectAnalyzerForMAUI projectAnalyzerForMAUI_Serv;

    public PgMainViewModel(IProjectAnalyzerForMAUI projectAnalyzerForMAUI_Service)
    {
        projectAnalyzerForMAUI_Serv = projectAnalyzerForMAUI_Service;
    }

    [ObservableProperty]
    string? projectInfo;

    [ObservableProperty]
    string? workInfo;

    [ObservableProperty]
    bool isBuilding;

    [RelayCommand]
    async Task CancelCertificate()
    {
        LoadProjectCommand.Cancel();
        ProjectInfo = null;
        await Task.Delay(1000);
        WorkInfo = null;
        await Task.CompletedTask;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    async Task LoadProject(CancellationToken token)
    {
        try
        {
            ProjectInfo = null;
            string solutionFilePath = await FileHelper.LoadProjectFile();
            if (string.IsNullOrEmpty(solutionFilePath)) return;

            token.ThrowIfCancellationRequested();

            IsBuilding = true;
            var progress = new Progress<string>(x => WorkInfo = x.Trim());
            var matchingResults = new Dictionary<string, string>();

            var projectName = projectAnalyzerForMAUI_Serv.GetProjectName(solutionFilePath);
            if (projectName.StartsWith("E: "))
            {
                ProjectInfo = projectName[3..];
                return;
            }

            token.ThrowIfCancellationRequested();

            // Ejecutar BuildProjectAsync y GetMauiTargetFrameworksAsync en paralelo
            var buildTask = projectAnalyzerForMAUI_Serv.BuildProjectAsync(solutionFilePath, progress, token);
            var frameworksTask = projectAnalyzerForMAUI_Serv.GetTargetFrameworksAsync(solutionFilePath, token);

            token.ThrowIfCancellationRequested();

            await Task.WhenAll(buildTask, frameworksTask);

            token.ThrowIfCancellationRequested();

            var buildOutput = buildTask.Result;
            var targetFrameworks = frameworksTask.Result;

            foreach (var tf in targetFrameworks)
            {
                var matchedLine = buildOutput.FirstOrDefault(
                    line => line.Contains(tf, StringComparison.Ordinal));
                if (matchedLine is not null)
                {
                    matchingResults[tf] = matchedLine;
                }
            }

            if (matchingResults.Count > 0)
            {
                StringBuilder infoOut = new();
                infoOut.AppendLine($"Nombre: {projectName}");
                infoOut.AppendLine("Target frameworks:");
                infoOut.AppendJoin(Environment.NewLine, matchingResults.Keys);
                ProjectInfo = infoOut.ToString();
            }
        }
        catch (OperationCanceledException)
        {
            WorkInfo = "Operación cancelada.";
        }
        finally
        {
            IsBuilding = false;
            WorkInfo = null;
        }
    }
}
