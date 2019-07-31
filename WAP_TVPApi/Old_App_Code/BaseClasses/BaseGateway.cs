using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiServices;
using System.Threading;
using System.Web.Caching;
using System.Runtime.CompilerServices;

/// <summary>
/// Summary description for BaseGateway
/// </summary>
public abstract class BaseGateway : System.Web.UI.Page
{
    private class KnownUsersAndPasswords
    {
        public String UserName { get; set; }
        public String Password { get; set; }
    }
    /// <summary>
    /// For backwards compatibility
    /// </summary>
    private static Dictionary<String, KnownUsersAndPasswords> s_KnownMacAddress = new Dictionary<string, KnownUsersAndPasswords>()
    {
        { "0004305A0C0C",new KnownUsersAndPasswords(){UserName = "shayo.ofer@gmail.com", Password = "123456"}},
        { "00043053C7FD",new KnownUsersAndPasswords(){UserName = "ido@tvinci.com", Password = "123456"}},
        { "00043055A598",new KnownUsersAndPasswords(){UserName = "alamdan@gmail.com", Password = "123456"}},        
        { "00043048324A",new KnownUsersAndPasswords(){UserName = "eilat.amir@gmail.com", Password = "123456"}},
        { "00043055A593",new KnownUsersAndPasswords(){UserName = "adinagrynberg@yahoo.com", Password = "123456"}},

        { "000430483825",new KnownUsersAndPasswords(){UserName = "amir1@tvinci.com", Password = "123456"}},
        { "00043049804A",new KnownUsersAndPasswords(){UserName = "netgem", Password = "123456"}},
        { "0004305A0BB2",new KnownUsersAndPasswords(){UserName = "africatv@tvinci.com", Password = "123456"}},
        { "0004305A0BBD",new KnownUsersAndPasswords(){UserName = "africatv@tvinci.com", Password = "123456"}},
        //{ "00043048324A",new KnownUsersAndPasswords(){UserName = "adina@tvinci.com", Password = "eliron27"}},        
        { "00043050E79A",new KnownUsersAndPasswords(){UserName = "demo@motorola.com", Password = "123456"}},        
        { "00043053C7C7",new KnownUsersAndPasswords(){UserName = "tvinci1@tvinci.com", Password = "123456"}},        
        { "0004305A0BBC",new KnownUsersAndPasswords(){UserName = "adina1@tvinci.com", Password = "123456"}},        
        { "0004305A0BBE",new KnownUsersAndPasswords(){UserName = "adina2@tvinci.com", Password = "123456"}},        
        //{ "00043055A598",new KnownUsersAndPasswords(){UserName = "yonatan1@tvinci.com", Password = "123456"}},        
        { "00043055A585",new KnownUsersAndPasswords(){UserName = "Ofer.Shmueli@orange.co.il", Password = "Orange123"}},        
        { "00043055A583",new KnownUsersAndPasswords(){UserName = "menahem.tirosh1@orange.co.il", Password = "Orange123"}},        
        { "00043055A548",new KnownUsersAndPasswords(){UserName = "menahem.tirosh1@orange.co.il", Password = "Orange123"}}, 
        { "00043055A555",new KnownUsersAndPasswords(){UserName = "Meytal.Sabag@orange.co.il", Password = "Orange123"}},        
        { "00043055A561",new KnownUsersAndPasswords(){UserName = "oriwa@012.net", Password = "Orange123"}},        
        { "00043055A586",new KnownUsersAndPasswords(){UserName = "Moshe.SimanTov@orange.co.il", Password = "Orange123"}},        
        { "00043055A568",new KnownUsersAndPasswords(){UserName = "dudim@012.net", Password = "Orange123"}},
        { "00043055A595",new KnownUsersAndPasswords(){UserName = "yacov.kedmi1@orange.co.il", Password = "Orange123"}},
        { "00043055A551",new KnownUsersAndPasswords(){UserName = "haim.romano1@orange.co.il", Password = "Orange123"}}
    };

    protected MediaService m_MediaService = new MediaService();
    protected SiteService m_SiteService = new SiteService();
    protected PricingService m_PricingService = new PricingService();
    protected DomainService m_DomainService = new DomainService();
    protected BillingService m_BillingService = new BillingService();
    protected ConditionalAccessService m_ConditionalAccessService = new ConditionalAccessService();
    protected SocialService m_SocialService = new SocialService();
    protected UsersService m_UsersService = new UsersService();
    protected NotificationService m_NotificationService = new NotificationService();

    private string m_WsUsername;
    private string m_WsPassword;

    protected PlatformType devType = PlatformType.STB;

    protected string WsUserName { get { return m_WsUsername; } }
    protected string WsPassword { get { return m_WsPassword; } }

    protected static Object mainRequestLocker = new object();
    protected static Dictionary<String, Object> locks = new Dictionary<string, object>();

    protected string ParseObject(object obj, int groupID, int items, int index, long mediaCount, PlatformType platform)
    {
        string retVal = string.Empty;
        IParser parser = ParserHelper.GetParser(groupID);
        if (parser != null)
        {
            retVal = parser.Parse(obj, items, index, groupID, mediaCount, platform);
        }
        return retVal;
    }

