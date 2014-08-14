using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CDNTokenizers
{
    public static class Utils
    {
        public static long GetEpochUTCTimeNow()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static string GetMD5Hash(string hash)
        {
            using (MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider())
            {
                byte[] originalBytes = ASCIIEncoding.Default.GetBytes(hash);
                byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
                return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
            }
        }

        public static byte[] SignHmacSha1(byte[] key, byte[] message)
        {
            byte[] hashBytes;

            using (HMACSHA1 hmac = new HMACSHA1(key))
            {
                hashBytes = hmac.ComputeHash(message);
            }

            return hashBytes;
        }

        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();

            string hex;
            foreach (byte b in bytes)
            {
                hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public static void AddQueryStringParams(ref UriBuilder uriBuilder, string queryStr)
        {
            if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
                uriBuilder.Query = string.Concat(uriBuilder.Query.Substring(1), "&", queryStr);
            else
                uriBuilder.Query = queryStr;
        }

        public static string GetConfigValue(string key)
        {
            string value = null;
            try
            {
                value = TCMClient.Settings.Instance.GetValue<string>(key);
            }
            catch
            {
            }

            return value;
        }

    }
}
