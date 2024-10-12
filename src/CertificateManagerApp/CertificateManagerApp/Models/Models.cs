namespace CertificateManagerApp.Models;

public class WindowsCertificate
{
    public string? Id { get; set; } // Thumbprint of the certificate and ID of the PFX file stored in LiteDB
    public string? ApplicationId { get; set; } // Id of the project associated with the certificate
    public DateTimeOffset ExpiryDate { get; set; } // Certificate expiry date
    public string? PfxPassword { get; set; } // Password for the PFX file
}
