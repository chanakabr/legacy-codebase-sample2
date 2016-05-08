using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebAPI.Utils
{
    public static class EncryptionUtils
    {
        // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89geji340t89u2");

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

        public static string Encrypt(string plainText, string passPhrase)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                byte[] cipherTextBytes = memoryStream.ToArray();
                                return BitConverter.ToString(cipherTextBytes).Replace("-", "");
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            IEnumerable<string> arr = SplitInParts(cipherText, 2);
            byte[] array = new byte[arr.Count()];
            for (int i = 0; i < arr.Count(); i++)
                array[i] = Convert.ToByte(arr.ElementAt(i), 16);

            byte[] cipherTextBytes = array;
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        public static byte[] HashSHA1(string payload)
        {
            return HashSHA1(Encoding.ASCII.GetBytes(payload));
        }

        public static byte[] HashSHA1(byte[] payload)
        {
            byte[] response = null;
            if (payload != null)
            {
                using (var sha1 = SHA1Managed.Create())
                {
                    response = sha1.ComputeHash(payload);
                }
            }
            return response;
        }

        public static string HashSHA256(string payload)
        {
            string response = null;
            if (!string.IsNullOrEmpty(payload))
            {
                using (var sha256 = SHA256Managed.Create())
                {
                    var hashed = sha256.ComputeHash(Encoding.ASCII.GetBytes(payload));
                    response = hashed.Aggregate(string.Empty, (x, y) => x + y.ToString("X2").ToLower());
                }
            }
            return response;
        }

        public static string HashSHA512(string payload)
        {
            string response = null;
            if (!string.IsNullOrEmpty(payload))
            {
                using (var sha512 = SHA512Managed.Create())
                {
                    var hashed = sha512.ComputeHash(Encoding.ASCII.GetBytes(payload));
                    response = hashed.Aggregate(string.Empty, (x, y) => x + y.ToString("X2").ToLower());
                }
            }
            return response;
        }

        public static string HashMD5(string payload)
        {
            string response = null;
            if (!string.IsNullOrEmpty(payload))
            {
                using (MD5 md5 = MD5.Create())
                {
                    var hashed = md5.ComputeHash(Encoding.ASCII.GetBytes(payload));
                    response = hashed.Aggregate(string.Empty, (x, y) => x + y.ToString("X2").ToLower());
                }
            }
            return response;
        }

        public static byte[] AesEncrypt(string secretForSigning, byte[] text, int blockSize)
        {
            // Key
            byte[] hashedKey = HashSHA1(secretForSigning);
            byte[] keyBytes = new byte[blockSize];
            Array.Copy(hashedKey, 0, keyBytes, 0, blockSize);

            //IV
            byte[] ivBytes = new byte[blockSize];

            // Text
            int textSize = ((text.Length + blockSize - 1) / blockSize) * blockSize;
            byte[] textAsBytes = new byte[textSize];
            Array.Copy(text, 0, textAsBytes, 0, text.Length);

            // Encrypt
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
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

        public static byte[] AesEncrypt(string text, string iv, string key)
        {
            AesManaged aes = new AesManaged();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(iv);

            // Text
            byte[] textAsBytes = Encoding.UTF8.GetBytes(text);

            // Encrypt
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
                aesAlg.Key = aes.Key;
                aesAlg.IV = aes.IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                msEncrypt = new MemoryStream();
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {

                        //Write all data to the stream.
                        swEncrypt.Write(text);
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

        public static byte[] AesDecrypt(string secretForSigning, byte[] text, int blockSize)
        {
            // Key
            byte[] hashedKey = HashSHA1(secretForSigning);
            byte[] keyBytes = new byte[blockSize];
            Array.Copy(hashedKey, 0, keyBytes, 0, blockSize);

            //IV
            byte[] ivBytes = new byte[blockSize];

            // Text
            int textSize = ((text.Length + blockSize - 1) / blockSize) * blockSize;
            byte[] textAsBytes = new byte[textSize];
            Array.Copy(text, 0, textAsBytes, 0, text.Length);

            // Decrypt
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.None;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cst = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cst.Write(text, 0, text.Length);
                        return ms.ToArray();
                    }
                }
            }
        }

        public static byte[] CreateRandomByteArray(int size)
        {
            byte[] b = new byte[size];
            new Random().NextBytes(b);
            return b;
        }
    }
}