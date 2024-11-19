using CertificateManagerApp.Models;
using CertificateManagerApp.Services;
using CertificateManagerApp.Tools;
using CertificateManagerApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text;

namespace CertificateManagerApp.ViewModels;

public partial class PgMainViewModel : ObservableRecipient
{
    readonly IIdentityServices identityServ;
    readonly IKeyBackupFileStorageService keyBackupFileStorageServ;
    readonly IWindowsCertificateFileStorageService winCertFileStorageServ;
    readonly IWindowsCertificateService winCertServ;
    readonly IProjectAnalyzerForMAUIService projectAnalyzerForMAUIServ;

    public PgMainViewModel(IKeyBackupFileStorageService keyBackupFileStorageService, IWindowsCertificateFileStorageService windowsCertificateFileStorageService, IWindowsCertificateService windowsCertificateService, IProjectAnalyzerForMAUIService projectAnalyzerForMAUIService, IIdentityServices identityServices)
    {
        identityServ = identityServices;
        HasIdentity = identityServ.Exist;
        keyBackupFileStorageServ = keyBackupFileStorageService;
        winCertFileStorageServ = windowsCertificateFileStorageService;
        winCertServ = windowsCertificateService;
        projectAnalyzerForMAUIServ = projectAnalyzerForMAUIService;
    }

    [ObservableProperty]
    bool hasIdentity;

    [RelayCommand]
    async Task GoToSettings()
    {
        IsActive = true;
        await Shell.Current.GoToAsync(nameof(PgSettings), true);
    }

    #region OPERACIONES
    [ObservableProperty]
    string? projectDetails;

    [ObservableProperty]
    string? workInfo;

    [ObservableProperty]
    bool isBuilding;

    [ObservableProperty]
    ObservableCollection<string>? platforms;

    [ObservableProperty]
    string? selectedPlatform;

    [ObservableProperty]
    string? platformsForCertifying;

    [ObservableProperty]
    ProjectInfo? currentProjectInfo;

    [RelayCommand]
    async Task CancelCertificate()
    {
        LoadProjectCommand.Cancel();
        ProjectDetails = null;
        await Task.Delay(1000);
        PlatformsForCertifying = null;
        WorkInfo = null;
        CurrentProjectInfo = null;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    async Task LoadProject(CancellationToken token)
    {
        try
        {
            ProjectDetails = null;
            string projectFilePath = await FileHelper.LoadProjectFile();
            if (string.IsNullOrEmpty(projectFilePath)) return;

            token.ThrowIfCancellationRequested();

            IsBuilding = true;
            var progress = new Progress<string>(x => WorkInfo = x.Trim());
            var matchingResults = new Dictionary<string, string>();

            var projectName = projectAnalyzerForMAUIServ.GetProjectName(projectFilePath);
            if (projectName.StartsWith("E: "))
            {
                ProjectDetails = projectName[3..];
                return;
            }

            token.ThrowIfCancellationRequested();

            var buildTask = await projectAnalyzerForMAUIServ.BuildProjectAsync(projectFilePath, progress, token);

            token.ThrowIfCancellationRequested();

            Platforms = [.. projectAnalyzerForMAUIServ.ResultantOperatingSystems.Select(x => x.ToString())];

            if (Platforms.Count > 0)
            {
                StringBuilder infoOut = new();
                infoOut.AppendLine($"Nombre: {projectName}");
                infoOut.AppendLine("Target frameworks:");
                infoOut.AppendJoin(Environment.NewLine, Platforms);

                ProjectDetails = infoOut.ToString();

                CurrentProjectInfo = new()
                {
                    ApplicationId = projectAnalyzerForMAUIServ.GetApplicationId(projectFilePath),
                    ApplicationTitle = projectAnalyzerForMAUIServ.GetProjectNameForCertificate(projectFilePath),
                    ApplicationDisplayVersion = projectAnalyzerForMAUIServ.GetApplicationDisplayVersion(projectFilePath)
                };
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

    [RelayCommand]
    void AddTargetFrameworksForCertifying()
    {
        PlatformsForCertifying += string.IsNullOrEmpty(PlatformsForCertifying)
            ? SelectedPlatform
            : Environment.NewLine + SelectedPlatform;

        Platforms!.Remove(SelectedPlatform!);
    }
    #endregion

    #region CERTIFICADOS
    [ObservableProperty]
    ObservableCollection<Certificate>? certs;

    [ObservableProperty]
    Certificate? selectedCert;

    [RelayCommand]
    void AddCert()
    {
        var currentPlatforms = PlatformsForCertifying!.Split(Environment.NewLine).Select(x => TypesAppStoresExtension.FromString(x));
        if (currentPlatforms.Any())
        {
            foreach (var p in currentPlatforms)
            {
                Action theVoid = p switch
                {
                    TypesAppStores.Android => () => CertifyForAndroid(),
                    TypesAppStores.Windows => () => CertifyForWindows(),
                    _ => throw new NotImplementedException(),
                };

                theVoid();
            }
        }
        CurrentProjectInfo = null;
    }
    #endregion

    protected override void OnActivated()
    {
        base.OnActivated();

        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<PgMainViewModel, string, string>(this, "39579BB1B80F4B1F9B8D1AB2CAEEB5AB", (r, m) =>
        {
            IsActive = false;

            if (bool.TryParse(m, out bool getM))
            {
                HasIdentity = getM;
            }
        });
    }

    public async void Initialize()
    {
        if (winCertServ.Exist)
        {
            var certsAll = winCertServ.GetAll();
            if (certsAll is null || certsAll.Count() == 0)
            {
                return;
            }

            Certs = [.. certsAll.Select(x => x.ToCertificate())];
        }

        await Task.CompletedTask;
    }

    #region EXTRA
    void CertifyForAndroid()
    {
        //string filePath = string.Empty;

        //WindowsCertificate newWindowsCertificate = new();
        //winCertServ.BeginTrans();
        //var result = winCertServ.Insert(newWindowsCertificate);
        //if (string.IsNullOrEmpty(result))
        //{
        //    return;
        //}

        //var fileInfo = winCertFileStorageServ.Insert(result, filePath);
        //if (fileInfo.Id != result)
        //{
        //    winCertServ.Rollback();
        //    return;
        //}
        //winCertServ.Commit();
    }

    void CertifyForWindows()
    {
        CurrentProjectInfo!.AppStore = TypesAppStores.Windows;

        string filePath = string.Empty;

        WindowsCertificate newWindowsCertificate = new();
        winCertServ.BeginTrans();
        var result = winCertServ.Insert(newWindowsCertificate);
        if (string.IsNullOrEmpty(result))
        {
            return;
        }

        var fileInfo = winCertFileStorageServ.Insert(result, filePath);
        if (fileInfo.Id != result)
        {
            winCertServ.Rollback();
            return;
        }
        winCertServ.Commit();
    }
    #endregion
}
