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
        private const string REPLACE_UNDERSCORE = "^^^";

        private string encryptedValue;
        private int groupId;
        private string userId;
        private KalturaSessionType sessionType;
        private DateTime expiration;
        private List<KalturaKeyValue> privileges;
        private string data;

        public enum KSVersion
        {
            TVPAPI = 0,
            V2 = 1
        }

        public class KSData
        {
            public string UDID { get; set; }
        }

        public KSVersion ksVersion { get; private set; }

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
                userId = value;
                //if (SessionType == KalturaSessionType.ADMIN)
                //    userId = value;
                //else
                //    throw new Exception("Unable to set userID without Admin KS");
            }
        }

        public KalturaSessionType SessionType
        {
            get { return sessionType; }
        }

        public List<KalturaKeyValue> Privileges
        {
            get { return privileges; }
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

        public KS(string secret, string groupID, string userID, int expiration, KalturaSessionType userType, string data, List<KalturaKeyValue> privilegesList, KSVersion ksType)
        {
            
            int relativeExpiration = (int)SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow) + expiration;

            //prepare data - url encode + replace '_'
            string encodedData = string.Empty;
            if (!string.IsNullOrEmpty(data))
            {
                encodedData = data.Replace("_", REPLACE_UNDERSCORE);
                encodedData = HttpUtility.UrlEncode(encodedData);
            }

            string ks = string.Format(KS_FORMAT, 
                privilegesList != null && privilegesList.Count > 0 ? string.Join(",", privilegesList.Select(p => string.Join(":", p.key, p.value))) : string.Empty, 
                (int)userType, relativeExpiration, userID, encodedData);
            byte[] ksBytes = Encoding.ASCII.GetBytes(ks);
            byte[] randomBytes = Utils.EncryptionUtils.CreateRandomByteArray(BLOCK_SIZE);
            byte[] randWithFields = new byte[ksBytes.Length + randomBytes.Length];
            Array.Copy(randomBytes, 0, randWithFields, 0, randomBytes.Length);
            Array.Copy(ksBytes, 0, randWithFields, randomBytes.Length, ksBytes.Length);

            byte[] signature = Utils.EncryptionUtils.HashSHA1(randWithFields);
            byte[] input = new byte[signature.Length + randWithFields.Length];
            Array.Copy(signature, 0, input, 0, signature.Length);
            Array.Copy(randWithFields, 0, input, signature.Length, randWithFields.Length);

            byte[] encryptedFields = Utils.EncryptionUtils.AesEncrypt(secret, input, BLOCK_SIZE);
            string prefix = string.Format("v2|{0}|", groupID);

            byte[] output = new byte[encryptedFields.Length + prefix.Length];
            Array.Copy(Encoding.ASCII.GetBytes(prefix), 0, output, 0, prefix.Length);
            Array.Copy(encryptedFields, 0, output, prefix.Length, encryptedFields.Length);

            StringBuilder encodedKs = new StringBuilder(System.Convert.ToBase64String(output));
            encodedKs = encodedKs.Replace("+", "-");
            encodedKs = encodedKs.Replace("/", "_");
            encodedKs = encodedKs.Replace("\n", "");
            encodedKs = encodedKs.Replace("\r", "");

            this.encryptedValue = encodedKs.ToString();
            this.ksVersion = ksType;
            this.data = data;
            this.expiration = DateTime.UtcNow.AddSeconds(expiration);
            this.groupId = int.Parse(groupID);
            this.privileges = privilegesList;
            this.sessionType = userType;
            this.userId = userID;
        }

        public static KS CreateKSFromEncoded(byte[] encryptedData, int groupId, string secret, string ksVal, KSVersion ksType)
        {
            KS ks = new KS();
            ks.encryptedValue = ksVal;
            ks.groupId = groupId;

            // get string
            string encryptedDataStr = System.Text.Encoding.ASCII.GetString(encryptedData);

            // decrypt fields
            int fieldsWithRandomIndex = string.Format("v2|{0}|", groupId).Count();
            byte[] fieldsWithHashBytes = Utils.EncryptionUtils.AesDecrypt(secret, encryptedData.Skip(fieldsWithRandomIndex).ToArray(), BLOCK_SIZE);

            // trim Right 0
            fieldsWithHashBytes = TrimRight(fieldsWithHashBytes);

            // check hash
            byte[] hash = fieldsWithHashBytes.Take(SHA1_SIZE).ToArray();
            byte[] fieldsWithRandom = fieldsWithHashBytes.Skip(SHA1_SIZE).ToArray();

            if (System.Text.Encoding.ASCII.GetString(hash) != System.Text.Encoding.ASCII.GetString(Utils.EncryptionUtils.HashSHA1(fieldsWithRandom)))
            {
                throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
            }

            //parse fields
            string fieldsString = System.Text.Encoding.ASCII.GetString(fieldsWithRandom.Skip(BLOCK_SIZE).ToArray());
            string[] fields = fieldsString.Split("&_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length < 3)
            {
                throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
            }

            ks.privileges = new List<KalturaKeyValue>();

            for (int i = 0; i < fields.Length; i++)
            {
                string[] pair = fields[i].Split('=');
                if (pair == null || pair.Length != 2)
                {
                    var privileges = fields[i].Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries);
                    foreach (var privilege in privileges)
                    {
                        pair = privilege.Split(':');
                        if (pair == null || pair.Length != 2)
                        {
                            ks.privileges.Add(new KalturaKeyValue() { key = pair[0], value = null });
                        }
                        else
                        {
                            ks.privileges.Add(new KalturaKeyValue() { key = pair[0], value = pair[1] });
                        }
                    }
                }
                else
                {
                    switch (pair[0])
                    {
                        case "t":
                            ks.sessionType = (KalturaSessionType)Enum.Parse(typeof(KalturaSessionType), pair[1]);
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
                            ks.data = string.Empty;
                            if (!string.IsNullOrEmpty(pair[1]))
                            {
                                ks.data = HttpUtility.UrlDecode(pair[1]);
                                ks.data = ks.data.Replace(REPLACE_UNDERSCORE, "_");
                            }
                            break;
                        default:
                            throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
                            break;
                    }
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

        

        public override string ToString()
        {
            return encryptedValue;
        }

        

        public static string preparePayloadData(List<KeyValuePair<string, string>> pairs)
        {
            return string.Join(";;", pairs.Select(x => string.Format("{0}={1}", x.Key, x.Value)));
        }

        public static List<KeyValuePair<string, string>> ExtractPayloadData(string payload)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrEmpty(payload))
            {
                foreach (var token in payload.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var t = token.Split('=');
                    pairs.Add(new KeyValuePair<string, string>(t[0], t[1]));
                }
            }

            return pairs;
        }

        internal void SaveOnRequest()
        {
            HttpContext.Current.Items.Add("KS", this);
        }

        public static void ClearOnRequest()
        {
            HttpContext.Current.Items.Remove("KS");
        }

        internal static void SaveOnRequest(KS ks)
        {
            if (HttpContext.Current.Items.Contains("KS"))
                HttpContext.Current.Items["KS"] = ks;
            else
                HttpContext.Current.Items.Add("KS", ks);
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
                sessionType = token.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                expiration = Utils.SerializationUtils.ConvertFromUnixTimestamp(token.AccessTokenExpiration),
                data = KSUtils.PrepareKSPayload(new WebAPI.Managers.Models.KS.KSData() { UDID = token.Udid })
            };

            return ks;
        }
    }
}