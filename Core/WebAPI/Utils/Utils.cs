using ApiObjects.User;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using ApiLogic.Catalog;
using KalturaRequestContext;
using TVinciShared;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Utils
{
    public class Utils
    {
        private static readonly HashSet<string> defaultResponseProfileProperties = new HashSet<string> {"error"};

        internal static int GetLanguageId(int groupId, string language)
        {
            // get all group languages
            List<Language> languages = GetGroupLanguages();
            if (languages == null)
            {
                return 0;
            }

            // get default/specific language
            Language langModel = new Language();
            if (string.IsNullOrEmpty(language))
                langModel = languages.Where(l => l.IsDefault).FirstOrDefault();
            else
                langModel = languages.Where(l => l.Code == language).FirstOrDefault();

            if (langModel != null)
                return langModel.Id;
            else
                return 0;
        }

        public static string GetClientIP()
        {
            if (HttpContext.Current.Items[RequestContextConstants.USER_IP] != null)
            {
                return HttpContext.Current.Items[RequestContextConstants.USER_IP].ToString();
            }

            string ip = string.Empty;
            string retIp = HttpContext.Current.Request.GetForwardedForHeader();
            string[] ipRange;

            if (!string.IsNullOrEmpty(retIp) && (ipRange = retIp.Split(',')) != null && ipRange.Length > 0)
            {
                ip = ipRange[0];
            }
            else
            {
                ip = HttpContext.Current.Request.GetRemoteAddress();
            }

            if (ip.Equals("127.0.0.1") || ip.Equals("::1") || ip.StartsWith("192.168.")) ip = "81.218.199.175";

            // Azur
            // when header contains :, it can be of two: either it has a port, or it is an IPv6.
            if (ip.Contains(':'))
            {
                IPAddress address;

                if (IPAddress.TryParse(ip, out address))
                {
                    ip = address.ToString();
                }
                else
                {
                    ip = ip.Substring(0, ip.IndexOf(':'));
                }
            }

            return ip.Trim();
        }

        public static string Generate32LengthGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        internal static string GetLanguageFromRequest()
        {
            if (HttpContext.Current.Items[RequestContextConstants.REQUEST_LANGUAGE] == null)
            {
                return null;
            }

            return HttpContext.Current.Items[RequestContextConstants.REQUEST_LANGUAGE].ToString();
        }

        public static int? GetGroupIdFromRequest()
        {
            if (HttpContext.Current.Items[RequestContextConstants.REQUEST_GROUP_ID] == null)
            {
                return null;
            }

            return (int) HttpContext.Current.Items[RequestContextConstants.REQUEST_GROUP_ID];
        }

        internal static string GetDefaultLanguage()
        {
            int? groupId = GetGroupIdFromRequest();
            if (!groupId.HasValue)
            {
                return null;
            }

            // get all group languages
            List<Language> languages = GetGroupLanguages();
            if (languages == null || languages.Count == 0)
            {
                return null;
            }

            Language langModel = languages.FirstOrDefault(l => l.IsDefault);
            if (langModel != null)
            {
                return langModel.Code;
            }

            return null;
        }

        internal static HashSet<string> GetGroupLanguageCodes()
        {
            HashSet<string> languageCodes = new HashSet<string>();
            List<Language> languages = GetGroupLanguages();
            if (languages != null)
            {
                foreach (Language lng in languages)
                {
                    if (!languageCodes.Contains(lng.Code))
                    {
                        languageCodes.Add(lng.Code);
                    }
                }
            }

            return languageCodes;
        }

        internal static List<Language> GetGroupLanguages()
        {
            int? groupId = GetGroupIdFromRequest();

            if (!groupId.HasValue)
            {
                return null;
            }

            if (Core.Catalog.CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId.Value))
            {
                Core.Catalog.CatalogGroupCache groupCache;

                if (Core.Catalog.CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId.Value, out groupCache))
                {
                    return Mapper.Map<List<Language>>(groupCache.LanguageMapById.Values.ToList());
                }
            }
            else
            {
                GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();

                return Mapper.Map<List<Language>>(groupManager.GetGroup(groupId.Value).GetLangauges());
            }

            return null;
        }

        internal static string GetCurrencyFromRequest()
        {
            var currency = HttpContext.Current.Items[RequestContextConstants.REQUEST_CURRENCY];
            return currency != null ? currency.ToString() : null;
        }

        internal static string GetFormatFromRequest()
        {
            var format = HttpContext.Current.Items[RequestContextConstants.REQUEST_FORMAT];
            return format != null ? format.ToString() : null;
        }

        internal static WebAPI.Models.General.KalturaBaseResponseProfile GetResponseProfileFromRequest()
        {
            KalturaBaseResponseProfile responseProfile = (KalturaBaseResponseProfile)HttpContext.Current.Items[RequestContextConstants.REQUEST_RESPONSE_PROFILE];

            return responseProfile != null ? responseProfile as WebAPI.Models.General.KalturaBaseResponseProfile : null;
        }

        public static string GetCurrentBaseUrl()
        {
            string xForwardedProtoHeader = HttpContext.Current.Request.Headers["X-Forwarded-Proto"];
            string xKProxyProto = HttpContext.Current.Request.Headers["X-KProxy-Proto"];

            string baseUrl = string.Format("{0}://{1}{2}", (!string.IsNullOrEmpty(xForwardedProtoHeader) && xForwardedProtoHeader == "https") ||
                (!string.IsNullOrEmpty(xKProxyProto) && xKProxyProto == "https") ?
                "https" : HttpContext.Current.Request.GetUrl().Scheme, ExtractHost(), HttpContext.Current.Request.GetApplicationPath().TrimEnd('/'));
            return baseUrl;
        }

        private static string ExtractHost()
        {
            string originalUriValue = null;
#if NETCOREAPP3_1
            originalUriValue = HttpContext.Current.Request.Headers.TryGetOriginalUriValue();
#elif NET45_OR_GREATER
            originalUriValue = HttpContext.Current.Request.Headers[HeadersExtensions.ServiceUrlHeaderName];
#endif

            return string.IsNullOrEmpty(originalUriValue) ? HttpContext.Current.Request.GetUrl().Host : originalUriValue;
        }

        public static long GetUserIdFromKs(KS ks = null)
        {
            if (ks == null)
            {
                ks = KS.GetFromRequest();
            }

            if (ks == null)
                return 0;

            return ks.UserId.ParseUserId();
        }

        public static bool IsAllowedToViewInactiveAssets(int groupId, string userId, bool ignoreDoesGroupUsesTemplates = false)
        {
            return APILogic.Api.Managers.RolesPermissionsManager.Instance.IsPermittedPermission(groupId, userId, ApiObjects.RolePermissions.VIEW_INACTIVE_ASSETS)
                   && (DoesGroupUsesTemplates(groupId) || ignoreDoesGroupUsesTemplates);
        }

        public static bool DoesGroupUsesTemplates(int groupId)
        {
            return Core.Catalog.CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
        }

        internal static bool GetAbortOnErrorFromRequest()
        {
            var abortOnError= HttpContext.Current.Items[RequestContextConstants.MULTI_REQUEST_GLOBAL_ABORT_ON_ERROR];
            return abortOnError != null ? (bool)abortOnError : false;
        }

        internal static string RemoveSlashesFromBase64Str(string str)
        {
            return str.Replace('/', '_');
        }

        internal static string ReturnSlashesToBase64Str(string str)
        {
            return str.Replace('_', '/');
        }

        internal static Dictionary<string, string> ConvertSerializeableDictionary(SerializableDictionary<string, KalturaStringValue> dict, bool setNullIfEmpty, bool allowNullKey = false)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();

            if (dict != null && dict.Count > 0)
            {
                foreach (KeyValuePair<string, KalturaStringValue> pair in dict)
                {
                    //BEO-9601 - Not saved in case "key" is empty
                    if (allowNullKey || !string.IsNullOrEmpty(pair.Key))
                    {
                        if (!res.ContainsKey(pair.Key))
                        {
                            res.Add(pair.Key, pair.Value.value);
                        }
                        else
                        {
                            throw new ClientException((int)StatusCode.ArgumentsDuplicate, string.Format("key {0} already exists in sent dictionary", pair.Key));
                        }
                    }
                }
            }
            else if (setNullIfEmpty)
            {
                res = null;
            }

            return res;
        }

        internal static SerializableDictionary<string, KalturaStringValue> ConvertToSerializableDictionary(List<ApiObjects.KeyValuePair> dictionary)
        {
            var result = new SerializableDictionary<string, KalturaStringValue>();

            if (dictionary?.Any() == true)
            {
                foreach (var pair in dictionary)
                {
                    if (!result.ContainsKey(pair.key))
                    {
                        result.Add(pair.key, new KalturaStringValue {value = pair.value});
                    }
                    else
                    {
                        throw new ClientException((int) StatusCode.ArgumentsDuplicate, $"key {pair.key} already exists in sent dictionary");
                    }
                }
            }

            return result;
        }

        internal static SerializableDictionary<string, KalturaStringValue> ConvertToSerializableDictionary(List<KeyValuePair<string, string>> dictionary)
        {
            var result = new SerializableDictionary<string, KalturaStringValue>();

            if (dictionary != null)
            {
                foreach (var pair in dictionary)
                {
                    if (!result.ContainsKey(pair.Key))
                    {
                        result.Add(pair.Key, new KalturaStringValue { value = pair.Value });
                    }
                    else
                    {
                        throw new ClientException((int)StatusCode.ArgumentsDuplicate, $"key {pair.Key} already exists in sent dictionary");
                    }
                }
            }

            return result;
        }

        public static IEnumerable<string> GetOnDemandResponseProfileProperties()
        {
            Models.General.KalturaBaseResponseProfile responseProfile = Utils.GetResponseProfileFromRequest();

            if (responseProfile != null && responseProfile is Models.General.KalturaOnDemandResponseProfile onDemandResponseProfile)
            {
                SerializableDictionary<string, object> filteredResponse = new SerializableDictionary<string, object>();

                if (!string.IsNullOrEmpty(onDemandResponseProfile.RetrievedProperties))
                {
                    var properties = onDemandResponseProfile.RetrievedProperties.Split(',').Select(p => p.Trim());
                    var result = new HashSet<string>(defaultResponseProfileProperties);
                    result.UnionWith(properties);

                    return result;
                }
            }

            return null;
        }

        internal static Dictionary<string, long> ConvertSerializeableDictionary(SerializableDictionary<string, KalturaLongValue> dict, bool setNullIfEmpty)
        {
            Dictionary<string, long> res = new Dictionary<string, long>();

            if (dict != null && dict.Count > 0)
            {
                foreach (KeyValuePair<string, KalturaLongValue> pair in dict)
                {
                    if (!string.IsNullOrEmpty(pair.Key))
                    {
                        if (!res.ContainsKey(pair.Key))
                        {
                            res.Add(pair.Key, pair.Value.value);
                        }
                        else
                        {
                            throw new ClientException((int)StatusCode.ArgumentsDuplicate, string.Format("key {0} already exists in sent dictionary", pair.Key));
                        }
                    }
                }
            }
            else if (setNullIfEmpty)
            {
                res = null;
            }

            return res;
        }

        public static IEnumerable<T> ParseCommaSeparatedValues<T>(
            string itemsIn,
            string propertyName,
            bool checkDuplicate = false,
            bool ignoreDefaultValueValidation = false) where T : IConvertible =>
            ParseCommaSeparatedValues<HashSet<T>, T>(
                itemsIn,
                propertyName,
                checkDuplicate,
                ignoreDefaultValueValidation);

        /// <summary>
        /// Convert comma separated string to collection.
        /// </summary>
        /// <typeparam name="U">Collection of T</typeparam>
        /// <typeparam name="T">>Type of items in collection</typeparam>
        /// <param name="itemsIn">Comma separated string</param>
        /// <param name="propertyName">>The property name of comma separated string (for error message)</param>
        /// <param name="checkDuplicate"></param>
        /// <param name="ignoreDefaultValueValidation"></param>
        /// <returns></returns>
        public static U ParseCommaSeparatedValues<U, T>(
            string itemsIn,
            string propertyName,
            bool checkDuplicate = false,
            bool ignoreDefaultValueValidation = false) where T : IConvertible where U : ICollection<T>
        {
            var values = Activator.CreateInstance<U>();

            if (string.IsNullOrEmpty(itemsIn))
            {
                return values;
            }

            var stringValues = itemsIn.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var valueType = typeof(T);
            foreach (var stringValue in stringValues)
            {
                T value;
                try
                {
                    value = valueType.IsEnum ? (T)Enum.Parse(valueType, stringValue) : (T)Convert.ChangeType(stringValue, valueType);
                }
                catch (Exception)
                {
                    throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, propertyName, valueType.Name);
                }

                if (value != null && (ignoreDefaultValueValidation || !value.Equals(default(T))))
                {
                    if (!values.Contains(value))
                    {
                        values.Add(value);
                    }
                    else if (checkDuplicate)
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, propertyName);
                    }
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, propertyName);
                }
            }

            return values;
        }

        public static bool IsBetween<T>(T value, T minValue, T maxValue) where T : IComparable
        {
            var compareWithMinValue = value.CompareTo(minValue);
            var compareWithMaxValue = value.CompareTo(maxValue);

            return compareWithMinValue >= 0 && compareWithMaxValue <= 0;
        }

        public static UserSearchContext GetUserSearchContext()
        {
            var groupId = KS.GetFromRequest().GroupId;
            var domainId = KS.GetContextData().DomainId ?? 0;
            var userId = GetUserIdFromKs();
            var languageId = GetLanguageId(groupId, KS.GetContextData().Language);
            var udid = KS.GetContextData().Udid;
            var userIp = GetClientIP();
            var isAllowedToViewInactiveAssets = IsAllowedToViewInactiveAssets(groupId, userId.ToString(), true);
            var group = GroupsManager.Instance.GetGroup(groupId);
            var sessionCharacteristicKey = KSUtils.ExtractKSPayload(KS.GetFromRequest()).SessionCharacteristicKey;

            return new UserSearchContext(domainId, userId, languageId, udid, userIp, false, group.UseStartDate, false, group.GetOnlyActiveAssets, isAllowedToViewInactiveAssets, sessionCharacteristicKey);
        }
    }
}
