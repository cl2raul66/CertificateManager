using CertificateManagerApp.Models;
using CertificateManagerApp.Tools;
using LiteDB;

namespace CertificateManagerApp.Services;

public interface IWindowsCertificateService
{
    bool Exist { get; }

    void BeginTrans();
    void Commit();
    void Delete(string id);
    IEnumerable<WindowsCertificate> GetAll();
    void Insert(WindowsCertificate certificate);
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
            Filename = FileHelper.GetFileDbPath("WindowsCertificates")
        };

        db = new LiteDatabase(cnxCert);
        collection = db.GetCollection<WindowsCertificate>();
    }

    public void BeginTrans() => db.BeginTrans();
    public void Commit() => db.Commit();
    public void Rollback() => db.Rollback();

    public bool Exist => collection.Count() > 0;

    public void Insert(WindowsCertificate certificate)
    {
        collection.Insert(certificate);
    }

    public IEnumerable<WindowsCertificate> GetAll()
    {
        return collection.FindAll();
    }

    public void Delete(string id)
    {
        collection.Delete(id);
    }
}
