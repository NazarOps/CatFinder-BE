using System.Security.Cryptography;
using System.Text;

namespace ApplicationLayer.Common.Security
{
    public static class TokenHasher
    {
        public static string Hash(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }

        public static bool Verify(string token, string expectedHash)
        {
            var tokenHashBytes = Encoding.UTF8.GetBytes(Hash(token));
            var expectedHashBytes = Encoding.UTF8.GetBytes(expectedHash);

            return CryptographicOperations.FixedTimeEquals(tokenHashBytes, expectedHashBytes);
        }
    }
}
