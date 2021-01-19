using System;
using System.Security.Cryptography;

namespace Core.Users
{
    public abstract class BaseEncrypter
    {
        public abstract string Encrypt(string sPass, string key);

        public static string GetRand64String()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);

                return Convert.ToBase64String(tokenData);
            }
        }

        public void GenerateEncryptPassword(string clearPassword, ref string encryptPassword, ref string salt)
        {
            salt = GetRand64String();
            encryptPassword = Encrypt(clearPassword, salt);
        }
    }
}