    protected int GetGroupIDByBroadcasterName(string broadcasterName)
    {
        int retVal = 0;
        switch (broadcasterName.ToLower())
        {
            case "tele5":
                {
                    retVal = 122;
                    m_WsUsername = "tvpapi_122";
                    m_WsPassword = "11111";
                    break;
                }
            case "whitelabel":
                {
                    retVal = 123;
                    m_WsUsername = "tvpapi_123";
                    m_WsPassword = "11111";
                    break;
                }
            case "whitelabel2":
                {
                    retVal = 124;
                    m_WsUsername = "tvpapi_124";
                    m_WsPassword = "11111";
                    break;
                }

            case "novetest":
                {
                    retVal = 93;
                    m_WsUsername = "tvpapi_93";
                    m_WsPassword = "11111";
                    break;
                }
            case "ipvision":
                {
                    retVal = 125;
                    m_WsUsername = "tvpapi_125";
                    m_WsPassword = "11111";
                    break;
                }
            case "turkcell":
                {
                    retVal = 131;
                    m_WsUsername = "tvpapi_131";
                    m_WsPassword = "11111";
                    break;
                }
            default:
                {
                    retVal = 122;
                    m_WsUsername = "tvpapi_122";
                    m_WsPassword = "11111";
                    break;
                }
        }
        return retVal;
    }

    protected InitializationObject GetInitObj()
    {
        string sUDID = HttpContext.Current.Request.QueryString["identity"];

        InitializationObject retVal = new InitializationObject();
        retVal.Platform = devType;
        retVal.ApiUser = m_WsUsername;
        retVal.ApiPass = m_WsPassword;
        retVal.UDID = sUDID;
        if (string.IsNullOrEmpty(retVal.UDID))
            retVal.UDID = "dummy";
        //Locale locale = new Locale();
        //locale.LocaleLanguage = "es";
        //retVal.Locale = locale;
        return retVal;
    }

    protected string GetStbUserId(int groupId)
    {
        string mac = Request.QueryString["identity"];
        if (String.IsNullOrEmpty(mac)) return String.Empty;
        string SiteGuid = String.Empty;
        //if (groupId == 125)
        //{
        //    do
        //    {
        //        if (HttpContext.Current.Cache[mac] != null)
        //            break;

        //        if (!locks.ContainsKey(mac))
        //        {
        //            lock (mainRequestLocker)
        //            {
        //                if (!locks.ContainsKey(mac))
        //                {
        //                    locks.Add(mac, new Object());
        //                }
        //            }
        //        }

        //        if (HttpContext.Current.Cache[mac] != null)
        //            break;

        //        lock (locks[mac])
        //        {
        //            if (HttpContext.Current.Cache[mac] != null)
        //                break;

        //            TVPApiModule.Services.ApiDomainsService.DeviceDomain[] retSiteGuid = m_DomainService.GetDeviceDomains(new InitializationObject()
        //            {
        //                UDID = mac,
        //                Platform = PlatformType.STB,
        //                ApiUser = m_WsUsername,
        //                ApiPass = m_WsPassword
        //            });
        //            if (retSiteGuid != null && retSiteGuid.Length > 0 && retSiteGuid[0].DomainID != 0)
        //            {
        //                SiteGuid = retSiteGuid[0].SiteGuid;
        //                HttpContext.Current.Cache.Add(mac, SiteGuid, null, DateTime.Now.AddMinutes(15), System.Web.Caching.Cache.NoSlidingExpiration,
        //                    System.Web.Caching.CacheItemPriority.Normal, ItemRemovedFromCacheCallback);
        //            }
        //            else
        //            {//Check Known MAC address
        //                if (s_KnownMacAddress.ContainsKey(mac))
        //                {
        //                    string usr = s_KnownMacAddress[mac].UserName;
        //                    string pass = s_KnownMacAddress[mac].Password;
        //                    SiteGuid = m_SiteService.SignIn(GetInitObj(), usr, pass).SiteGuid;
        //                    if (SiteGuid == null)
        //                        SiteGuid = "0";
        //                    HttpContext.Current.Cache.Add(mac, SiteGuid, null, DateTime.Now.AddMinutes(15), System.Web.Caching.Cache.NoSlidingExpiration,
        //                        System.Web.Caching.CacheItemPriority.Normal, ItemRemovedFromCacheCallback);
        //                }
        //            }
        //        }

        //    } while (false);

        //    SiteGuid = HttpContext.Current.Cache[mac] != null ? HttpContext.Current.Cache[mac].ToString() : String.Empty;

        //    return SiteGuid;
        //}

        return m_SiteService.SignIn(GetInitObj(), "adina@tvinci.com", "eliron27").SiteGuid;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void ItemRemovedFromCacheCallback(String key, object value, CacheItemRemovedReason removeReason)
    {
        if (removeReason == CacheItemRemovedReason.Expired)
        {
            locks.Remove(key);
        }
    }
}

