using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using WebAPI.Utils;
using WebAPI.Models;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using System.Web.Http.Controllers;
using System.Net.Http;

namespace WebAPI.Managers.Models
{
    public class KS
    {
        private const int BLOCK_SIZE = 16;
        private const int SHA1_SIZE = 20;
        private const string KS_FORMAT = "{0}&_t={1}&_e={2}&_u={3}&_d={4}";

        private string encryptedValue;
        private int groupId;
        private string userId;
        private KalturaSessionType userType;
        private DateTime expiration;
        private string privilege;
        private string data;

        public const string PAYLOAD_UDID = "UDID";

        public enum KSType
        {
            TVPAPI = 0,
            V2 = 1
        }

        public KSType ksType { get; private set; }

        public bool IsValid
        {
            get { return expiration > DateTime.UtcNow; }
        }

        public int GroupId
        {
            get { return groupId; }
        }

        public string UserId
        {
            get { return userId; }
            set
            {
                if (UserType == KalturaSessionType.ADMIN)
                    userId = value;
                else
                    throw new Exception("Unable to set userID without Admin KS");
            }
        }

        public KalturaSessionType UserType
        {
            get { return userType; }
        }

        public string Privilege
        {
            get { return privilege; }
        }

        public DateTime Expiration
        {
            get { return expiration; }
        }

        public string Data
        {
            get { return data; }
        }

        private KS()
        {
        }

        public KS(string secret, string groupID, string userID, int expiration, KalturaSessionType userType, string data, string privilege, KSType ksType)
        {
            int relativeExpiration = (int)SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow) + expiration;

            string ks = string.Format(KS_FORMAT, privilege, (int)userType, relativeExpiration, userID, !string.IsNullOrEmpty(data) ? HttpUtility.UrlEncode(data) : string.Empty);
            byte[] ksBytes = Encoding.ASCII.GetBytes(ks);
            byte[] randomBytes = createRandomByteArray(BLOCK_SIZE);
            byte[] randWithFields = new byte[ksBytes.Length + randomBytes.Length];
            Array.Copy(randomBytes, 0, randWithFields, 0, randomBytes.Length);
            Array.Copy(ksBytes, 0, randWithFields, randomBytes.Length, ksBytes.Length);

            byte[] signature = hashSHA1(randWithFields);
            byte[] input = new byte[signature.Length + randWithFields.Length];
            Array.Copy(signature, 0, input, 0, signature.Length);
            Array.Copy(randWithFields, 0, input, signature.Length, randWithFields.Length);

            byte[] encryptedFields = aesEncrypt(secret, input);
            string prefix = string.Format("v2|{0}|", groupID);

            byte[] output = new byte[encryptedFields.Length + prefix.Length];
            Array.Copy(Encoding.ASCII.GetBytes(prefix), 0, output, 0, prefix.Length);
            Array.Copy(encryptedFields, 0, output, prefix.Length, encryptedFields.Length);

            StringBuilder encodedKs = new StringBuilder(System.Convert.ToBase64String(output));
            encodedKs = encodedKs.Replace("+", "-");
            encodedKs = encodedKs.Replace("/", "_");
            encodedKs = encodedKs.Replace("\n", "");
            encodedKs = encodedKs.Replace("\r", "");

            encryptedValue = encodedKs.ToString();

            this.ksType = ksType;
        }

        public static KS CreateKSFromEncoded(byte[] encryptedData, int groupId, string secret, string ksVal, KSType ksType)
        {
            KS ks = new KS();
            ks.encryptedValue = ksVal;
            ks.groupId = groupId;

            // get string
            string encryptedDataStr = System.Text.Encoding.ASCII.GetString(encryptedData);

            // decrypt fields
            int fieldsWithRandomIndex = string.Format("v2|{0}|", groupId).Count();
            byte[] fieldsWithHashBytes = aesDecrypt(secret, encryptedData.Skip(fieldsWithRandomIndex).ToArray());

            // trim Right 0
            fieldsWithHashBytes = TrimRight(fieldsWithHashBytes);

            // check hash
            byte[] hash = fieldsWithHashBytes.Take(SHA1_SIZE).ToArray();
            byte[] fieldsWithRandom = fieldsWithHashBytes.Skip(SHA1_SIZE).ToArray();

            if (System.Text.Encoding.ASCII.GetString(hash) != System.Text.Encoding.ASCII.GetString(hashSHA1(fieldsWithRandom)))
            {
                throw new UnauthorizedException((int)StatusCode.InvalidKS, "Wrong KS format");
            }

            //parse fields
            string[] fields = System.Text.Encoding.ASCII.GetString(fieldsWithRandom.Skip(BLOCK_SIZE).ToArray()).Split("&_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length < 3)
            {
                throw new UnauthorizedException((int)StatusCode.InvalidKS, "Invalid KS");
            }

            ks.privilege = fields[0];

            for (int i = 1; i < fields.Length; i++)
            {
                string[] pair = fields[i].Split('=');
                if (pair == null || pair.Length != 2)
                {
                    throw new UnauthorizedException((int)StatusCode.InvalidKS, "Invalid KS");
                }

                switch (pair[0])
                {
                    case "t":
                        ks.userType = (KalturaSessionType)Enum.Parse(typeof(KalturaSessionType), pair[1]);
                        break;
                    case "e":
                        long expiration;
                        long.TryParse(pair[1], out expiration);
                        ks.expiration = SerializationUtils.ConvertFromUnixTimestamp(expiration);
                        break;
                    case "u":
                        ks.userId = pair[1];
                        break;
                    case "d":
                        ks.data = !string.IsNullOrEmpty(pair[1]) ? HttpUtility.UrlDecode(pair[1]) : string.Empty;
                        break;
                    default:
                        throw new UnauthorizedException((int)StatusCode.InvalidKS, "Invalid KS");
                }
            }

            return ks;
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

        private static byte[] aesDecrypt(string secretForSigning, byte[] text)
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

        public override string ToString()
        {
            return encryptedValue;
        }

        private static byte[] hashSHA1(string payload)
        {
            return hashSHA1(Encoding.UTF8.GetBytes(payload));
        }

        private static byte[] hashSHA1(byte[] payload)
        {
            var sha1 = SHA1Managed.Create();
            return sha1.ComputeHash(payload);
        }


        private static byte[] aesEncrypt(string secretForSigning, byte[] text)
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

        private byte[] createRandomByteArray(int size)
        {
            byte[] b = new byte[size];
            new Random().NextBytes(b);
            return b;
        }

        public static string preparePayloadData(List<KeyValuePair<string, string>> pairs)
        {
            return string.Join(";;", pairs.Select(x => string.Format("{0}={1}", x.Key, x.Value)));
        }

        internal void SaveOnRequest()
        {
            HttpContext.Current.Items.Add("KS", this);
        }

        internal static KS GetFromRequest()
        {
            return (KS)HttpContext.Current.Items["KS"];
        }

        public static KS CreateKSFromApiToken(ApiToken token)
        {
            KS ks = new KS()
            {
                groupId = token.GroupID,
                userId = token.UserId,
                userType = token.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                expiration = Utils.SerializationUtils.ConvertFromUnixTimestamp(token.AccessTokenExpiration),
            };

            return ks;
        }
    }
}