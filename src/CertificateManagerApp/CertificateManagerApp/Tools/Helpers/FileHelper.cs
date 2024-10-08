
namespace CertificateManagerApp.Tools;

public class FileHelper
{
    public static async Task<string> LoadProjectFile()
    {
        var projectFile = await FilePicker.Default.PickAsync();
        if (projectFile is not null)
        {
            return projectFile.FullPath;
        }
        return string.Empty;
    }

    //public static async Task LoadFolder()
    //{
    //    var folderResult = await FolderPicker.Default.PickAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    //    if (folderResult.IsSuccessful)
    //    {

    //    }
    //}

    #region EXTRA
    
    #endregion
}
