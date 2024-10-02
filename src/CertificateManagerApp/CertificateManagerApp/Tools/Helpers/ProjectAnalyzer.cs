using System.Xml.Linq;
using CertificateManagerApp.Models;

namespace CertificateManagerApp.Tools;

public partial class ProjectAnalyzer
{
    public static ProjectInfo? AnalyzeProjectFile(string projectContent)
    {
        try
        {
            var doc = XDocument.Parse(projectContent);
            var projectInfo = new ProjectInfo
            {
                Language = DetermineLanguage(doc),
                Sdk = DetermineSdk(doc)
            };

            var propertyGroup = doc.Root?.Elements("PropertyGroup").FirstOrDefault();
            if (propertyGroup != null)
            {
                projectInfo.TargetFramework = DetermineTargetFramework(propertyGroup);
                projectInfo.OutputType = DetermineOutputType(propertyGroup);
                projectInfo.UsesWPF = DetermineUseWPF(propertyGroup);
                projectInfo.UsesWinForms = DetermineUseWinForms(propertyGroup);
            }

            projectInfo.ProjectType = DetermineProjectType(projectInfo);

            return projectInfo;
        }
        catch (Exception ex)
        {
            var result = new InvalidOperationException("Error al analizar el archivo del proyecto.", ex);
            Console.WriteLine(result.Message);
            return null;
        }
    }

    public static async Task<ProjectInfo?> AnalyzeProjectFileFromPath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        if (!filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var projectContent = await File.ReadAllTextAsync(filePath);
        return AnalyzeProjectFile(projectContent);
    }

    #region EXTRA
    static readonly Dictionary<string, string> SdkToProjectType = new()
    {
        // SDKs Web y Cloud
        { "Microsoft.NET.Sdk.Web", "ASP.NET Core Web Application" },
        { "Microsoft.NET.Sdk.Razor", "Razor Class Library" },
        { "Microsoft.NET.Sdk.BlazorWebAssembly", "Blazor WebAssembly Application" },
        { "Microsoft.NET.Sdk.Worker", "Worker Service" },
        { "Microsoft.NET.Sdk.Function", "Azure Functions" },
        { "Microsoft.NET.Sdk.Azure", "Azure Project" },
        { "Microsoft.NET.Sdk.SignalRClient", "SignalR Client" },
        { "Microsoft.NET.Sdk.WebSdk", "Web SDK" },

        // SDKs Desktop
        { "Microsoft.NET.Sdk.WindowsDesktop", "Windows Desktop Application" },
        { "Microsoft.NET.Sdk.Wpf", "WPF Application" },
        { "Microsoft.NET.Sdk.WindowsForms", "Windows Forms Application" },
        
        // SDKs Móvil y Multiplataforma
        { "Microsoft.NET.Sdk.Maui", "MAUI Application" },
        { "Microsoft.NET.Sdk.Android", "Android Application" },
        { "Microsoft.NET.Sdk.iOS", "iOS Application" },
        { "Microsoft.NET.Sdk.MacCatalyst", "Mac Catalyst Application" },
        { "Microsoft.NET.Sdk.MacOS", "macOS Application" },
        { "Microsoft.NET.Sdk.tvOS", "tvOS Application" },
        { "Microsoft.NET.Sdk.Tizen", "Tizen Application" },

        // SDKs Juegos
        { "Microsoft.NET.Sdk.Unity", "Unity Project" },
        { "Microsoft.NET.Sdk.UnityGameCore", "Unity Game Core" },

        // SDKs IoT y Dispositivos
        { "Microsoft.NET.Sdk.IoT", "IoT Application" },
        { "Microsoft.NET.Sdk.Device", "Device Application" },
        { "Microsoft.NET.Sdk.Nano", "Nano Framework" },

        // SDKs Extensiones y Herramientas
        { "Microsoft.NET.Sdk.Razor.Tool", "Razor Tool" },
        { "Microsoft.VisualStudio.Windows.Forms.DesignTools.WpfDesigner", "WPF Designer" },
        { "Microsoft.NET.Sdk.ProjectCreationTools", "Project Creation Tool" },
        { "Microsoft.NET.Sdk.SharedFramework.Ref", "Shared Framework Reference" },
        { "Microsoft.NET.Sdk.Web.ProjectSystem", "Web Project System" },
        { "Microsoft.NET.Sdk.Publish", "Publish Tool" },
        
        // SDKs Pruebas
        { "Microsoft.NET.Test.Sdk", "Test Project" },
        { "Microsoft.NET.Sdk.Templates", "Template Project" },

        // SDKs Básicos y Otros
        { "Microsoft.NET.Sdk", "General .NET Project" },
        { "Microsoft.NET.Sdk.IL", "IL Project" },
        { "Microsoft.NET.Sdk.WindowsAppSDK", "Windows App SDK" },
        { "Microsoft.NET.Sdk.NodeJs", "NodeJS .NET Project" },
        { "Microsoft.NET.Sdk.WebAssembly", "WebAssembly Project" },
        { "Microsoft.NET.Sdk.Runtime", "Runtime Project" },
        { "Microsoft.NET.Sdk.Composite", "Composite Project" },
        { "Microsoft.NET.Sdk.DefaultItems", "Default Items Project" },
        { "Microsoft.NET.Sdk.Publish.Tasks", "Publish Tasks" },
        { "Microsoft.NET.Sdk.ClientApp", "Client Application" },
        { "Microsoft.NET.Sdk.Web.Application.Ref", "Web Application Reference" }
    };

