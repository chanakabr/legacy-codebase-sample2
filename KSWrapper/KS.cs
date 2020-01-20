using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        public const string INVALID_KS_FORMAT = "Invalid KS format";

        private const int BLOCK_SIZE = 16;
        private const int SHA1_SIZE = 20;
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
        public string Random { get; private set; }
        public string OriginalUserId { get; private set; }

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

        public KS(string ks, int groupId, byte[] encryptedData, string adminSecret, string fallbackSecret)
        {
            this.ksVersion = KSVersion.V2;
            this.EncryptedValue = ks;
            this.GroupId = groupId;

            // decrypt fields
            int fieldsWithRandomIndex = string.Format("v2|{0}|", groupId).Count();
            byte[] fieldsWithHashBytes = EncryptUtils.AesDecrypt(adminSecret, encryptedData.Skip(fieldsWithRandomIndex).ToArray(), BLOCK_SIZE);

            // trim Right 0
            fieldsWithHashBytes = TrimRight(fieldsWithHashBytes);

            // check hash
            byte[] hash = fieldsWithHashBytes.Take(SHA1_SIZE).ToArray();
            byte[] fieldsWithRandom = fieldsWithHashBytes.Skip(SHA1_SIZE).ToArray();

            if (Encoding.ASCII.GetString(hash) != Encoding.ASCII.GetString(EncryptUtils.HashSHA1(fieldsWithRandom)))
            {
                if (!string.IsNullOrEmpty(fallbackSecret))
                {
                    fieldsWithHashBytes = EncryptUtils.AesDecrypt(fallbackSecret, encryptedData.Skip(fieldsWithRandomIndex).ToArray(), BLOCK_SIZE);

                    // trim Right 0
                    fieldsWithHashBytes = TrimRight(fieldsWithHashBytes);

                    // check hash
                    hash = fieldsWithHashBytes.Take(SHA1_SIZE).ToArray();
                    fieldsWithRandom = fieldsWithHashBytes.Skip(SHA1_SIZE).ToArray();
                    if (Encoding.ASCII.GetString(hash) != Encoding.ASCII.GetString(EncryptUtils.HashSHA1(fieldsWithRandom)))
                    {
                        throw new FormatException(INVALID_KS_FORMAT);
                    }
                }
                else
                {
                    throw new FormatException(INVALID_KS_FORMAT);
                }
            }

            //parse fields
            this.Random = string.Concat(Array.ConvertAll(fieldsWithRandom.Take(BLOCK_SIZE).ToArray(), b => b.ToString("X2"))); // byte array to hex string
            string fieldsString = Encoding.ASCII.GetString(fieldsWithRandom.Skip(BLOCK_SIZE).ToArray());
            string[] fields = fieldsString.Split("&_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length < 3)
            {
                throw new FormatException(INVALID_KS_FORMAT);
            }

            this.Privileges = new Dictionary<string, string>();

            for (int i = 0; i < fields.Length; i++)
            {
                string[] pair = fields[i].Split('=');
                if (pair.Length != 2)
                {
                    this.Privileges.Add(pair[0], null);
                }
                else
                {
                    switch (pair[0])
                    {
                        case "t":
                            int sessionType;
                            int.TryParse(pair[1], out sessionType);
                            this.SessionType = sessionType;
                            break;
                        case "e":
                            long expiration;
                            long.TryParse(pair[1], out expiration);
                            this.Expiration = DateUtils.UtcUnixTimestampSecondsToDateTime(expiration);
                            break;
                        case "u":
                            this.UserId = pair[1];
                            break;
                        case "d":
                            this.Data = string.Empty;
                            if (!string.IsNullOrEmpty(pair[1]))
                            {
                                this.Data = HttpUtility.UrlDecode(pair[1]);
                                this.Data = this.Data.Replace(REPLACE_UNDERSCORE, "_");
                            }
                            break;
                        default:
                            this.Privileges.Add(pair[0], pair[1]);
                            break;
                    }
                }
            }
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

        public KSData ExtractKSData()
        {
            var pairs = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(this.Data))
            {
                foreach (var token in this.Data.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var matchPair = Regex.Match(token, @"^([\w\d]+)(\=)([^;]+)", RegexOptions.IgnoreCase);
                    if (matchPair.Success && matchPair.Groups.Count == 4)
                    {
                        var key = matchPair.Groups[1].Value;
                        var value = matchPair.Groups[3].Value;
                        if (!pairs.ContainsKey(key))
                        {
                            pairs.Add(key, value);
                        }
                    }
                }
            }

            return new KSData(pairs);
        }

        private byte[] TrimRight(byte[] arr)
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
    }
}
