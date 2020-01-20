using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using TVinciShared;

namespace KSWrapper
{
    public enum KSVersion
    {
        TVPAPI = 0,
        V2 = 1
    }

    public class KS
    {
        private const int BLOCK_SIZE = 16;
        private const string REPLACE_UNDERSCORE = "^^^";
        private const string KS_FORMAT = "{0}&_t={1}&_e={2}&_u={3}&_d={4}";

        public Dictionary<string, string> Privileges { get; private set; }
        private string EncryptedValue { get; set; }
        public string Data { get; private set; }
        public DateTime Expiration { get; private set; }
        public int GroupId { get; private set; }
        public int SessionType { get; private set; } // KalturaSessionType
        public string UserId { get; private set; }
        public KSVersion ksVersion { get; private set; }

        public KS(string secret, string groupID, string userID, int expiration, int userType, KSData data, Dictionary<string, string> privilegesList, KSVersion ksType, List<string> ksSecrets)
        {
            byte[] randomBytes = CreateRandomByteArray(BLOCK_SIZE);
            int relativeExpiration = (int)DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow) + expiration;
            //prepare data - url encode + replace '_'
            string encodedData = string.Empty;
            var payload = string.Empty;
            if (data != null)
            {
                data.Signature = ConvertSignature(randomBytes, ksSecrets);
                payload = data.PrepareKSPayload();
                encodedData = payload.Replace("_", REPLACE_UNDERSCORE);
                encodedData = HttpUtility.UrlEncode(encodedData);
            }

            this.Privileges = privilegesList;
            string ks = string.Format(KS_FORMAT, JoinPrivileges(), userType, relativeExpiration, userID, encodedData);
            byte[] ksBytes = Encoding.ASCII.GetBytes(ks);

            byte[] randWithFields = new byte[ksBytes.Length + randomBytes.Length];
            Array.Copy(randomBytes, 0, randWithFields, 0, randomBytes.Length);
            Array.Copy(ksBytes, 0, randWithFields, randomBytes.Length, ksBytes.Length);

            byte[] signature = EncryptUtils.HashSHA1(randWithFields);
            byte[] input = new byte[signature.Length + randWithFields.Length];
            Array.Copy(signature, 0, input, 0, signature.Length);
            Array.Copy(randWithFields, 0, input, signature.Length, randWithFields.Length);

            byte[] encryptedFields = AesEncrypt(secret, input, BLOCK_SIZE);
            string prefix = string.Format("v2|{0}|", groupID);

            byte[] output = new byte[encryptedFields.Length + prefix.Length];
            Array.Copy(Encoding.ASCII.GetBytes(prefix), 0, output, 0, prefix.Length);
            Array.Copy(encryptedFields, 0, output, prefix.Length, encryptedFields.Length);

            StringBuilder encodedKs = new StringBuilder(Convert.ToBase64String(output));
            encodedKs = encodedKs.Replace("+", "-");
            encodedKs = encodedKs.Replace("/", "_");
            encodedKs = encodedKs.Replace("\n", "");
            encodedKs = encodedKs.Replace("\r", "");

            this.EncryptedValue = encodedKs.ToString();
            this.ksVersion = ksType;
            this.Data = payload;
            this.Expiration = DateTime.UtcNow.AddSeconds(expiration);
            this.GroupId = int.Parse(groupID);
            this.SessionType = userType;
            this.UserId = userID;
        }

        private byte[] CreateRandomByteArray(int size)
        {
            byte[] b = new byte[size];
            new Random().NextBytes(b);
            return b;
        }

        private string ConvertSignature(byte[] randomBytes, List<string> ksSecrets)
        {
            if (ksSecrets == null || ksSecrets.Count == 0 || randomBytes == null )
            {
                return null;
            }

            var secret = ksSecrets.FirstOrDefault();
            var random = Encoding.Default.GetString(randomBytes);
            var concat = string.Format(KSData.SignatureFormat, random, secret);
            return Encoding.Default.GetString(EncryptUtils.HashSHA1(concat));
        }

        public string JoinPrivileges(string valuesSeperator = "&_", string inValueSeperator = "=")
        {
            if (this.Privileges == null || this.Privileges.Count == 0)
                return string.Empty;

            return string.Join(valuesSeperator, this.Privileges.Select(p => string.Join(inValueSeperator, p.Key, (p.Value == null ? string.Empty : p.Value))));
        }

        private byte[] AesEncrypt(string secretForSigning, byte[] text, int blockSize)
        {
            // Key
            byte[] hashedKey = EncryptUtils.HashSHA1(secretForSigning);
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

        public override string ToString()
        {
            return this.EncryptedValue;
        }
    }
}
