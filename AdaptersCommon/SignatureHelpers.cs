using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using KLogMonitor;

namespace AdaptersCommon
{
    public class SignatureHelpers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int BLOCK_SIZE = 16;

        public static string CalculateSignature(string paramsString, string secret)
        {
            var paramsHash = HashSHA1(paramsString);
            var encryptParams = AesEncrypt(secret, paramsHash);
            var signature = Convert.ToBase64String(encryptParams);
            return signature;
        }

        public static bool IsSignatureValid(string paramsString, string signature, string secret)
        {
            try
            {
                string builtSignature = CalculateSignature(paramsString, secret);
                return (builtSignature.SequenceEqual(signature));
            }
            catch (Exception ex)
            {
                log.Error("error while validating signature", ex);
                return false;
            }
        }

        private static byte[] AesEncrypt(string secretForSigning, byte[] text)
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

        private static byte[] HashSHA1(string payload)
        {
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var sha1 = SHA1Managed.Create();
            return sha1.ComputeHash(payloadBytes);
        }
    }
}
