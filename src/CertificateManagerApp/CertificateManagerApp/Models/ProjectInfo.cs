namespace CertificateManagerApp.Models;

public class ProjectInfo
{
    public string? Language { get; set; }
    public string? TargetFramework { get; set; }
    public string? ProjectType { get; set; }
    public string? OutputType { get; set; }
    public bool UsesWPF { get; set; }
    public bool UsesWinForms { get; set; }
    public bool UsesMaui { get; set; }
    public string? Sdk { get; set; }
    public HashSet<string>? ProjectCapabilities { get; set; }
}
