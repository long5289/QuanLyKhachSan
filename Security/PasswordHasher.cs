using System.Security.Cryptography;
using System.Text;

namespace HotelBooking.Web.Security;

public static class PasswordHasher
{
    public static string Hash(string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(password)));
}