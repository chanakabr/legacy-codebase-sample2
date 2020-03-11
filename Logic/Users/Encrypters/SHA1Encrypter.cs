using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Core.Users
{
    public class SHA1Encrypter  : BaseEncrypter
    {
        int     m_nGroupID;

        public SHA1Encrypter()
        {
            m_nGroupID = 0;
        }

        public SHA1Encrypter(int nGroupID)
        {
            m_nGroupID = nGroupID;
        }

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

        public override void GenerateEncryptPassword(string clearPassword, ref string EncryptPassword, ref string salt)
        {
            salt            = GetRand64String();
            EncryptPassword = Encrypt(clearPassword, salt);
        }
    }
}
