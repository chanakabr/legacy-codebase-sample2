using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using KLogMonitor;
using TVinciShared;
using ApiObjects.Base;
using WebAPI.Utils;
using System.Text.RegularExpressions;
using WebAPI.ClientManagers;

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
        private Dictionary<string, string> privileges;
        private string data;

        public enum KSVersion
        {
            TVPAPI = 0,
            V2 = 1
        }

        public class KSData
        {
            public string UDID { get; set; }
            public int CreateDate { get; set; }
            public int RegionId { get; set; }
            public List<long> UserSegments { get; set; }
            public List<long> UserRoles { get; set; }
            public string Signature { get; set; }

            public KSData()
            {
            }

            public KSData(string udid, int createDate, int regionId, List<long> userSegments, List<long> userRoles)
            {
                this.UDID = udid;
                this.CreateDate = createDate;
                this.RegionId = regionId;
                this.UserSegments = userSegments;
                this.UserRoles = userRoles;
            }

            public KSData(KSData payload, int createDate)
            {
                this.CreateDate = createDate;
                this.UDID = payload.UDID;
                this.RegionId = payload.RegionId;
                this.UserSegments = payload.UserSegments;
                this.UserRoles = payload.UserRoles;
            }

            public KSData(ApiToken token, int createDate, string udid)
            {
                this.CreateDate = createDate;
                this.UDID = udid;
                this.RegionId = token.RegionId;
                this.UserSegments = token.UserSegments;
                this.UserRoles = token.UserRoles;
            }
        }

        public KSVersion ksVersion { get; private set; }

        public bool IsValid
        {
            get { return AuthorizationManager.IsKsValid(this); }
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

        public string Random { get; set; }

        public string OriginalUserId { get; set; }

        public KalturaSessionType SessionType
        {
            get { return sessionType; }
        }

        public Dictionary<string, string> Privileges
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

        private string ConvertSignature(byte[] randomBytes, int groupID)
        {
            var group = GroupsManager.GetGroup(groupID);
            if (!group.EnforceGroupsSecret)
            {
                return string.Empty;
            }
            var secret = group.GroupSecrets?.LastOrDefault();
            if (string.IsNullOrEmpty(secret))
            {
                return string.Empty;
            }
            var random = Encoding.Default.GetString(randomBytes);
            var concat = string.Format(group.SignatureFormat, random, secret);
            return Encoding.Default.GetString(EncryptionUtils.HashSHA1(concat));
        }

        public KS(string secret, string groupID, string userID, int expiration, KalturaSessionType userType, KSData data, Dictionary<string, string> privilegesList, KSVersion ksType)
        {
            byte[] randomBytes = Utils.EncryptionUtils.CreateRandomByteArray(BLOCK_SIZE);
            int relativeExpiration = (int)DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow) + expiration;
            //prepare data - url encode + replace '_'
            string encodedData = string.Empty;
            var payload = string.Empty;
            if (data != null)
            {
                data.Signature = ConvertSignature(randomBytes, Convert.ToInt32(groupID));
                payload = KSUtils.PrepareKSPayload(data);
                encodedData = payload.Replace("_", REPLACE_UNDERSCORE);
                encodedData = HttpUtility.UrlEncode(encodedData);
            }

            string ks = string.Format(KS_FORMAT, JoinPrivileges(privilegesList), (int)userType, relativeExpiration, userID, encodedData);
            byte[] ksBytes = Encoding.ASCII.GetBytes(ks);

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

            StringBuilder encodedKs = new StringBuilder(Convert.ToBase64String(output));
            encodedKs = encodedKs.Replace("+", "-");
            encodedKs = encodedKs.Replace("/", "_");
            encodedKs = encodedKs.Replace("\n", "");
            encodedKs = encodedKs.Replace("\r", "");

            this.encryptedValue = encodedKs.ToString();
            this.ksVersion = ksType;
            this.data = payload;
            this.expiration = DateTime.UtcNow.AddSeconds(expiration);
            this.groupId = int.Parse(groupID);
            this.privileges = privilegesList;
            this.sessionType = userType;
            this.userId = userID;
        }

        public static string JoinPrivileges(Dictionary<string, string> privileges, string valuesSeperator = "&_", string inValueSeperator = "=")
        {
            if (privileges == null || privileges.Count == 0)
                return string.Empty;

            return string.Join(valuesSeperator, privileges.Select(p => string.Join(inValueSeperator, p.Key, (p.Value == null ? string.Empty : p.Value))));
        }

        public static KS CreateKSFromEncoded(byte[] encryptedData, int groupId, string secret, string ksVal, KSVersion ksType, string secretFallback = null)
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
                if (!string.IsNullOrEmpty(secretFallback))
                {
                    fieldsWithHashBytes = Utils.EncryptionUtils.AesDecrypt(secretFallback, encryptedData.Skip(fieldsWithRandomIndex).ToArray(), BLOCK_SIZE);

                    // trim Right 0
                    fieldsWithHashBytes = TrimRight(fieldsWithHashBytes);

                    // check hash
                    hash = fieldsWithHashBytes.Take(SHA1_SIZE).ToArray();
                    fieldsWithRandom = fieldsWithHashBytes.Skip(SHA1_SIZE).ToArray();
                    if (System.Text.Encoding.ASCII.GetString(hash) != System.Text.Encoding.ASCII.GetString(Utils.EncryptionUtils.HashSHA1(fieldsWithRandom)))
                    {
                        throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
                    }
                }
                else
                {
                    throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
                }
            }

            //parse fields
            ks.Random = string.Concat(Array.ConvertAll(fieldsWithRandom.Take(BLOCK_SIZE).ToArray(), b => b.ToString("X2"))); // byte array to hex string
            string fieldsString = System.Text.Encoding.ASCII.GetString(fieldsWithRandom.Skip(BLOCK_SIZE).ToArray());
            string[] fields = fieldsString.Split("&_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length < 3)
            {
                throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
            }

            ks.privileges = new Dictionary<string, string>();

            for (int i = 0; i < fields.Length; i++)
            {
                string[] pair = fields[i].Split('=');
                if (pair.Length != 2)
                {
                    ks.privileges.Add(pair[0], null);
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
                            ks.expiration = DateUtils.UtcUnixTimestampSecondsToDateTime(expiration);
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
                            ks.privileges.Add(pair[0], pair[1]);
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

        public static Dictionary<string, string> ExtractPayloadData(string payload)
        {
            var pairs = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(payload))
            {
                foreach (var token in payload.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries))
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

            return pairs;
        }

        internal void SaveOnRequest()
        {
            HttpContext.Current.Items[Constants.GROUP_ID] = groupId;
            HttpContext.Current.Items.Add(RequestContext.REQUEST_GROUP_ID, groupId);
            HttpContext.Current.Items.Add(RequestContext.REQUEST_KS, this);
        }

        public static void ClearOnRequest()
        {
            HttpContext.Current.Items.Remove(RequestContext.REQUEST_GROUP_ID);
            HttpContext.Current.Items.Remove(RequestContext.REQUEST_KS);
        }

        internal static void SaveOnRequest(KS ks)
        {
            if (HttpContext.Current.Items.ContainsKey(RequestContext.REQUEST_KS))
                HttpContext.Current.Items[RequestContext.REQUEST_KS] = ks;
            else
                HttpContext.Current.Items.Add(RequestContext.REQUEST_KS, ks);

            if (HttpContext.Current.Items.ContainsKey(RequestContext.REQUEST_GROUP_ID))
                HttpContext.Current.Items[RequestContext.REQUEST_GROUP_ID] = ks.groupId;
            else
                HttpContext.Current.Items.Add(RequestContext.REQUEST_GROUP_ID, ks.groupId);
        }

        internal static KS GetFromRequest()
        {
            return (KS)HttpContext.Current.Items[RequestContext.REQUEST_KS];
        }

        public static KS CreateKSFromApiToken(ApiToken token, string tokenVal)
        {
            KS ks = new KS()
            {
                groupId = token.GroupID,
                userId = token.UserId,
                expiration = DateUtils.UtcUnixTimestampSecondsToDateTime(token.AccessTokenExpiration),
                sessionType = token.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                data = KSUtils.PrepareKSPayload(new KS.KSData(token, 0, token.Udid)),
                encryptedValue = tokenVal
            };

            return ks;
        }

        public static KS ParseKS(string ks)
        {
            StringBuilder sb = new StringBuilder(ks);
            sb = sb.Replace("-", "+");
            sb = sb.Replace("_", "/");

            int groupId = 0;
            byte[] encryptedData = null;
            string encryptedDataStr = null;
            string[] ksParts = null;

            try
            {
                encryptedData = System.Convert.FromBase64String(sb.ToString());
                encryptedDataStr = System.Text.Encoding.ASCII.GetString(encryptedData);
                ksParts = encryptedDataStr.Split('|');
            }
            catch (Exception)
            {
                throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
            }

            if (ksParts.Length < 3 || ksParts[0] != "v2" || !int.TryParse(ksParts[1], out groupId))
            {
                throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
            }

            Group group = WebAPI.ClientManagers.GroupsManager.GetGroup(groupId);
            string adminSecret = group.UserSecret;

            // build KS
            string fallbackSecret = group.UserSecretFallbackExpiryEpoch > DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow) ? group.UserSecretFallback : null;
            return KS.CreateKSFromEncoded(encryptedData, groupId, adminSecret, ks, KS.KSVersion.V2, fallbackSecret);
        }

        public static ContextData GetContextData()
        {
            var ks = GetFromRequest();
            long? domainId = null, userId = null;

            try
            {
                domainId = HouseholdUtils.GetHouseholdIDByKS();
            }
            catch (Exception) { }

            try
            {
                userId = Utils.Utils.GetUserIdFromKs(ks);
            }
            catch (Exception) { }

            var contextData = new ContextData(ks.GroupId)
            {
                DomainId = domainId,
                UserId = userId
            };

            return contextData;
        }
    }
}