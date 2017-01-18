using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Core.Users
{
    public class SHA384Encrypter : BaseEncrypter
    {
        int m_nGroupID;

        public SHA384Encrypter()
        {
            m_nGroupID = 0;
        }

        public SHA384Encrypter(int nGroupID)
        {
            m_nGroupID = nGroupID;
        }

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

        public override void GenerateEncryptPassword(string clearPassword, ref string EncryptPassword, ref string salt)
        {
            salt = GetRand64String();
            EncryptPassword = Encrypt(clearPassword, salt);
        }
    }
}
