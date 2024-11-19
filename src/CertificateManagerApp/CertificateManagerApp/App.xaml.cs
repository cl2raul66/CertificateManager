using CertificateManagerApp.Resources.Localization;
using System.Globalization;
using System.Runtime.InteropServices;

namespace CertificateManagerApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        SetCulture();
        MainPage = new AppShell();
    }

    #region EXTRA
    void SetCulture()
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        ResourceDictionary resourceDictionary;

        switch (culture)
        {
            case "en":
                resourceDictionary = new LanguageEn();
                break;
            default:
                return;
        }

        var defaultDictionary = Resources.MergedDictionaries.FirstOrDefault(d => d.Source?.ToString().Contains("LanguageEn.xaml") == true);
        if (defaultDictionary is not null)
        {
            Resources.MergedDictionaries.Remove(defaultDictionary);
        }

        Resources.MergedDictionaries.Add(resourceDictionary);
    }
    #endregion
}
