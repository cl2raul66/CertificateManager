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

            var propertyGroups = doc.Root?.Elements("PropertyGroup");
            if (propertyGroups is not null && propertyGroups.Any())
            {
                foreach (var propertyGroup in propertyGroups)
                {
                    projectInfo.TargetFramework = DetermineTargetFramework(propertyGroup) ?? projectInfo.TargetFramework;
                    projectInfo.OutputType = DetermineOutputType(propertyGroup) ?? projectInfo.OutputType;
                    projectInfo.UsesWPF |= DetermineUseWPF(propertyGroup);
                    projectInfo.UsesWinForms |= DetermineUseWinForms(propertyGroup);
                }
            }

            DetermineProjectCapabilities(doc, projectInfo);
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
        { "Microsoft.NET.Sdk", "Proyecto .NET" },
        { "Microsoft.NET.Sdk.Web", "Aplicación web ASP.NET Core" },
        { "Microsoft.NET.Sdk.Razor", "Biblioteca Razor" },
        { "Microsoft.NET.Sdk.BlazorWebAssembly", "Aplicación Blazor WebAssembly" },
        { "Microsoft.NET.Sdk.Worker", "Servicio Worker" },
        { "Microsoft.NET.Sdk.WindowsDesktop", "Aplicación de Escritorio Windows" },
        { "Microsoft.NET.Sdk.Maui", "Aplicación .NET MAUI" },
        { "Microsoft.NET.Sdk.iOS", "Aplicación iOS" },
        { "Microsoft.NET.Sdk.Android", "Aplicación Android" },
        { "Microsoft.NET.Sdk.MacCatalyst", "Aplicación MacCatalyst" },
        { "Microsoft.NET.Sdk.tvOS", "Aplicación tvOS" },
        { "Microsoft.NET.Sdk.Function", "Azure Functions" }
    };

    static string DetermineSdk(XDocument doc)
    {
        return doc.Root?.Attribute("Sdk")?.Value ?? string.Empty;
    }

    static string DetermineLanguage(XDocument doc)
    {
        // Podríamos expandir esto para detectar otros lenguajes si es necesario
        return "C#";
    }

    static string? DetermineTargetFramework(XElement propertyGroup)
    {
        return propertyGroup.Element("TargetFramework")?.Value ??
               propertyGroup.Element("TargetFrameworks")?.Value;
    }

    static string? DetermineOutputType(XElement propertyGroup)
    {
        return propertyGroup.Element("OutputType")?.Value;
    }

    static bool DetermineUseWPF(XElement propertyGroup)
    {
        return bool.TryParse(propertyGroup.Element("UseWPF")?.Value, out bool result) && result;
    }

    static bool DetermineUseWinForms(XElement propertyGroup)
    {
        return bool.TryParse(propertyGroup.Element("UseWindowsForms")?.Value, out bool result) && result;
    }


    static void DetermineProjectCapabilities(XDocument doc, ProjectInfo projectInfo)
    {
        var itemGroups = doc.Root?.Elements("ItemGroup");
        if (itemGroups != null)
        {
            projectInfo!.ProjectCapabilities ??= [];
            foreach (var itemGroup in itemGroups)
            {
                var projectCapabilities = itemGroup.Elements("ProjectCapability")
                    .Select(e => e.Attribute("Include")?.Value)
                    .Where(v => !string.IsNullOrEmpty(v));

                foreach (var capability in projectCapabilities)
                {
                    projectInfo!.ProjectCapabilities.Add(capability!);
                }
            }
        }
    }

    static string DetermineProjectType(ProjectInfo info)
    {
        // Primero, intentamos determinar por SDK
        if (SdkToProjectType.TryGetValue(info.Sdk!, out string? sdkType))
        {
            // Para Microsoft.NET.Sdk, necesitamos más análisis
            if (info.Sdk == "Microsoft.NET.Sdk")
            {
                return DetermineNetSdkProjectType(info);
            }
            return sdkType;
        }

        // Si no encontramos por SDK, usamos otras propiedades
        return DetermineProjectTypeByProperties(info);
    }

    static string DetermineNetSdkProjectType(ProjectInfo info)
    {
        // Verificar primero por capacidades específicas
        if (info.ProjectCapabilities!.Contains("TestContainer"))
        {
            return "Proyecto de Pruebas";
        }

        // Luego por OutputType
        return info.OutputType?.ToLowerInvariant() switch
        {
            "library" => "Biblioteca de Clases",
            "exe" => "Aplicación de Consola",
            "winexe" => "Aplicación de Windows",
            _ => "Biblioteca de Clases" // Por defecto, asumimos biblioteca si no hay OutputType
        };
    }

    static string DetermineProjectTypeByProperties(ProjectInfo info)
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

        // Verificar capacidades específicas del proyecto
        if (info.ProjectCapabilities!.Contains("Maui"))
        {
            return "Aplicación .NET MAUI";
        }
        if (info.ProjectCapabilities.Contains("XamarinAndroid"))
        {
            return "Aplicación Xamarin.Android";
        }
        if (info.ProjectCapabilities.Contains("XamariniOS"))
        {
            return "Aplicación Xamarin.iOS";
        }

        return "Tipo de proyecto no identificado";
    }
    #endregion
}
