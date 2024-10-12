using CertificateManagerApp.Tools;
using LiteDB;

namespace CertificateManagerApp.Services;

public interface IWindowsCertificateFileStorageService
{
    void Delete(string id);
    IEnumerable<LiteFileInfo<string>> GetAll();
    LiteFileInfo<string>? GetFile(string id);
    void Insert(string id, string filePath);
}

public class WindowsCertificateFileStorageService : IWindowsCertificateFileStorageService
{
    readonly LiteDatabase db;
    readonly ILiteStorage<string> storage;

    public WindowsCertificateFileStorageService()
    {
        var cnxFiles = new ConnectionString()
        {
            Filename = FileHelper.GetFileDbPath("WindowsCertificateFilesStorage")
        };

        db = new LiteDatabase(cnxFiles);
        storage = db.FileStorage;
    }

    public void Insert(string id, string filePath)
    {
        storage.Upload(id, filePath);
    }

    public void Delete(string id)
    {
        storage.Delete(id);
    }

    public IEnumerable<LiteFileInfo<string>> GetAll()
    {
        return storage.FindAll();
    }

    public LiteFileInfo<string>? GetFile(string id)
    {
        return storage.FindById(id);
    }
}
