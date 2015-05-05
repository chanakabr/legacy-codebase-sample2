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
        private const string KS_FORMAT = "{0}&_t={1}&_e={2}&_u={3}";

        private bool isValid;
        private string encryptedValue;
        private int groupId;
        private int userId;
        private eUserType userType;
        private long expiration;
        private string privilege;

        public enum eUserType { USER = 0, ADMIN = 2 }

        public bool IsValid
        {
            get { return isValid; }
        }

        public int GroupId
        {
            get { return groupId; }
        }

        public int UserId
        {
            get { return userId; }
        }

        public eUserType UserType
        {
            get { return userType; }
        }

        public string Privilege
        {
            get { return privilege; }
        }

        public long Expiration
        {
            get { return expiration; }
        }

        public string Privilege
        {
            get { return privilege; }
        }

        private KS()
        {
        }

        public KS(string adminSecret, string groupID, string userID, int expiration, eUserType userType)
        {
            int relativeExpiration = (int)SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow) + expiration;

            string ks = string.Format(KS_FORMAT, "all=*", (int)userType, relativeExpiration, userID);
            byte[] ksBytes = Encoding.UTF8.GetBytes(ks);
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

            StringBuilder encodedKs = new StringBuilder(System.Convert.ToBase64String(output));
            encodedKs = encodedKs.Replace("+", "-");
            encodedKs = encodedKs.Replace("/", "_");
            encodedKs = encodedKs.Replace("\n", "");
            encodedKs = encodedKs.Replace("\r", "");

            encryptedValue = encodedKs.ToString();
        }

        public static KS CreateKSFromEncoded(string base64Value)
        {
            KS ks = null;

            if (string.IsNullOrEmpty(base64Value))
            {
                return null;
            }

            StringBuilder sb = new StringBuilder(base64Value);
            sb = sb.Replace("-", "+");
            sb = sb.Replace("_", "/");
            byte[] encryptedData = System.Convert.FromBase64String(sb.ToString());

            string value = System.Text.Encoding.UTF8.GetString(encryptedData);

            string[] ksParts = value.Split('|');

            if (ksParts.Length != 3 || ksParts[0] != "v2")
            {
                return null;
            }

            ks = new KS();

            // parse group id
            if (!int.TryParse(ksParts[1], out ks.groupId))
            {
                return null;
            }

            // get group secret
            Group group = GroupsManager.GetGroup(ks.groupId);
            string adminSecret = group.AdminSecret;

            // decrypt fields
            byte[] encryptedFieldsBytes = Encoding.UTF8.GetBytes(ksParts[2]);
            byte[] fieldsBytes = aesDecrypt(adminSecret, encryptedFieldsBytes);

            //parse fields
            string[] fields = System.Text.Encoding.UTF8.GetString(fieldsBytes).Split("&_".ToCharArray());

            if (fields == null || fields.Length != 4)
            {
                return null;
            }

            ks.privilege = fields[0];

            for (int i = 1; i < fields.Length; i++)
            {
                string[] pair = fields[i].Split('=');
                if (pair == null || pair.Length != 2)
                {
                    return null;
                }

                switch (pair[0])
                {
                    case "t":
                        ks.userType = (eUserType)Enum.Parse(typeof(eUserType), pair[1]);
                        break;
                    case "e":
                        int expiration;
                        int.TryParse(pair[1], out expiration);
                        ks.expiration = expiration;
                        break;
                    case "u":
                        int user;
                        int.TryParse(pair[1], out user);
                        ks.userId = user;
                        break;
                    default:
                        return null;
                        break;
                }
            }

            ks.isValid = true;

            return ks;
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

            // Encrypt
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
                        cst.Write(textAsBytes, 0, textSize);
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