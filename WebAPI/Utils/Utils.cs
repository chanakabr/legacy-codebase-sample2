using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using TVinciShared;
using WebAPI.ClientManagers;
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
            if (HttpContext.Current.Items[RequestContext.USER_IP] != null)
            {
                return HttpContext.Current.Items[RequestContext.USER_IP].ToString();
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
            if (HttpContext.Current.Items[RequestContext.REQUEST_LANGUAGE] == null)
            {
                return null;
            }

            return HttpContext.Current.Items[RequestContext.REQUEST_LANGUAGE].ToString();
        }

        public static int? GetGroupIdFromRequest()
        {
            if (HttpContext.Current.Items[RequestContext.REQUEST_GROUP_ID] == null)
            {
                return null;
            }

            return (int) HttpContext.Current.Items[RequestContext.REQUEST_GROUP_ID];
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
                        
            return GroupsManager.GetGroup(groupId.Value).Languages;
        }

        internal static string GetCurrencyFromRequest()
        {
            var currency = HttpContext.Current.Items[RequestContext.REQUEST_CURRENCY];
            return currency != null ? currency.ToString() : null;
        }

        internal static string GetFormatFromRequest()
        {
            var format = HttpContext.Current.Items[RequestContext.REQUEST_FORMAT];
            return format != null ? format.ToString() : null;
        }

        internal static WebAPI.Models.General.KalturaBaseResponseProfile GetResponseProfileFromRequest()
        {
            KalturaBaseResponseProfile responseProfile = (KalturaBaseResponseProfile)HttpContext.Current.Items[RequestContext.REQUEST_RESPONSE_PROFILE];
                        
            return responseProfile != null ? responseProfile as WebAPI.Models.General.KalturaBaseResponseProfile : null;
        }

        public static bool ConvertStringToDateTimeByFormat(string dateInString, string convertToFormat, out DateTime dateTime)
        {
            return DateTime.TryParseExact(dateInString, convertToFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime);
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

        public static long GetUserIdFromKs()
        {
            var ks = KS.GetFromRequest();

            if (ks == null)
                return 0;

            string siteGuid = ks.UserId;

            if (siteGuid == "0")
                return 0;

            long userId = 0;
            if (long.TryParse(siteGuid, out userId) && userId > 0)
            {
                return userId;
            }
            else
            {
                return 0;
            }
        }
        
        public static bool IsAllowedToViewInactiveAssets(int groupId, string userId, bool ignoreDoesGroupUsesTemplates = false)
        {
            return APILogic.Api.Managers.RolesPermissionsManager.IsPermittedPermission(groupId, userId, ApiObjects.RolePermissions.VIEW_INACTIVE_ASSETS)
                   && (DoesGroupUsesTemplates(groupId) || ignoreDoesGroupUsesTemplates);
        }

        public static bool DoesGroupUsesTemplates(int groupId)
        {
            return Core.Catalog.CatalogManagement.CatalogManager.DoesGroupUsesTemplates(groupId);
        }

        internal static bool GetAbortOnErrorFromRequest()
        {
            var abortOnError= HttpContext.Current.Items[RequestContext.MULTI_REQUEST_GLOBAL_ABORT_ON_ERROR];
            return abortOnError != null ? (bool)abortOnError : false;
        }
    }
}