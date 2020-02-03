using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using KLogMonitor;

namespace AdapaterCommon.Helpers
{
    public static class EncryptionUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const int BLOCK_SIZE = 16;

        public static byte[] AesDecrypt(byte[] encryptedText, string secret)
        {
            // Key
            byte[] hashedKey = HashSHA1(secret);
            byte[] keyBytes = new byte[BLOCK_SIZE];
            Array.Copy(hashedKey, 0, keyBytes, 0, BLOCK_SIZE);

            //IV
            byte[] ivBytes = new byte[BLOCK_SIZE];

            // Decrypt
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes.Select(b => (byte)b).ToArray();
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.None;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cst = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cst.Write(encryptedText, 0, encryptedText.Length);
                        return TrimRight(ms.ToArray());
                    }
                }
            }
        }

        public static byte[] AesEncrypt(string secretForSigning, byte[] text)
        {
            // Key
            byte[] hashedKey = HashSHA1(secretForSigning);
            byte[] keyBytes = new byte[BLOCK_SIZE];
            Array.Copy(hashedKey, 0, keyBytes, 0, BLOCK_SIZE);

            //IV
            byte[] ivBytes = new byte[BLOCK_SIZE];

            // Text
            int textSize = ((text.Length + BLOCK_SIZE - 1) / BLOCK_SIZE) * BLOCK_SIZE;
            byte[] textAsBytes = new byte[textSize];
            Array.Copy(text, 0, textAsBytes, 0, text.Length);

            // Encrypt
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes.Select(b => (byte)b).ToArray();
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.None;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cst = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cst.Write(textAsBytes, 0, textSize);
                        return ms.ToArray();
                    }
                }
            }
        }

        public static bool IsSignatureValid(string paramsString, string signature, string secret)
        {
            try
            {
                string builtSignature = System.Convert.ToBase64String(AesEncrypt(secret, HashSHA1(paramsString)));
                return (builtSignature.SequenceEqual(signature));
            }
            catch (Exception ex)
            {
                log.Error("error while validating signature", ex);
                return false;
            }
        }

        public static string HashStringWithSha1(string stringToHash)
        {
            string response = null;

            byte[] hashed = HashSHA1(stringToHash);
            if (hashed != null && hashed.Length > 0)
            {
                response = hashed.Aggregate(string.Empty, (x, y) => x + y.ToString("X2").ToLower());
            }

            return response;
        }

        private static byte[] HashSHA1(string payload)
        {
            return HashSHA1(Encoding.UTF8.GetBytes(payload));
        }

        private static byte[] HashSHA1(byte[] payload)
        {
            var sha1 = SHA1Managed.Create();
            return sha1.ComputeHash(payload);
        }

        private static byte[] TrimRight(byte[] arr)
        {
            bool isFound = false;
            return arr.Reverse().SkipWhile(x =>
            {
                if (isFound)
                    return false;
                if (x == 0)
                    return true;
                else
                {
                    isFound = true;
                    return false;
                }
            }).Reverse().ToArray();
        }

        public static string MD5(string input)
        {
            // byte array representation of that string
            byte[] encodedPassword = new UTF8Encoding().GetBytes(input);

            // need MD5 to calculate the hash
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

            // string representation (similar to UNIX format)
            return BitConverter.ToString(hash)
               // without dashes
               .Replace("-", string.Empty)
               // make lowercase
               .ToLower();
        }
    }
}