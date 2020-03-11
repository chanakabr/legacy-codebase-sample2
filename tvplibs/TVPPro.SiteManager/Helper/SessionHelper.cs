using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.DataEntities;

namespace TVPPro.SiteManager.Helper
{
    //This class wraps all Session variable handling (reading and writing)
    public class SessionHelper
    {
        public static TVPPro.SiteManager.Services.BillingService.BillingUserData UserPurchaseData
        {
            get
            {
                TVPPro.SiteManager.Services.BillingService.BillingUserData purchaseData;
                GetValueFromSession<TVPPro.SiteManager.Services.BillingService.BillingUserData>("UserPurchaseData", out purchaseData);
                return purchaseData;
            }

            set
            {
                SetValueInSession("UserPurchaseData", value);
            }
        }
        public static string CuponCode
        {
            get
            {
                string cupCode = string.Empty;
                GetValueFromSession<string>("CuponCode", out cupCode);
                return cupCode;
            }
            set
            {
                SetValueInSession("CuponCode", value);
            }
        }
        public static bool WatchPurchasedItem
        {
            get
            {
                bool bRet = false;
                GetValueFromSession<bool>("WatchPurchasedItem", out bRet);
                return bRet;
            }
            set
            {
                SetValueInSession("WatchPurchasedItem", value);
            }
        }

        public static string CurrentUserToken
        {
            get
            {
                string retVal = string.Empty;
                GetValueFromSession<string>("CurrentUserToken", out retVal);
                return retVal;
            }
            set
            {
                SetValueInSession("CurrentUserToken", value);
            }
        }

        public static bool IsLegalAge
        {
            get
            {
                bool retVal = false;
                if (GetValueFromSession<bool>("IsLegalAge", out retVal))
                {
                    return retVal;
                }
                return false;
            }
            set
            {
                SetValueInSession("IsLegalAge", value);
            }
        }

        public static Dictionary<string, string> QueryDictionary
        {
            get
            {
                Dictionary<string, string> retVal;
                if (GetValueFromSession<Dictionary<string, string>>("QueryDict", out retVal))
                {
                    return retVal;
                }
                else
                {
                    return new Dictionary<string, string>();
                }
            }
            set
            {
                SetValueInSession("QueryDict", value);
            }
        }

        public static string NextPage
        {
            get
            {
                string retVal = string.Empty;
                if (GetValueFromSession<string>("NextPage", out retVal))
                {
                    return retVal;
                }
                return string.Empty;
            }
            set
            {
                SetValueInSession("NextPage", value);
            }
        }

        public static Locale LocaleInfo
        {
            get
            {
                Locale locale;
                if (GetValueFromSession<Locale>("LocaleInfo", out locale))
                {
                    return locale;
                }
                return null;
            }
            set
            {
                SetValueInSession("LocaleInfo", value);
            }
        }

        public static string Affiliate
        {
            get
            {
                string retVal = string.Empty;
                if (GetValueFromSession<string>("Affiliate", out retVal))
                {
                    return retVal;
                }
                return string.Empty;
            }
            set
            {
                SetValueInSession("Affiliate", value);
            }
        }

        public static string InviteCode
        {
            get
            {
                string retVal = string.Empty;
                if (GetValueFromSession<string>("InviteCode", out retVal))
                {
                    return retVal;
                }
                return string.Empty;
            }
            set
            {
                SetValueInSession("InviteCode", value);
            }
        }

        public static dsItemInfo CurrentMediaItem
        {
            get
            {
                dsItemInfo retVal = null;
                if (GetValueFromSession<dsItemInfo>("CurrentMediaItem", out retVal))
                {
                    return retVal;
                }
                return null;
            }
            set
            {
                SetValueInSession("CurrentMediaItem", value);
            }
        }

        public static string FavoriteAction
        {
            get
            {
                string retVal = string.Empty;
                if (GetValueFromSession<string>("FavoriteAction", out retVal))
                {
                    return retVal;
                }
                return string.Empty;
            }
            set
            {
                SetValueInSession("FavoriteAction", value);
            }
        }

        public static string FavoriteMediaId
        {
            get
            {
                string retVal = string.Empty;
                if (GetValueFromSession<string>("FavoriteMediaId", out retVal))
                {
                    return retVal;
                }
                return string.Empty;
            }
            set
            {
                SetValueInSession("FavoriteMediaId", value);
            }
        }

        public static string RefererUrl
        {
            get
            {
                string retVal = string.Empty;
                if (GetValueFromSession<string>("RefererUrl", out retVal))
                {
                    return retVal;
                }
                return string.Empty;
            }
            set
            {
                SetValueInSession("RefererUrl", value);
            }
        }

        public static bool PurchaseMode
        {
            get
            {
                bool bRet = false;
                GetValueFromSession<bool>("PurchaseMode", out bRet);
                return bRet;
            }
            set
            {
                SetValueInSession("PurchaseMode", value);
            }
        }

