using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using WebAPI.Utils;

namespace WebAPI.Managers.Models
{
    public class KS
    {
        private const int BLOCK_SIZE = 16;
        private bool isValid;
        private string value;

        public enum eUserType { USER = 0, ADMIN = 2 }
        public bool IsValid
        {
            get { return isValid; }
        }

        public string Value
        {
            get { return value; }
        }

        public KS(string adminSecret, string groupID, string userID, int expiration, eUserType userType)
        {
            //string ksFormat = "_u={0}&_e={1}&_t={2}&Privileges={3}";
            string ksFormat = "{0}&_t={1}&_e={2}&_u={3}";


            int relativeExpiration = (int)SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow) + expiration;

            string ks = string.Format(ksFormat, "all=*", (int)userType, relativeExpiration, userID);
            byte[] ksBytes = Encoding.ASCII.GetBytes(ks);
            byte[] randomBytes = createRandomByteArray(BLOCK_SIZE);
            byte[] randWithFields = new byte[ksBytes.Length + randomBytes.Length];
            Array.Copy(randomBytes, 0, randWithFields, 0, randomBytes.Length);
            Array.Copy(ksBytes, 0, randWithFields, randomBytes.Length, ksBytes.Length);

            byte[] signature = hashSHA1(randWithFields);
            byte[] input = new byte[signature.Length + randWithFields.Length];
            Array.Copy(signature, 0, input, 0, signature.Length);
            Array.Copy(randWithFields, 0, input, signature.Length, randWithFields.Length);

            byte[] encryptedFields = aesEncrypt(adminSecret, input);
            string prefix = string.Format("v2|{0}|", groupID);

            byte[] output = new byte[encryptedFields.Length + prefix.Length];
            Array.Copy(Encoding.UTF8.GetBytes(prefix), 0, output, 0, prefix.Length);
            Array.Copy(encryptedFields, 0, output, prefix.Length, encryptedFields.Length);

            string encodedKs = System.Convert.ToBase64String(output);
            encodedKs = encodedKs.Replace("+", "-");
            encodedKs = encodedKs.Replace("/", "_");
            encodedKs = encodedKs.Replace("\n", "");
            encodedKs = encodedKs.Replace("\r", "");

            value = encodedKs;

        }

        public static KS CreateKSFromEncoded(string base64Value)
        {
            //TODO: decode, decrypt, populate
            return null;
        }

        private byte[] hashSHA1(string payload)
        {
            return hashSHA1(Encoding.ASCII.GetBytes(payload));
        }

        private byte[] hashSHA1(byte[] payload)
        {
            var sha1 = SHA1Managed.Create();
            return sha1.ComputeHash(payload);
        }


        private byte[] aesEncrypt(string secretForSigning, byte[] text)
        {
            // Key
            byte[] hashedKey = hashSHA1(secretForSigning);
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

        private byte[] createRandomByteArray(int size)
        {
            byte[] b = new byte[size];
            new Random().NextBytes(b);
            return b;
        }
    }
}