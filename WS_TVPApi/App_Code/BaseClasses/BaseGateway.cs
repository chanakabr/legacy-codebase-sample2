using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiServices;

/// <summary>
/// Summary description for BaseGateway
/// </summary>
public abstract class BaseGateway : System.Web.UI.Page
{
    protected MediaService m_MediaService = new MediaService();

    protected SiteService m_SiteService = new SiteService();

    private string m_WsUsername;
    private string m_WsPassword;

    protected PlatformType devType = PlatformType.STB;

    protected string WsUserName { get { return m_WsUsername; } }
    protected string WsPassword { get { return m_WsPassword; } }

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
        //Locale locale = new Locale();
        //locale.LocaleLanguage = "es";
        //retVal.Locale = locale;
        return retVal;
    }

    protected string GetStbUserId(int groupId)
    {
        string mac = Request.QueryString["identity"];
        if (groupId == 125)
        {
            if (mac == "00043050E72B")
                return m_SiteService.SignIn(GetInitObj(), "ido@tvinci.com", "eliron27").SiteGuid;
            else if (mac != "00043050E79A")
                return m_SiteService.SignIn(GetInitObj(), "adina@tvinci.com", "eliron27").SiteGuid;            
            else
                return m_SiteService.SignIn(GetInitObj(), "yonatan@tvinci.com", "eliron27").SiteGuid;
        }

        return m_SiteService.SignIn(GetInitObj(), "adina@tvinci.com", "eliron27").SiteGuid;
    }
}
