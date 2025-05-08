using System.Security.Cryptography;
using System.Text;

namespace atmnr_api.Helpers;
public static class Constants
{
    public static readonly DateTime MinDate = new DateTime(1911, 1, 1);
    public static bool GuidEmpty(Guid? guid)
    {
        return guid == Guid.Empty || guid == null;
    }

    public static readonly int expiryDay = 20;
}