    static string DetermineSdk(XDocument doc)
    {
        return doc.Root?.Attribute("Sdk")?.Value ?? string.Empty;
    }

    static string DetermineLanguage(XDocument doc)
    {
        // Por defecto asumimos C#, pero podríamos expandir esto para detectar otros lenguajes
        return "C#";
    }

    static string DetermineTargetFramework(XElement propertyGroup)
    {
        var targetFramework = propertyGroup.Element("TargetFramework")?.Value;
        var targetFrameworks = propertyGroup.Element("TargetFrameworks")?.Value;

        return targetFramework ?? targetFrameworks ?? "No especificado";
    }

    static string DetermineOutputType(XElement propertyGroup)
    {
        return propertyGroup.Element("OutputType")?.Value ?? string.Empty;
    }

    static bool DetermineUseWPF(XElement propertyGroup)
    {
        return bool.TryParse(propertyGroup.Element("UseWPF")?.Value, out bool result) && result;
    }

    static bool DetermineUseWinForms(XElement propertyGroup)
    {
        return bool.TryParse(propertyGroup.Element("UseWindowsForms")?.Value, out bool result) && result;
    }

    static string DetermineProjectType(ProjectInfo info)
    {
        if (SdkToProjectType.TryGetValue(info.Sdk!, out string? baseType))
        {
            if (info.Sdk == "Microsoft.NET.Sdk")
            {
                return DetermineNetSdkProjectType(info);
            }
            else if (info.Sdk == "Microsoft.NET.Sdk.WindowsDesktop")
            {
                return DetermineWindowsDesktopProjectType(info);
            }
            return baseType;
        }
        return "SDK no reconocido";
    }

    static string DetermineNetSdkProjectType(ProjectInfo info)
    {
        return info.OutputType!.ToLowerInvariant() switch
        {
            "exe" => "Aplicación de Consola",
            "winexe" => "Aplicación de Windows",
            "library" => "Biblioteca de Clases",
            _ => "Proyecto .NET"
        };
    }

    static string DetermineWindowsDesktopProjectType(ProjectInfo info)
    {
        if (info.UsesWPF && info.UsesWinForms)
        {
            return "Aplicación WPF y Windows Forms";
        }
        else if (info.UsesWPF)
        {
            return "Aplicación WPF";
        }
        else if (info.UsesWinForms)
        {
            return "Aplicación Windows Forms";
        }
        return "Aplicación de Escritorio Windows";
    }
    #endregion
}
