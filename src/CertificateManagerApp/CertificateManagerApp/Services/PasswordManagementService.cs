using System.Security.Cryptography;
using System.Text;

namespace CertificateManagerApp.Services;

public interface IPasswordManagementService
{
    Task ExtractPasswordAndCopyToClipboardAsync(string identifier);
    Task<string> GenerateStrongPasswordAsync(string identifier);
}

public class PasswordManagementService : IPasswordManagementService
{
    readonly IKeyBackupFileStorageService keyBackupFileStorageServ;

    public PasswordManagementService(IKeyBackupFileStorageService keyBackupFileStorageService)
    {
        keyBackupFileStorageServ = keyBackupFileStorageService;
    }

    public async Task<string> GenerateStrongPasswordAsync(string identifier)
    {
        var secureStorageKey = GenerateSecureStorageKey(identifier);

        // Generar una contraseña fuerte
        var password = GenerateStrongPassword();

        // Guardar la contraseña en Secure Storage
        await SecureStorage.SetAsync(secureStorageKey, password);

        // Hacer un respaldo usando KeyBackupService
        keyBackupFileStorageServ.BackupPassword(password, identifier);

        return password;
    }

    public async Task ExtractPasswordAndCopyToClipboardAsync(string identifier)
    {
        var secureStorageKey = GenerateSecureStorageKey(identifier);

        // Extraer la contraseña de Secure Storage
        var password = await SecureStorage.GetAsync(secureStorageKey);
        if (string.IsNullOrEmpty(password))
        {
            // Si no se encuentra en Secure Storage, recuperar del respaldo en KeyBackupService
            password = keyBackupFileStorageServ.RestorePassword(identifier);
        }

        // Copiar la contraseña al portapapeles
        if (!string.IsNullOrEmpty(password))
        {
            await Clipboard.SetTextAsync(password);
        }
    }

    #region EXTRA
    string GenerateStrongPassword()
    {
        const int length = 16;
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
        var chars = new char[length];
        var bytes = new byte[length];

        RandomNumberGenerator.Fill(bytes);

        for (int i = 0; i < length; i++)
        {
            chars[i] = validChars[bytes[i] % validChars.Length];
        }

        return new string(chars);
    }

    string GenerateSecureStorageKey(string identifier)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(identifier + Guid.NewGuid().ToString()));
        return Convert.ToBase64String(hash);
    }
    #endregion
}
