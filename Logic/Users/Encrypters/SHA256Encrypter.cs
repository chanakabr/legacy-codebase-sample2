using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Core.Users
{
    public class SHA256Encrypter : BaseEncrypter
    {
        int m_nGroupID;

        public SHA256Encrypter()
        {
            m_nGroupID = 0;
        }

        public SHA256Encrypter(int nGroupID)
        {
            m_nGroupID = nGroupID;
        }

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

        public override void GenerateEncryptPassword(string clearPassword, ref string EncryptPassword, ref string salt)
        {
            salt = GetRand64String();
            EncryptPassword = Encrypt(clearPassword, salt);
        }
    }
}
