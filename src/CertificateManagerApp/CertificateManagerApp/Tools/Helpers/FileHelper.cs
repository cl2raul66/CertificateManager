
namespace CertificateManagerApp.Tools;

public class FileHelper
{
    static readonly string DIR_DB = Path.Combine(AppContext.BaseDirectory, "Data");

    public static string GetFileDbPath(string db_filename)
    {
        if (!Directory.Exists(DIR_DB))
        {
            Directory.CreateDirectory(DIR_DB);
        }

        return Path.Combine(DIR_DB, $"{db_filename}.db");
    }

    public static async Task<string> LoadProjectFile()
    {
        var projectFile = await FilePicker.Default.PickAsync();
        if (projectFile is not null)
        {
            return projectFile.FullPath;
        }
        return string.Empty;
    }

    #region EXTRA

    #endregion
}
