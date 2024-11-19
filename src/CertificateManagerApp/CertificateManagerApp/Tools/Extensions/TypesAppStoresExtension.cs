namespace CertificateManagerApp.Tools;

public class TypesAppStoresExtension
{
    public static string ToString(TypesAppStores type)
    {
        return Enum.GetName(typeof(TypesAppStores), type) ?? "NONE";
    }

    public static TypesAppStores FromString(string nameType)
    {
        if (Enum.TryParse(nameType, out TypesAppStores result))
        {
            return result;
        }
        return TypesAppStores.NONE;
    }
}
