using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TVinciShared
{
    public static class EncryptUtils
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
            var aes = new AesManaged
            {
                Key = Convert.FromBase64String(key),
                IV = Convert.FromBase64String(iv)
            };

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
                aesAlg = new RijndaelManaged
                {
                    Key = aes.Key,
                    IV = aes.IV
                };

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

        public static bool IsSignatureValid(string paramsString, string signature, string secret)
        {
            byte[] decryptedSignature = AesDecrypt(System.Convert.FromBase64String(signature), secret);
            byte[] sha1Params = HashSHA1(paramsString);

            return (decryptedSignature.SequenceEqual(sha1Params));
        }

        public static byte[] TrimRight(byte[] arr)
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
        
        public static byte[] HashSHA1(string payload)
        {
            return HashSHA1(Encoding.UTF8.GetBytes(payload));
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

        // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89geji340t89u2");

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

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

        private static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }
    }
}
