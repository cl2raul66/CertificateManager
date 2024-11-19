using CertificateManagerApp.Models;
using CertificateManagerApp.Tools;
using LiteDB;

namespace CertificateManagerApp.Services;

public interface IIdentityServices
{
    bool Exist { get; }

    void BeginTrans();
    void Commit();
    bool Delete(string id);
    IEnumerable<Identity> GetAll();
    string Insert(Identity entity);
    void Rollback();
}

public class IdentityServices : IIdentityServices
{
    readonly LiteDatabase db;
    readonly ILiteCollection<Identity> collection;

    public IdentityServices()
    {
        var cnxCert = new ConnectionString()
        {
            Filename = FileHelper.GetFileDbPath("Identities")
        };

        db = new LiteDatabase(cnxCert);
        collection = db.GetCollection<Identity>();
    }

    public void BeginTrans() => db.BeginTrans();
    public void Commit() => db.Commit();
    public void Rollback() => db.Rollback();

    public bool Exist => collection.Count() > 0;

    public string Insert(Identity entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = ShortGuidHelper.Generate();
        }
        return collection.Insert(entity).AsString ?? string.Empty;
    }

    public IEnumerable<Identity> GetAll()
    {
        return collection.FindAll();
    }

    public bool Delete(string id)
    {
        return collection.Delete(id);
    }
}
