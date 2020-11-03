using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.Users
{
    public class SHA384Encrypter : BaseEncrypter
    {
        public override string Encrypt(string sPass, string key)
        {
            return GetSHA384HashData(key + sPass);
        }

        private static string GetSHA384HashData(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            using (SHA384CryptoServiceProvider cryptoTransformSHA384 = new SHA384CryptoServiceProvider())
            {
                return BitConverter.ToString(cryptoTransformSHA384.ComputeHash(buffer)).Replace("-", "").ToLower();
            }
        }
    }
}
