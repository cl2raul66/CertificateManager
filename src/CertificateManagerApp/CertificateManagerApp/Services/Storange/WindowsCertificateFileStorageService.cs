using CertificateManagerApp.Tools;
using LiteDB;

namespace CertificateManagerApp.Services;

public interface IWindowsCertificateFileStorageService
{
    void BeginTrans();
    void Commit();
    bool Delete(string id);
    IEnumerable<LiteFileInfo<string>> GetAll();
    LiteFileInfo<string>? GetFile(string id);
    LiteFileInfo<string> Insert(string id, string filePath);
    void Rollback();
}

public class WindowsCertificateFileStorageService : IWindowsCertificateFileStorageService
{
    readonly LiteDatabase db;
    readonly ILiteStorage<string> storage;

    public WindowsCertificateFileStorageService()
    {
        var cnxFiles = new ConnectionString()
        {
            Filename = FileHelper.GetFileDbPath("WinCertsFilesStorage")
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

    public LiteFileInfo<string> Insert(string id, string filePath)
    {
        return storage.Upload(id, filePath);
    }

    public bool Delete(string id)
    {
        return storage.Delete(id);
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
