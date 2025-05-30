using CertificateManagerApp.ViewModels;

namespace CertificateManagerApp.Views;

public partial class PgMain : ContentPage
{
    public PgMain(PgMainViewModel vm)
    {
        InitializeComponent();

        vm.Initialize();
        BindingContext = vm;
    }

    void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var collectionView = sender as CollectionView;
        if (collectionView is not null)
        {
            if (e.CurrentSelection.Count == 0)
            {
                collectionView.SelectionMode = SelectionMode.None;
            }
            collectionView.SelectionMode = SelectionMode.Single;
        }
    }
}