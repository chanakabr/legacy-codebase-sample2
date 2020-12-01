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
            using (var sha1 = SHA1Managed.Create())
            {
                return sha1.ComputeHash(payload);
            }
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

        // "For AES, NIST selected three members of the Rijndael family, each with a block size of 128 bits, but three different key lengths: 128, 192 and 256 bits."
        // https://en.wikipedia.org/wiki/Advanced_Encryption_Standard
        public static byte[] AesDecrypt(byte[] text, string secretForSigning)
        {
            // Key
            byte[] hashedKey = HashSHA1(secretForSigning);
            byte[] keyBytes = new byte[BLOCK_SIZE];
            Array.Copy(hashedKey, 0, keyBytes, 0, BLOCK_SIZE);
            return AesDecrypt(text, keyBytes);
        }

        public static byte[] AesDecrypt(byte[] text, byte[] key)
        {
            //IV
            byte[] ivBytes = new byte[BLOCK_SIZE];

            // Decrypt
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.None;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cst = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cst.Write(text, 0, text.Length);
                        var arrayWithZeroChars = ms.ToArray();

                        var realLength = arrayWithZeroChars.Length;
                        while (realLength > 0 && arrayWithZeroChars[realLength - 1] == '\0') realLength--;

                        byte[] textAsBytes = new byte[realLength];
                        Array.Copy(arrayWithZeroChars, 0, textAsBytes, 0, realLength);
                        return textAsBytes;
                    }
                }
            }
        }

        public static byte[] AesEncrypt(byte[] text, string secretForSigning)
        {
            // Key
            byte[] hashedKey = HashSHA1(secretForSigning);
            byte[] keyBytes = new byte[BLOCK_SIZE];
            Array.Copy(hashedKey, 0, keyBytes, 0, BLOCK_SIZE);
            return AesEncrypt(text, keyBytes);
        }

        public static byte[] AesEncrypt(byte[] text, byte[] key)
        {
            //IV
            byte[] ivBytes = new byte[BLOCK_SIZE];

            // Text
            int textSize = ((text.Length + BLOCK_SIZE - 1) / BLOCK_SIZE) * BLOCK_SIZE;
            byte[] textAsBytes = new byte[textSize];
            Array.Copy(text, 0, textAsBytes, 0, text.Length);  // we will have '\0' chars in the end of the array, which will be trimmed in decryption. why do we need this???

            // Encrypt
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
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

        public static string HashMD5(string payload)
        {
            string response = null;
            if (!string.IsNullOrEmpty(payload))
            {
                using (MD5 md5 = MD5.Create())
                {
                    var hashed = md5.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    response = hashed.Aggregate(string.Empty, (x, y) => x + y.ToString("X2").ToLower());
                }
            }
            return response;
        }
    }
}
