using CertificateManagerApp.Models;

namespace CertificateManagerApp.Tools;

public static class WindowsCertificateExtension
{
    public static Certificate ToCertificate(this WindowsCertificate entity)
    {
        return new Certificate
        {
            Id = entity.Id,
            ApplicationTitle = entity.Project?.ApplicationTitle ?? string.Empty,
            CommonName = entity.Owner?.CommonName ?? string.Empty,
            AppStores = entity.Project?.AppStore ?? default,
            ExpiryTime = (int)(entity.ExpiryDate - DateTimeOffset.Now).TotalDays,
        };
    }
}

