using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.Users
{
    public class SHA256Encrypter : BaseEncrypter
    {
        public override string Encrypt(string sPass, string key)
        {
            return GetSHA256HashData(key + sPass);
        }

        private static string GetSHA256HashData(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            using (SHA256CryptoServiceProvider cryptoTransformSHA256 = new SHA256CryptoServiceProvider())
            {
                return BitConverter.ToString(cryptoTransformSHA256.ComputeHash(buffer)).Replace("-", "").ToLower();
            }
        }
    }
}
