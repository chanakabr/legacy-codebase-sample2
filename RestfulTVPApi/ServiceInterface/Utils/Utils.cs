using RestfulTVPApi.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace RestfulTVPApi.ServiceInterface
{
    public static class Utils
    {
        public static bool GetUseStartDateValue(int groupId)
        {
            //return bool.Parse(ConfigManager.GetInstance().GetConfig(groupId, platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate);
            return GroupsManager.GetInstance(groupId).UseStartDate;
            
        }

        public static bool GetIsSingleLoginValue(int groupId)
        {
            //return ConfigManager.GetInstance().GetConfig(groupId, platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
            return GroupsManager.GetInstance(groupId).ShouldSupportSingleLogin;
        }

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (long)diff.TotalSeconds;
        }

        public static string GetClientIP()
        {
            string ip = string.Empty;
            string retIp = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(retIp))
            {
                string[] ipRange = retIp.Split(',');
                ip = ipRange[ipRange.Length - 1];
            }
            else
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            if (ip.Equals("127.0.0.1") || ip.Equals("::1") || ip.StartsWith("192.168.")) ip = "81.218.199.175";

            return ip;
        }

        public static string DecryptSiteGuid(string key, string IV, string siteGuid)
        {
            string encrtyped = string.Empty;

            AesManaged aes = new AesManaged();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(IV);

            return DecryptStringFromBytes_AES(Convert.FromBase64String(siteGuid), aes.Key, aes.IV);
        }

        public static string DecryptStringFromBytes_AES(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            MemoryStream msDecrypt = null;
            CryptoStream csDecrypt = null;
            StreamReader srDecrypt = null;
            RijndaelManaged aesAlg = null;

            string plaintext = null;
            try
            {
                aesAlg = new RijndaelManaged();
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                msDecrypt = new MemoryStream(cipherText);
                csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                srDecrypt = new StreamReader(csDecrypt);

                plaintext = srDecrypt.ReadToEnd();
            }
            finally
            {
                if (srDecrypt != null)
                    srDecrypt.Close();
                if (csDecrypt != null)
                    csDecrypt.Close();
                if (msDecrypt != null)
                    msDecrypt.Close();

                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        public static string EncryptSiteGuid(string key, string IV, string siteGuid)
        {
            string encrtyped = string.Empty;

            AesManaged aes = new AesManaged();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(IV);

            return Convert.ToBase64String(EncryptStringToBytes_AES(siteGuid, aes.Key, aes.IV));
        }

        public static byte[] EncryptStringToBytes_AES(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the stream used to encrypt to an in memory
            // array of bytes.
            MemoryStream msEncrypt = null;

            // Declare the RijndaelManaged object
            // used to encrypt the data.
            RijndaelManaged aesAlg = null;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                msEncrypt = new MemoryStream();
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return msEncrypt.ToArray();
        }
    }
}