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
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
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
                value = Phx.Lib.Appconfig.TCMClient.Settings.Instance.GetValue<string>(key);
            }
            catch
            {
            }

            return value;
        }

        public static string CalculateHMAC(string data, string key, Algorithm algorithm)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                HMAC hmac = HMAC.Create(algorithm.ToString());
                hmac.Key = ToByteArray(key);

                // compute hmac
                byte[] rawHmac = hmac.ComputeHash(Encoding.ASCII.GetBytes(data));

                // convert to hex string
                foreach (var b in rawHmac)
                {
                    sb.AppendFormat("{0:x2}", b);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create token", ex);
            }

            return sb.ToString();
        }

        public static byte[] ToByteArray(string me)
        {
            int len = me.Length;
            byte[] data = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                int val1 = -1, val2 = -1;

                try
                {
                    val1 = Convert.ToInt32(me[i].ToString(), 16) << 4;
                }
                catch (FormatException)
                {
                }
                catch (ArgumentException)
                {
                }

                try
                {
                    val2 = Convert.ToInt32(me[i + 1].ToString(), 16);
                }
                catch (FormatException)
                {
                }
                catch (ArgumentException)
                {
                }

                val1 += val2;
                data[i / 2] = Convert.ToByte(val1);
            }
            return data;
        }

    }
}
