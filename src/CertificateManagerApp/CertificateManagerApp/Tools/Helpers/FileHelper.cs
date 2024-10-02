using CertificateManagerApp.Models;
using System.Text;

namespace CertificateManagerApp.Tools;

public class FileHelper
{
    public static async Task<string> LoadProjectFile()
    {
        var projectFile = await FilePicker.Default.PickAsync();
        if (projectFile is not null)
        {
            var projectInfo = await ProjectAnalyzer.AnalyzeProjectFileFromPath(projectFile.FullPath);
            if (projectInfo is not null)
            {
                return GetProjectInfoString(projectInfo);
            }
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
    static string GetProjectInfoString(ProjectInfo projectInfo)
    {
        var properties = projectInfo.GetType().GetProperties();
        var result = new StringBuilder();

        foreach (var property in properties)
        {
            var value = property.GetValue(projectInfo);

            if (value is HashSet<string> capabilities)
            {
                _ = result.AppendLine(
                    capabilities.Count > 0
                    ? $"{property.Name}: {string.Join(", ", capabilities)}"
                    : $"{property.Name}: null"
                );
            }
            else
            {
                result.AppendLine($"{property.Name}: {value?.ToString() ?? "null"}");
            }
        }

        return result.ToString();
    }
    #endregion
}
