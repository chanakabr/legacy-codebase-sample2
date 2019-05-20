using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace TVinciShared
{
    public class HashUtils
    {
        static public string GetUTF8MD5Hash(string hash)
        {
            using (MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider())
            {
                byte[] originalBytes = UTF8Encoding.Default.GetBytes(hash);
                byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
                return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
            }
        }

        public static string GetMD5HashUTF8EncodingInHexaString(string sInput)
        {
            MD5 md5 = null;
            StringBuilder sb = new StringBuilder();
            try
            {
                md5 = MD5.Create();
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(sInput));
                for (int i = 0; i < data.Length; i++)
                    sb.Append(data[i].ToString("x2"));
            }
            finally
            {
                #region Disposing
                if (md5 != null)
                {
                    md5.Clear();
                }
                #endregion
            }

            return sb.ToString();
        }
    }
}
