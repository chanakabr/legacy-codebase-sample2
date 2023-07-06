using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ApiObjects.Base;
using Phx.Lib.Appconfig;
using KalturaRequestContext;
using Phx.Lib.Log;
using TVinciShared;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Managers.Models
{
    public class KS
    {
        private static int AccessTokenLength = ApplicationConfiguration.Current.RequestParserConfiguration.AccessTokenLength.Value;

        private const int BLOCK_SIZE = 16;
        private const int SHA1_SIZE = 20;
        private const string KS_FORMAT = "{0}&_t={1}&_e={2}&_u={3}&_d={4}";
        private const string REPLACE_UNDERSCORE = "^^^";

        private string encryptedValue;
        private string userId;

        public bool IsKsFormat => HasKsFormat(encryptedValue);

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
            public string SessionCharacteristicKey { get; }
            public int DomainId { get; set; }
            public string Signature { get; set; }
            public BypassCacheEligibility? BypassCacheEligibility { get; set; }

            public static KSData Empty { get; } = new KSData();

            private KSData(){}

            public KSData(
                string udid,
                int createDate,
                int regionId,
                List<long> userSegments,
                List<long> userRoles,
                string sessionCharacteristicKey,
                int domainId,
                BypassCacheEligibility? bypassCacheEligibility,
                string signature = "")
            {
                UDID = udid;
                CreateDate = createDate;
                DomainId = domainId;
                RegionId = regionId;
                UserSegments = userSegments;
                UserRoles = userRoles;
                SessionCharacteristicKey = sessionCharacteristicKey;
                Signature = signature;
                BypassCacheEligibility = bypassCacheEligibility;
            }

            public KSData(KSData payload, int createDate)
            {
                CreateDate = createDate;
                UDID = payload.UDID;
                DomainId = payload.DomainId;
                RegionId = payload.RegionId;
                UserSegments = payload.UserSegments;
                UserRoles = payload.UserRoles;
                SessionCharacteristicKey = payload.SessionCharacteristicKey;
                BypassCacheEligibility = payload.BypassCacheEligibility;
            }

            public KSData(ApiToken token, int createDate, string udid)
            {
                CreateDate = createDate;
                UDID = udid;
                DomainId = token.DomainId;
                RegionId = token.RegionId;
                UserSegments = token.UserSegments;
                UserRoles = token.UserRoles;
                SessionCharacteristicKey = token.SessionCharacteristicKey;
                BypassCacheEligibility = token.BypassCacheEligibility;
            }
        }

        public KSVersion ksVersion { get; private set; }

        public bool IsValid => AuthorizationManager.IsKsValid(this);

        public int GroupId { get; private set; }

        public string UserId
        {
            get => userId;
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

        public KalturaSessionType SessionType { get; private set; }

        public Dictionary<string, string> Privileges { get; private set; }

        public DateTime Expiration { get; private set; }

        public string Data { get; private set; }

        private KS()
        {
        }

        private string ConvertSignature(byte[] randomBytes)
        {
            var secrets = ApplicationConfiguration.Current.RequestParserConfiguration.KsSecrets;
            var secret = secrets.FirstOrDefault();
            var random = Encoding.Default.GetString(randomBytes);
            var concat = string.Format(EncryptionUtils.SignatureFormat, random, secret);
            return Encoding.Default.GetString(EncryptionUtils.HashSHA1(concat));
        }

        public KS(string secret, string groupID, string userID, int expiration, KalturaSessionType userType, KSData data, Dictionary<string, string> privilegesList, KSVersion ksType)
        {
            byte[] randomBytes = EncryptionUtils.CreateRandomByteArray(BLOCK_SIZE);
            int relativeExpiration = (int)DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow) + expiration;
            //prepare data - url encode + replace '_'
            string encodedData = string.Empty;
            var payload = string.Empty;
            if (data != null)
            {
                data.Signature = ConvertSignature(randomBytes);
                payload = KSUtils.PrepareKSPayload(data);
                encodedData = payload.Replace("_", REPLACE_UNDERSCORE);
                encodedData = HttpUtility.UrlEncode(encodedData);
            }

            string ks = string.Format(KS_FORMAT, JoinPrivileges(privilegesList), (int)userType, relativeExpiration, userID, encodedData);
            byte[] ksBytes = Encoding.ASCII.GetBytes(ks);

            byte[] randWithFields = new byte[ksBytes.Length + randomBytes.Length];
            Array.Copy(randomBytes, 0, randWithFields, 0, randomBytes.Length);
            Array.Copy(ksBytes, 0, randWithFields, randomBytes.Length, ksBytes.Length);

            byte[] signature = EncryptionUtils.HashSHA1(randWithFields);
            byte[] input = new byte[signature.Length + randWithFields.Length];
            Array.Copy(signature, 0, input, 0, signature.Length);
            Array.Copy(randWithFields, 0, input, signature.Length, randWithFields.Length);

            byte[] encryptedFields = EncryptionUtils.AesEncrypt(secret, input, BLOCK_SIZE);
            var prefix = $"v2|{groupID}|";

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
            this.Data = payload;
            this.Expiration = DateTime.UtcNow.AddSeconds(expiration);
            this.GroupId = int.Parse(groupID);
            this.Privileges = privilegesList;
            this.SessionType = userType;
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
            ks.GroupId = groupId;

            // get string
            string encryptedDataStr = Encoding.ASCII.GetString(encryptedData);

            // decrypt fields
            int fieldsWithRandomIndex = string.Format("v2|{0}|", groupId).Count();
            var encrypted = encryptedData.Skip(fieldsWithRandomIndex).ToArray();
            byte[] fieldsWithHashBytes = EncryptionUtils.AesDecrypt(secret, encrypted, BLOCK_SIZE);

            // trim Right 0
            fieldsWithHashBytes = TrimRight(fieldsWithHashBytes);

            // check hash
            var hash = Encoding.ASCII.GetString(fieldsWithHashBytes.Take(SHA1_SIZE).ToArray());
            byte[] fieldsWithRandom = fieldsWithHashBytes.Skip(SHA1_SIZE).ToArray();
            var fieldsHash = Encoding.ASCII.GetString(EncryptionUtils.HashSHA1(fieldsWithRandom));
            if (hash != fieldsHash)
            {
                if (!string.IsNullOrEmpty(secretFallback))
                {
                    fieldsWithHashBytes = EncryptionUtils.AesDecrypt(secretFallback, encrypted, BLOCK_SIZE);

                    // trim Right 0
                    fieldsWithHashBytes = TrimRight(fieldsWithHashBytes);

                    // check hash
                    hash = Encoding.ASCII.GetString(fieldsWithHashBytes.Take(SHA1_SIZE).ToArray());
                    fieldsWithRandom = fieldsWithHashBytes.Skip(SHA1_SIZE).ToArray();
                    fieldsHash = Encoding.ASCII.GetString(EncryptionUtils.HashSHA1(fieldsWithRandom));
                    if (hash != fieldsHash)
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
            string fieldsString = Encoding.ASCII.GetString(fieldsWithRandom.Skip(BLOCK_SIZE).ToArray());
            string[] fields = fieldsString.Split("&_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length < 3)
            {
                throw new UnauthorizedException(UnauthorizedException.INVALID_KS_FORMAT);
            }

            ks.Privileges = new Dictionary<string, string>();

            for (int i = 0; i < fields.Length; i++)
            {
                string[] pair = fields[i].Split('=');
                if (pair.Length != 2)
                {
                    ks.Privileges.Add(pair[0], null);
                }
                else
                {
                    switch (pair[0])
                    {
                        case "t":
                            ks.SessionType = (KalturaSessionType)Enum.Parse(typeof(KalturaSessionType), pair[1]);
                            break;
                        case "e":
                            long expiration;
                            long.TryParse(pair[1], out expiration);
                            ks.Expiration = DateUtils.UtcUnixTimestampSecondsToDateTime(expiration);
                            break;
                        case "u":
                            ks.userId = pair[1];
                            break;
                        case "d":
                            ks.Data = string.Empty;
                            if (!string.IsNullOrEmpty(pair[1]))
                            {
                                ks.Data = HttpUtility.UrlDecode(pair[1]);
                                ks.Data = ks.Data.Replace(REPLACE_UNDERSCORE, "_");
                            }
                            break;
                        default:
                            ks.Privileges.Add(pair[0], pair[1]);
                            break;
                    }
                }
            }

            return ks;
        }

        public static bool HasKsFormat(string value)
        {
            return value.Length > AccessTokenLength;
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
            HttpContext.Current.Items[Constants.GROUP_ID] = GroupId;
            HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_GROUP_ID, GroupId);
            HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_KS, this);
            var payload = GetPayload();
            RequestContextUtilsInstance.Setter().SetKsPayload(payload.RegionId, payload.SessionCharacteristicKey);
        }

        public static void ClearOnRequest()
        {
            HttpContext.Current.Items.Remove(RequestContextConstants.REQUEST_GROUP_ID);
            HttpContext.Current.Items.Remove(RequestContextConstants.REQUEST_KS);
            RequestContextUtilsInstance.Setter().RemoveKsPayload();
        }

        internal static void SaveOnRequest(KS ks)
        {
            if (HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_KS))
                HttpContext.Current.Items[RequestContextConstants.REQUEST_KS] = ks;
            else
                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_KS, ks);

            if (HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_GROUP_ID))
                HttpContext.Current.Items[RequestContextConstants.REQUEST_GROUP_ID] = ks.GroupId;
            else
                HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_GROUP_ID, ks.GroupId);

            if (!string.IsNullOrEmpty(ks.OriginalUserId) && ks.OriginalUserId != ks.userId && long.TryParse(ks.OriginalUserId, out long originalUserId))
            {
                if (HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_KS_ORIGINAL_USER_ID))
                    HttpContext.Current.Items[RequestContextConstants.REQUEST_KS_ORIGINAL_USER_ID] = originalUserId;
                else
                    HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_KS_ORIGINAL_USER_ID, originalUserId);
            }

            var payload = GetPayload();
            RequestContextUtilsInstance.Setter().SetKsPayload(payload.RegionId, payload.SessionCharacteristicKey);
        }

        internal static KS GetFromRequest()
        {
            if (HttpContext.Current == null) return null;

            var items = HttpContext.Current.Items;
            return items.ContainsKey(RequestContextConstants.REQUEST_KS)
                ? (KS)items[RequestContextConstants.REQUEST_KS]
                : null;
        }

        public static KS CreateKSFromApiToken(ApiToken token, string tokenVal)
        {
            var ks = new KS
            {
                GroupId = token.GroupID,
                userId = token.UserId,
                Expiration = DateUtils.UtcUnixTimestampSecondsToDateTime(token.AccessTokenExpiration),
                SessionType = token.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                Data = KSUtils.PrepareKSPayload(new KSData(token, 0, token.Udid)),
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
                encryptedData = Convert.FromBase64String(sb.ToString());
                encryptedDataStr = Encoding.ASCII.GetString(encryptedData);
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

            Group group = GroupsManager.Instance.GetGroup(groupId);
            string adminSecret = group.UserSecret;

            // build KS
            string fallbackSecret = group.UserSecretFallbackExpiryEpoch > DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow) ? group.UserSecretFallback : null;
            return CreateKSFromEncoded(encryptedData, groupId, adminSecret, ks, KSVersion.V2, fallbackSecret);
        }

        public static ContextData GetContextData(bool skipDomain = false)
        {
            var ks = GetFromRequest();
            if (ks == null)
            {
                return null;
            }

            var payload = GetPayload();

            return new ContextData(ks.GroupId)
            {
                DomainId = GetDomainId(skipDomain),
                UserId = Utils.Utils.GetUserIdFromKs(ks),
                Udid = payload?.UDID,
                UserIp = Utils.Utils.GetClientIP(),
                Language = Utils.Utils.GetLanguageFromRequest(),
                Format = Utils.Utils.GetFormatFromRequest(),
                OriginalUserId = long.TryParse(ks.OriginalUserId, out var originalUserId) && originalUserId > 0 ? originalUserId : (long?)null,
                RegionId = payload?.RegionId > 0 ? payload.RegionId : (long?)null,
                SessionCharacteristicKey = payload?.SessionCharacteristicKey,
                UserRoleIds = payload?.UserRoles
            };
        }

        public bool IsImpersonatedRequest()
        {
            return !OriginalUserId.IsNullOrEmpty();
        }

        private static KSData GetPayload()
        {
            try
            {
                return KSUtils.ExtractKSPayload();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static long? GetDomainId(bool skipDomain)
        {
            if (skipDomain)
            {
                return null;
            }

            try
            {
                return HouseholdUtils.GetHouseholdIDByKS();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
