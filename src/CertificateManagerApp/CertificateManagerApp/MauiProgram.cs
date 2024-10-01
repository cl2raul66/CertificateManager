using CommunityToolkit.Maui;
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

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
