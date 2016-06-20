using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TVinciShared
{
    public class EncryptUtils
    {
        private const int BLOCK_SIZE = 16;

        public static byte[] AesEncrypt(string secretForSigning, byte[] text, byte[] iv = null)
        {
            // Key
            byte[] hashedKey = HashSHA1(secretForSigning);
            byte[] keyBytes = new byte[BLOCK_SIZE];
            Array.Copy(hashedKey, 0, keyBytes, 0, BLOCK_SIZE);
            
            //IV
            byte[] ivBytes;

            if (iv != null)
                ivBytes = iv;
            else
                ivBytes = new byte[BLOCK_SIZE];

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

        public static byte[] HashSHA1(string payload)
        {
            return HashSHA1(Encoding.UTF8.GetBytes(payload));
        }

        public static byte[] HashSHA1(byte[] payload)
        {
            var sha1 = SHA1Managed.Create();
            return sha1.ComputeHash(payload);
        }

        public static bool IsSignatureValid(string paramsString, string signature, string secret)
        {
            byte[] decryptedSignature = AesDecrypt(System.Convert.FromBase64String(signature), secret);
            byte[] sha1Params = HashSHA1(paramsString);

            return (decryptedSignature.SequenceEqual(sha1Params));
        }

        private static byte[] AesDecrypt(byte[] encryptedText, string secret)
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

        public static string EncryptRJ128(string key, string iv, string text)
        {
            var encoding = new UTF8Encoding();
            var Key = encoding.GetBytes(key);
            var IV = encoding.GetBytes(iv);
            byte[] encrypted;

            using (var rj = new RijndaelManaged())
            {
                try
                {
                    rj.Padding = PaddingMode.PKCS7;
                    rj.Mode = CipherMode.CBC;
                    rj.KeySize = 128;
                    rj.BlockSize = 128;
                    rj.Key = Key;
                    rj.IV = IV;

                    var ms = new MemoryStream();

                    using (var cs = new CryptoStream(ms, rj.CreateEncryptor(Key, IV), CryptoStreamMode.Write))
                    {
                        using (var sr = new StreamWriter(cs))
                        {
                            sr.Write(text);
                            sr.Flush();
                            cs.FlushFinalBlock();
                        }
                        encrypted = ms.ToArray();
                    }
                }
                finally
                {
                    rj.Clear();
                }
            }

            return BitConverter.ToString(encrypted).Replace("-", "").ToLower();
        }

    }
}
