using CertificateManagerApp.Models;

namespace CertificateManagerApp.Tools;

public static class IdentityExtension
{
    public static Owner ToOwner(this Identity entity)
    {
        return new Owner
        {
            Id = entity.Id,
            CommonName = entity.CommonName
        };
    }
}
