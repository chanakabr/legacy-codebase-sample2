using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.Users
{
    public class SHA1Encrypter  : BaseEncrypter
    {
        public override string Encrypt(string sPass, string key)
        {
            return GetSHA1HashData(key + sPass);
        }

        private static string GetSHA1HashData(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            using (SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider())
            {
                return BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "").ToLower();
            }
        }
    }
}
