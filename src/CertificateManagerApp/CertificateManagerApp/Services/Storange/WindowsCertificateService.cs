using CertificateManagerApp.Models;
using CertificateManagerApp.Tools;
using LiteDB;

namespace CertificateManagerApp.Services;

public interface IWindowsCertificateService
{
    bool Exist { get; }

    void BeginTrans();
    void Commit();
    bool Delete(string id);
    IEnumerable<WindowsCertificate> GetAll();
    string Insert(WindowsCertificate entity);
    void Rollback();
}

public class WindowsCertificateService : IWindowsCertificateService
{
    readonly LiteDatabase db;
    readonly ILiteCollection<WindowsCertificate> collection;

    public WindowsCertificateService()
    {
        var cnxCert = new ConnectionString()
        {
            Filename = FileHelper.GetFileDbPath("WinCerts")
        };

        db = new LiteDatabase(cnxCert);
        collection = db.GetCollection<WindowsCertificate>();
    }

    public void BeginTrans() => db.BeginTrans();
    public void Commit() => db.Commit();
    public void Rollback() => db.Rollback();

    public bool Exist => collection.Count() > 0;

    public string Insert(WindowsCertificate entity)
    {
        return collection.Insert(entity).AsString ?? string.Empty;
    }

    public IEnumerable<WindowsCertificate> GetAll()
    {
        return collection.FindAll();
    }

    public bool Delete(string id)
    {
        return collection.Delete(id);
    }
}
