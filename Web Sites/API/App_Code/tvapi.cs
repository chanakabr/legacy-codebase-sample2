using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using ApiObjects;

/// <summary>
/// Summary description for tvapi
/// </summary>
[WebService(Namespace = "http://api.tvinci.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.ComponentModel.ToolboxItem(false)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class tvapi : System.Web.Services.WebService
{
    #region Helper functions
    static protected Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName ,DateTime dUTCTime)
    {
        string sIP = TVinciShared.PageUtils.GetCallerIP();
        Int32 nGroupID = TVinciShared.WS_Utils.GetGroupID("api", sFunctionName, sWSUserName, sWSPassword, sIP);
        if (nGroupID == 0)
        {
            string sSecret = TVinciShared.WS_Utils.GetSecretCode("api", sFunctionName, sWSUserName, ref nGroupID);
            if (sSecret != "")
            {
                string sToHash = sSecret + (dUTCTime - new DateTime(1970 , 1 ,1)).TotalSeconds.ToString() + sIP;
                string sHashed = TVinciShared.ProtocolsFuncs.CalculateMD5Hash(sToHash);
                if (sHashed != sWSPassword)
                {
                    Logger.Logger.Log("WS ignored (hash not correct)", "IP: " + sIP + ",Function: " + sFunctionName + " UN: " + sWSUserName + " Pass: " + sWSPassword, "api");
                    nGroupID = 0;
                }
            }
            else
                Logger.Logger.Log("WS ignored", "IP: " + sIP + ",Function: " + sFunctionName + " UN: " + sWSUserName + " Pass: " + sWSPassword, "api");
        }
        return nGroupID;
    }

    static protected void HandleSession()
    {
        string sSessionID = HttpContext.Current.Session.SessionID;
        HttpContext.Current.Response.Cookies["ASP.NET_SessionId"].Value = sSessionID;
    }

    #endregion

    #region Public API
    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    public UserIMRequestObject TVAPI_GetTvinciGUID(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetTvinciGUID" , dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.GetTvinciGUID(oInitObj, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject TVAPI_GetMedias(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, Int32[] nMediaIDs, MediaInfoStructObject theInfoStruct)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetMedias", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.SingleMedia(oInitObj, nGroupID, nMediaIDs, theInfoStruct);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_GetMedias ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public MediaInfoStructObject TVAPI_GetMediaStructure(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetMediaStructure", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.GetMediaStructure(oInitObj, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_GetMediaStructure ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject TVAPI_GetMediaInfo(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, Int32[] nMediaIDs, MediaInfoStructObject theInfoStruct)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetMediaInfo", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    oInitObj.m_oFileRequestObjects = null;
                    HandleSession();
                    return APIWS.api.SingleMedia(oInitObj, nGroupID, nMediaIDs, theInfoStruct);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_GetMediaInfo ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject TVAPI_SearchMedia(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, SearchDefinitionObject oSearchDefinitionObj, MediaInfoStructObject theInfoStruct)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_SearchMedia", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.SearchMedia(oInitObj, oSearchDefinitionObj, theInfoStruct, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_SearchMedia ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject TVAPI_SearchRelated(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nMediaID)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_SearchRelated", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.SearchRelated(oInitObj, theInfoStruct, thePageDef, nMediaID, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_SearchRelated ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject TVAPI_NowPlaying(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_NowPlaying", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.NowPlaying(oInitObj, theInfoStruct, thePageDef, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_NowPlaying ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject TVAPI_UserLastWatched(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_UserLastWatched", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.UserLastWatched(oInitObj, theInfoStruct, thePageDef, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_UserLastWatched ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject TVAPI_PeopleWhoWatchedAlsoWatched(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nMediaID, Int32 nMediaFileID)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_PeopleWhoWatchedAlsoWatched", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.PeopleWhoWatchedAlsoWatched(oInitObj, theInfoStruct, nGroupID, nMediaID, nMediaFileID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_PeopleWhoWatchedAlsoWatched ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject[] TVAPI_ChannelsMedia(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, ChannelRequestObject[] theChannelsRequestObj)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_ChannelsMedia", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.ChannelsMedia(oInitObj, theInfoStruct, theChannelsRequestObj, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_ChannelsMedia ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public CategoryObject[] TVAPI_CategoriesTree(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nCategoryID, bool bWithChannels)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_CategoriesTree", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.CategoriesTree(oInitObj, theInfoStruct, nGroupID, nCategoryID, bWithChannels);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_CategoriesTree ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject[] TVAPI_CategoryChannels(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nCategoryID)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_CategoryChannels", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.CategoryChannels(oInitObj, theInfoStruct, nGroupID, nCategoryID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_CategoryChannels ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject[] TVAPI_UserSavedChannels(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_UserSavedChannels", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.UserChannels(oInitObj, theInfoStruct, nGroupID , "");
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_UserSavedChannels ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject[] TVAPI_UserDeleteChannel(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nChannelID)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_UserDeleteChannel", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.DeleteUserChannel(oInitObj, theInfoStruct, nGroupID, nChannelID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_UserDeleteChannel ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public GenericWriteResponse TVAPI_UserSavePlaylist(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, Int32[] nMediaIDs, string sPlaylistTitle, bool bRewrite)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_UserSavePlaylist", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.saveUserPlaylist(oInitObj, nGroupID, nMediaIDs, sPlaylistTitle, bRewrite);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_UserSavePlaylist ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public GenericWriteResponse TVAPI_SendMediaByEmail(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, Int32 nMediaID,
        string sFromEmail, string sToEmail, string sRecieverName, string sSenderName, string sContent)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_SendMediaByEmail", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.SendMediaByEmail(oInitObj, nGroupID, nMediaID, sFromEmail, sToEmail, sRecieverName, sSenderName, sContent);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_SendMediaByEmail ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public RateResponseObject TVAPI_RateMedia(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, Int32 nMediaID,
        Int32 nRateVal)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_RateMedia", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.RateMedia(oInitObj, nMediaID, nRateVal, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_RateMedia ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(RateResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ChannelObject TVAPI_PersonalRatedList(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nMinimumRateVal, Int32 nMaximumRateVal)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_PersonalRatedList", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.PersonalRateList(oInitObj, theInfoStruct, thePageDef, nGroupID , nMinimumRateVal , nMaximumRateVal);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_PersonalRatedList ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ApiObjects.TagResponseObject[] TVAPI_TagValues(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, TagRequestObject[] oTagsDefinition)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_TagValues", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.TagValues(oInitObj, oTagsDefinition, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_TagValues ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }
    
    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ApiObjects.OneTimeObject TVAPI_OneTime(string sWSUserName, string sWSPassword, DateTime dUTCTime, InitializationObject oInitObj, 
        Int32 nMediaID , Int32 nMediaFileID , string sBaseLink , string sCDNImplType)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_OneTime", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.OneTimeLink(oInitObj, nMediaID , nMediaFileID , sBaseLink , sCDNImplType, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_OneTime ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ApiObjects.UserComment[] TVAPI_MediaComments(string sWSUserName, string sWSPassword, DateTime dUTCTime,
        InitializationObject oInitObj, Int32 nMediaID, string sCommentType)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_MediaComments", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.MediaComments(oInitObj, nMediaID, nGroupID , sCommentType);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_MediaComments ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public ApiObjects.UserComment[] TVAPI_SaveUserComment(string sWSUserName, string sWSPassword, DateTime dUTCTime,
        InitializationObject oInitObj, UserComment oComment, bool bAutoActivate, string sCommentType)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_SaveUserComment", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.UserSaveComment(oInitObj, oComment, bAutoActivate, nGroupID , sCommentType);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_SaveUserComment ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public bool TVAPI_SetMediaDuration(string sWSUserName, string sWSPassword, DateTime dUTCTime,
        InitializationObject oInitObj , Int32 nMediaFileID , Int32 nDurationInSec)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_SetMediaDuration", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.SetMediaDuration(oInitObj, nMediaFileID, nDurationInSec, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_SetMediaDuration ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return false;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public bool TVAPI_MediaMark(string sWSUserName, string sWSPassword, DateTime dUTCTime,
        InitializationObject oInitObj, string sAction, Int32 nLocationInSec, MediaFileObject theMediaFileObject)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_MediaMark", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.MediaMark(oInitObj, sAction, nLocationInSec, ref theMediaFileObject, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_MediaMark ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return false;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    [WebMethod(true)]
    [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
    [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PlayListSchema))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaAdObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaFileObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaInfoObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(MediaPersonalStatistics))]
    [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaBoolObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaDoubleObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaM2MObject))]
    [System.Xml.Serialization.XmlInclude(typeof(MetaStrObject))]
    [System.Xml.Serialization.XmlInclude(typeof(PageDefinition))]
    [System.Xml.Serialization.XmlInclude(typeof(ChannelRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(CategoryObject))]
    [System.Xml.Serialization.XmlInclude(typeof(GenericWriteResponse))]
    [System.Xml.Serialization.XmlInclude(typeof(StringRange))]
    [System.Xml.Serialization.XmlInclude(typeof(TagResponseObject))]
    [System.Xml.Serialization.XmlInclude(typeof(TagRequestObject))]
    [System.Xml.Serialization.XmlInclude(typeof(OneTimeObject))]
    [System.Xml.Serialization.XmlInclude(typeof(UserComment))]
    public bool TVAPI_Hit(string sWSUserName, string sWSPassword, DateTime dUTCTime,
        InitializationObject oInitObj, Int32 nLocationInVideoSec, MediaFileObject theMediaFileObject)
    {
        try
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_Hit", dUTCTime);
            if (nGroupID != 0)
            {
                APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == nGroupID)
                {
                    HandleSession();
                    return APIWS.api.Hit(oInitObj, nLocationInVideoSec, ref theMediaFileObject, nGroupID);
                }
                else
                {
                    Logger.Logger.Log("WS: TVAPI_Hit ignored due to group mimatch", "Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + nGroupID.ToString(), "api");
                    HttpContext.Current.Response.StatusCode = 404;
                    return false;
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("WS Exception: ", ex.Message + " || " + ex.StackTrace, "api");
            throw ex;
        }
    }

    #endregion

}

