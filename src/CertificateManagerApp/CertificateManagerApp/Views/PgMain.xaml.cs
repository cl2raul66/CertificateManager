using CertificateManagerApp.ViewModels;

namespace CertificateManagerApp.Views;

public partial class PgMain : ContentPage
{
	public PgMain(PgMainViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}