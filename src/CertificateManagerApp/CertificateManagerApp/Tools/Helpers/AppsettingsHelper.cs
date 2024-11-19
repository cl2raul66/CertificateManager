using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace CertificateManagerApp.Tools;

internal static class  AppsettingsHelper
{
    public static void AddAppsettings(this MauiAppBuilder builder)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.appsettings.json")!;
        if (stream is not null)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
            builder.Configuration.AddConfiguration(config);
        }
    }
}
