using CertificateManagerApp.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateManagerApp.ViewModels;

public partial class PgMainViewModel : ObservableValidator
{
    [ObservableProperty]
    string? projectInfo;

    [RelayCommand]
    async Task LoadProject()
    {
        string solutionFilePath = await FileHelper.LoadProjectFile();
        if (!string.IsNullOrEmpty(solutionFilePath))
        {
            ProjectInfo = solutionFilePath;
        }
    }
}
