using ApiObjects.User;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Utils
{
    public class Utils
    {
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
            if (HttpContext.Current.Items[RequestContextUtils.USER_IP] != null)
            {
                return HttpContext.Current.Items[RequestContextUtils.USER_IP].ToString();
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
            if (HttpContext.Current.Items[RequestContextUtils.REQUEST_LANGUAGE] == null)
            {
                return null;
            }

            return HttpContext.Current.Items[RequestContextUtils.REQUEST_LANGUAGE].ToString();
        }

        public static int? GetGroupIdFromRequest()
        {
            if (HttpContext.Current.Items[RequestContextUtils.REQUEST_GROUP_ID] == null)
            {
                return null;
            }

            return (int) HttpContext.Current.Items[RequestContextUtils.REQUEST_GROUP_ID];
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
            var currency = HttpContext.Current.Items[RequestContextUtils.REQUEST_CURRENCY];
            return currency != null ? currency.ToString() : null;
        }

        internal static string GetFormatFromRequest()
        {
            var format = HttpContext.Current.Items[RequestContextUtils.REQUEST_FORMAT];
            return format != null ? format.ToString() : null;
        }

        internal static WebAPI.Models.General.KalturaBaseResponseProfile GetResponseProfileFromRequest()
        {
            KalturaBaseResponseProfile responseProfile = (KalturaBaseResponseProfile)HttpContext.Current.Items[RequestContextUtils.REQUEST_RESPONSE_PROFILE];
                        
            return responseProfile != null ? responseProfile as WebAPI.Models.General.KalturaBaseResponseProfile : null;
        }

        public static string GetCurrentBaseUrl()
        {
            string xForwardedProtoHeader = HttpContext.Current.Request.Headers["X-Forwarded-Proto"];
            string xKProxyProto = HttpContext.Current.Request.Headers["X-KProxy-Proto"];

            string baseUrl = string.Format("{0}://{1}{2}", (!string.IsNullOrEmpty(xForwardedProtoHeader) && xForwardedProtoHeader == "https") ||
                (!string.IsNullOrEmpty(xKProxyProto) && xKProxyProto == "https") ?
                "https" : HttpContext.Current.Request.GetUrl().Scheme, HttpContext.Current.Request.GetUrl().Host, HttpContext.Current.Request.GetApplicationPath().TrimEnd('/'));
            return baseUrl;
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
            return APILogic.Api.Managers.RolesPermissionsManager.IsPermittedPermission(groupId, userId, ApiObjects.RolePermissions.VIEW_INACTIVE_ASSETS)
                   && (DoesGroupUsesTemplates(groupId) || ignoreDoesGroupUsesTemplates);
        }

        public static bool DoesGroupUsesTemplates(int groupId)
        {
            return Core.Catalog.CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
        }

        internal static bool GetAbortOnErrorFromRequest()
        {
            var abortOnError= HttpContext.Current.Items[RequestContextUtils.MULTI_REQUEST_GLOBAL_ABORT_ON_ERROR];
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
                    return onDemandResponseProfile.RetrievedProperties.Split(',').Select(p => p.Trim());
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

        internal static IEnumerable<long> ParseCommaSeparatedString(string value, char[] separator, string argumentName)
        {
            var ids = new HashSet<long>();
            if (!string.IsNullOrEmpty(value))
            {
                var stringValues = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var stringValue in stringValues)
                {
                    if (long.TryParse(stringValue, out var id) && !ids.Contains(id))
                    {
                        ids.Add(id);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, argumentName);
                    }
                }
            }

            return ids.ToArray();
        }
    }
}
