using System.Security.Cryptography;
using System.Text;
using CertificateManagerApp.Tools;
using LiteDB;
using Microsoft.Extensions.Configuration;

namespace CertificateManagerApp.Services;

public interface IKeyBackupFileStorageService
{
    void BackupPassword(string password, string identifier);
    void BeginTrans();
    void Commit();
    string RestorePassword(string identifier);
    void Rollback();
}

public class KeyBackupFileStorageService : IKeyBackupFileStorageService
{
    private readonly LiteDatabase db;
    private readonly ILiteStorage<string> storage;

    public KeyBackupFileStorageService(IConfiguration configuration)
    {
        var dbPassword = configuration["PasswordBackup:DatabasePassword"];
        var cnxFiles = new ConnectionString()
        {
            Filename = FileHelper.GetFileDbPath("PwdBackup"),
            Password = dbPassword
        };

        db = new LiteDatabase(cnxFiles);
        storage = db.FileStorage;
    }

    public void BeginTrans()
    {
        db.BeginTrans();
    }

    public void Rollback()
    {
        db.Rollback();
    }

    public void Commit()
    {
        db.Commit();
    }

    public void BackupPassword(string password, string identifier)
    {
        var backupKey = GenerateBackupKey(identifier);

        var encryptedPassword = EncryptPassword(password);
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(encryptedPassword));
        storage.Upload(backupKey, backupKey, memoryStream);
    }

    public string RestorePassword(string identifier)
    {
        var backupKey = GenerateBackupKey(identifier);

        var file = storage.FindById(backupKey);
        if (file != null)
        {
            using (var stream = file.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                var encryptedPassword = reader.ReadToEnd();
                return DecryptPassword(encryptedPassword);
            }
        }
        return string.Empty;
    }

    private string GenerateBackupKey(string identifier)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(identifier));
            return Convert.ToBase64String(hash);
        }
    }

    private string EncryptPassword(string password)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
    }

    private string DecryptPassword(string encryptedPassword)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedPassword));
    }
}
