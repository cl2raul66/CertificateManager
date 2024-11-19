using CertificateManagerApp.Tools;

namespace CertificateManagerApp.Models;

public class Owner
{
    public string? Id { get; set; }
    public string? CommonName { get; set; } 
}

public class Certificate
{
    public string? Id { get; set; }
    public string? ApplicationTitle { get; set; }
    public string? CommonName { get; set; }
    public TypesAppStores AppStores { get; set; }
    public int ExpiryTime { get; set; } // In days
}
