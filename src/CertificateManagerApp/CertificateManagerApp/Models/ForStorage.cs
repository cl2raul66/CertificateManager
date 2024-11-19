using CertificateManagerApp.Tools;

namespace CertificateManagerApp.Models;

public class WindowsCertificate
{
    public string? Id { get; set; } // Thumbprint of the certificate and ID of the PFX file stored in LiteDB
    public ProjectInfo? Project { get; set; } 
    public DateTimeOffset ExpiryDate { get; set; } // Certificate expiry date
    public string? PfxPassword { get; set; } // Password for the PFX file
    public Identity? Owner { get; set; }
}

public class ProjectInfo
{
    public string? ApplicationId { get; set; } // Id of the project associated with the certificate
    public string? ApplicationTitle { get; set; }
    public TypesAppStores AppStore { get; set; }
    public string? ApplicationDisplayVersion { get; set; } // Format x.x.x and starts with 1, Example: 1.0.0
    public string? GitHubRepositoryId { get; set; }
}

public class Identity
{
    public string? Id { get; set; }
    public string? CommonName { get; set; } // CN
    public string? Organization { get; set; } // O
    public string? Country { get; set; } // C
    public string? Email { get; set; } // E
    public string? GitHubId { get; set; }
}
