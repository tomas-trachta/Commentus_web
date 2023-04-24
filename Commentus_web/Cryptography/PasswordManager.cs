using System.Text;
using System.Security.Cryptography;

namespace Commentus_web.Cryptography
{
    public static class PasswordManager
    {
        public static Tuple<byte[],byte[]> HashPassword(string password)
        {
            var random = RandomNumberGenerator.Create();
            int max_length = 32;
            byte[] salt = new byte[max_length];
            random.GetNonZeroBytes(salt);

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            byte[] saltedPasswordBytes = new byte[passwordBytes.Length + salt.Length];
            Array.Copy(passwordBytes, saltedPasswordBytes, passwordBytes.Length);
            Array.Copy(salt, 0, saltedPasswordBytes, passwordBytes.Length, salt.Length);

            byte[] hashBytes = SHA256.Create().ComputeHash(saltedPasswordBytes);

            return Tuple.Create(hashBytes, salt);
        }

        public static byte[] ComputeHash(string password, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            byte[] saltedPasswordBytes = new byte[passwordBytes.Length + salt.Length];
            Array.Copy(passwordBytes, saltedPasswordBytes, passwordBytes.Length);
            Array.Copy(salt, 0, saltedPasswordBytes, passwordBytes.Length, salt.Length);

            return SHA256.Create().ComputeHash(saltedPasswordBytes);
        }
    }
}
