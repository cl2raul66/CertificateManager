using CertificateManagerApp.Views;

namespace CertificateManagerApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(PgSettings), typeof(PgSettings));
    }
}
