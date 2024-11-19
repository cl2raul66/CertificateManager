using CertificateManagerApp.Tools;
using CertificateManagerApp.Services;
using CertificateManagerApp.ViewModels;
using CertificateManagerApp.Views;
using CommunityToolkit.Maui;
using Microsoft.Build.Locator;
using Microsoft.Extensions.Logging;

namespace CertificateManagerApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("calibri.ttf", "calibri");
                fonts.AddFont("icofont.ttf", "icofont");
            });

        builder.AddAppsettings();
        //builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        builder.Services.AddSingleton<IIdentityServices, IdentityServices>();
        builder.Services.AddSingleton<IKeyBackupFileStorageService, KeyBackupFileStorageService>();
        builder.Services.AddSingleton<IWindowsCertificateFileStorageService, WindowsCertificateFileStorageService>();
        builder.Services.AddSingleton<IPasswordManagementService, PasswordManagementService>();
        builder.Services.AddSingleton<IWindowsCertificateService, WindowsCertificateService>();
        builder.Services.AddSingleton<IProjectAnalyzerForMAUIService, ProjectAnalyzerForMAUIService>();

        builder.Services.AddTransient<PgMain, PgMainViewModel>();
        builder.Services.AddTransient<PgSettings, PgSettingsViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Registrar el SDK de .NET
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        return builder.Build();
    }
}