        public static string PurchaseMediaId
        {
            get
            {
                string retVal = string.Empty;
                GetValueFromSession<string>("PurchaseMediaId", out retVal);
                return retVal;
            }
            set
            {
                SetValueInSession("PurchaseMediaId", value);
            }
        }

        public static bool UseFinalEndDate
        {
            get
            {
                bool bRet = false;
                GetValueFromSession<bool>("UseFinalEndDate", out bRet);
                return bRet;
            }
            set
            {
                SetValueInSession("UseFinalEndDate", value);
            }
        }

        public static string UniqueId
        {
            get
            {
                string retVal = string.Empty;
                GetValueFromSession<string>("UniqueId", out retVal);
                return retVal;
            }
            set
            {
                SetValueInSession("UniqueId", value);
            }
        }

        public static string RequestIp
        {
            get
            {
                string retVal = string.Empty;
                GetValueFromSession<string>("RequestIp", out retVal);
                return retVal;
            }
            set
            {
                SetValueInSession("RequestIp", value);
            }
        }

        public static string SessionId
        {
            get
            {
                string retVal = string.Empty;
                GetValueFromSession<string>("SessionId", out retVal);
                return retVal;
            }
            set
            {
                SetValueInSession("SessionId", value);
            }
        }

        public static int ClientTimezoneOffset
        {
            get
            {
                int retVal = 0;
                GetValueFromSession<int>("TimeZoneOffset", out retVal);
                return retVal;
            }
            set
            {
                SetValueInSession("TimeZoneOffset", value);
            }
        }

        private static string TryGetDeviceDNA()
        {
            string value = string.Empty;
            try
            {
                SessionHelper.GetValueFromSession<string>("DeviceDNA", out value);

                if (string.IsNullOrEmpty(value))
                {
                    value = System.Web.HttpContext.Current.Request.Headers["X-DeviceDNA-UDID"];

                    // This is for development purposes 
                    if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["deviceDNA_OR"]))
                    {
                        value = System.Configuration.ConfigurationManager.AppSettings["deviceDNA_OR"];
                    }

                    if (!string.IsNullOrEmpty(value))
                    {
                        DeviceDNA = value;
                    }
                }
            }
            catch
            {
                value = string.Empty;
            }

            return value;
        }

        public static string DeviceDNA
        {
            get
            {
                return TryGetDeviceDNA();
            }
            set
            {
                SetValueInSession("DeviceDNA", value);
            }
        }

        public static bool UseDeviceDNA
        {
            get
            {
                bool bRet = false;
                if (System.Configuration.ConfigurationManager.AppSettings["EnableDeviceDNA"] != null)
                {
                    bRet = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableDeviceDNA"]);
                }
                return bRet;
            }
        }

        public static bool ActionAllowed
        {
            get
            {
                bool bRet = false;

                bool useDeviceDNA = false;
                if (System.Configuration.ConfigurationManager.AppSettings["EnableDeviceDNA"] != null)
                {
                    useDeviceDNA = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["EnableDeviceDNA"]);
                }

                if (useDeviceDNA)
                {
                    GetValueFromSession<bool>("ActionAllowed", out bRet);
                }
                else
                {
                    bRet = true;
                }
                return bRet;
            }
            set
            {
                SetValueInSession("ActionAllowed", value);
            }
        }

        public static bool IsValueInSession(string key)
        {
            bool retVal = false;

            try
            {
                if (HttpContext.Current.Session[key] != null)
                {
                    retVal = true;
                }
            }
            catch (Exception) { }

            return retVal;
        }


        public static bool GetValueFromSession<TValue>(string key, out TValue value)
        {
            value = default(TValue);
            if (HttpContext.Current.Session[key] is TValue)
            {
                value = (TValue)HttpContext.Current.Session[key];
                return true;
            }
            
            return false;
        }

        public static void SetValueInSession(string key, object value)
        {
            if (IsValueInSession(key))
            {
                HttpContext.Current.Session.Remove(key);
            }
            HttpContext.Current.Session[key] = value;

        }

        public static void RemoveValueInSession(string key)
        {
            if (IsValueInSession(key))
            {
                HttpContext.Current.Session.Remove(key);
            }
        }

        public static TvinciPlatform.Users.Country IPCountry
        {
            get
            {
                TvinciPlatform.Users.Country oRet = null;
                GetValueFromSession<TvinciPlatform.Users.Country>("IPCountry", out oRet);
                return oRet;
            }
            set
            {
                SetValueInSession("IPCountry", value);
            }
        }

        public class SessionKeys
        {
            public const string UserPassword = "I";
            public const string UserName = "username";
            public const string KeepSessionAlive = "KeepSessionAlive";
            public const string FacebookUserConfiguration = "FBUser";
            public const string IsDomainMaster = "IsDomainMaster";
            public const string DomianID = "DomianID";
            public const string SiteGUID = "SiteGUID";

        }
    }
}
