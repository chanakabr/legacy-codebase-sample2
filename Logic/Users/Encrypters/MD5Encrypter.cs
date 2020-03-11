using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class MD5Encrypter : BaseEncrypter
    {
        int m_nGroupID;

        public MD5Encrypter()
        {
        }

        public MD5Encrypter(int nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public override string Encrypt(string sPass, string key)
        {
            // step 1, calculate MD5 hash from input
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(sPass);
                byte[] hash = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }

        public override void GenerateEncryptPassword(string clearPassword, ref string EncryptPassword, ref string salt)
        {
            EncryptPassword = Encrypt(clearPassword, "");
        }
    }
}
