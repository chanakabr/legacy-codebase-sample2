using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using ApiObjects;
using APIWS;
using System.Xml;
using System.IO;
using TVinciShared;
using System.Configuration;

namespace WS_API
{

    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://api.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    //[System.Web.Script.Services.ScriptService]
    public class API : System.Web.Services.WebService
    {
        #region Helper functions
        static protected Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName)
        {
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            Int32 nGroupID = TVinciShared.WS_Utils.GetGroupID("api", sFunctionName, sWSUserName, sWSPassword, sIP);
            if (nGroupID == 0)
                Logger.Logger.Log("WS ignored", "IP: " + sIP + ",Function: " + sFunctionName + " UN: " + sWSUserName + " Pass: " + sWSPassword, "api");
            return nGroupID;
        }
        #endregion

        #region Public API
        [WebMethod(EnableSession = true)]
        [System.Xml.Serialization.XmlInclude(typeof(UserIMRequestObject))]
        [System.Xml.Serialization.XmlInclude(typeof(FileRequestObject))]
        [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
        [System.Xml.Serialization.XmlInclude(typeof(LanguageRequestObject))]
        [System.Xml.Serialization.XmlInclude(typeof(PicObject))]
        [System.Xml.Serialization.XmlInclude(typeof(PlayerIMRequestObject))]
        public UserIMRequestObject TVAPI_GetTvinciGUID(string sWSUserName, string sWSPassword, InitializationObject oInitObj)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetTvinciGUID");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                        return APIWS.api.GetTvinciGUID(oInitObj, nGroupID);
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

        [WebMethod(EnableSession = true)]
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
        public ChannelObject TVAPI_GetMedias(string sWSUserName, string sWSPassword, InitializationObject oInitObj, Int32[] nMediaIDs, MediaInfoStructObject theInfoStruct)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetMedias");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                        return APIWS.api.SingleMedia(oInitObj, nGroupID, nMediaIDs, theInfoStruct);
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

        [WebMethod(EnableSession = true)]
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
        public MediaInfoStructObject TVAPI_GetMediaStructure(string sWSUserName, string sWSPassword, InitializationObject oInitObj)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetMediaStructure");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                        return APIWS.api.GetMediaStructure(oInitObj, nGroupID);
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

        static string GetXmlString(string strFile)
        {
            // Load the xml file into XmlDocument object.
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(strFile);
            }
            catch (XmlException e)
            {
                //Console.WriteLine(e.Message);
            }
            // Now create StringWriter object to get data from xml document.
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            xmlDoc.WriteTo(xw);
            return sw.ToString();
        }

        [WebMethod(EnableSession = true)]
        public int InsertEPGSchedule(string sWSUserName, string sWSPassword, int channelID, string fileName, bool isDelete)
        {
            int retVal = 0;
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetMediaStructure");
            string xml = GetXmlString(fileName);
            if (nGroupID != 0)
            {
                retVal = APIWS.api.InserEPGScheduleToChannel(nGroupID, channelID, xml, isDelete);
            }
            return retVal;
        }
        [WebMethod(EnableSession = true)]
        public List<EPGChannelObject> GetEPGChannel(string sWSUserName, string sWSPassword, string sPicSize)
        {


            List<EPGChannelObject> retVal = new List<EPGChannelObject>();
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetEPGChannel");

            if (nGroupID != 0)
            {
                retVal = APIWS.api.GetEPGChannel(nGroupID, sPicSize);
            }
            return retVal;

        }
        //[WebMethod(EnableSession = true)]
        //[System.Xml.Serialization.XmlInclude(typeof(EPGChannelProgrammeObject))]
        //[System.Xml.Serialization.XmlInclude(typeof(List<EPGChannelProgrammeObject>))]
        //public List<EPGChannelProgrammeObject> GetEPGChannelProgramme(string sWSUserName, string sWSPassword, string channelID, string sPicSize, EPGUnit oUnit, int nFromOffsetDay, int nToOffsetDay, int nUTCOffset)
        //{
        //    List<EPGChannelProgrammeObject> retVal = new List<EPGChannelProgrammeObject>();
        //    Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetEPGChannelProgramme");

        //    if (nGroupID != 0)
        //    {
        //        retVal = EPGLogic.GetEPGChannelPrograms(nGroupID, channelID, sPicSize, oUnit, nFromOffsetDay, nToOffsetDay, nUTCOffset);
        //    }
        //    return retVal;
        //}

        //[WebMethod(EnableSession = true)]
        //[System.Xml.Serialization.XmlInclude(typeof(EPGChannelProgrammeObject))]
        //[System.Xml.Serialization.XmlInclude(typeof(List<EPGChannelProgrammeObject>))]
        //public List<EPGChannelProgrammeObject> GetEPGChannelProgrammeByDates(string sWSUserName, string sWSPassword, string channelID, string sPicSize, DateTime fromDay, DateTime toDay, int nUTCOffset)
        //{
        //    List<EPGChannelProgrammeObject> retVal = new List<EPGChannelProgrammeObject>();
        //    Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetEPGChannelProgrammeByDates");

        //    if (nGroupID != 0)
        //    {
        //        retVal = EPGLogic.GetEPGChannelProgramsByDates(nGroupID, channelID, sPicSize, fromDay, toDay, nUTCOffset);
        //    }
        //    return retVal;
        //}


        //[WebMethod(EnableSession = true)]
        //[System.Xml.Serialization.XmlInclude(typeof(EPGChannelProgrammeObject))]
        //[System.Xml.Serialization.XmlInclude(typeof(List<EPGChannelProgrammeObject>))]
        //public List<EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgramme(string sWSUserName, string sWSPassword, string[] channelID, string sPicSize, EPGUnit oUnit, int nFromOffsetDay, int nToOffsetDay, int nUTCOffset)
        //{
        //    List<EPGMultiChannelProgrammeObject> retVal = new List<EPGMultiChannelProgrammeObject>();
        //    Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetEPGChannelProgramme");

        //    if (nGroupID != 0)
        //    {
        //        retVal = APIWS.api.GetEPGMultiChannelProgramme(nGroupID, channelID, sPicSize, oUnit, nFromOffsetDay, nToOffsetDay, nUTCOffset);
        //    }
        //    return retVal;
        //}

        //[WebMethod(EnableSession = true)]
        //[System.Xml.Serialization.XmlInclude(typeof(EPGChannelProgrammeObject))]
        //[System.Xml.Serialization.XmlInclude(typeof(List<EPGChannelProgrammeObject>))]
        //public List<EPGChannelProgrammeObject> SearchEPGContent(string sWSUserName, string sWSPassword, string sSearchValue, int nPageIndex, int nPageSize)
        //{
        //    List<EPGChannelProgrammeObject> retVal = new List<EPGChannelProgrammeObject>();
        //    Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetEPGChannelProgrammeByDates");

        //    if (nGroupID != 0)
        //    {
        //        retVal = EPGLogic.SearchEPGContent(nGroupID, sSearchValue, nPageIndex, nPageSize);
        //    }
        //    return retVal;
        //}

        [WebMethod(EnableSession = true)]
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
        public ChannelObject TVAPI_GetMediaInfo(string sWSUserName, string sWSPassword, InitializationObject oInitObj, Int32[] nMediaIDs, MediaInfoStructObject theInfoStruct)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_GetMediaInfo");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
                        oInitObj.m_oFileRequestObjects = null;

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

        [WebMethod(EnableSession = true)]
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
        public ChannelObject TVAPI_SearchMedia(string sWSUserName, string sWSPassword, InitializationObject oInitObj, SearchDefinitionObject oSearchDefinitionObj, MediaInfoStructObject theInfoStruct)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_SearchMedia");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod(EnableSession = true)]
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
        public ChannelObject TVAPI_SearchRelated(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nMediaID)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_SearchRelated");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod(EnableSession = true)]
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
        public ChannelObject TVAPI_NowPlaying(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_NowPlaying");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(AdminAccountUserResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(AdminAccountUserObj))]
        public AdminAccountUserResponse AdminSignIn(string sWSUserName, string sWSPassword, string username, string pass)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "AdminSignIn");
                if (nGroupID != 0 && nGroupID == 1)
                {

                    return APIWS.api.GetAdminUserAccount(username, pass);
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
                HttpContext.Current.Response.StatusCode = 404;
            }
            return null;
        }

        [WebMethod(EnableSession = true)]
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
        public ChannelObject TVAPI_UserLastWatched(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_UserLastWatched");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod(EnableSession = true)]
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
        public ChannelObject TVAPI_PeopleWhoWatchedAlsoWatched(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nMediaID, Int32 nMediaFileID)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_PeopleWhoWatchedAlsoWatched");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod(EnableSession = true)]
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
        public ChannelObject[] TVAPI_ChannelsMedia(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, ChannelRequestObject[] theChannelsRequestObj)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_ChannelsMedia");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod(EnableSession = true)]
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
        [System.Xml.Serialization.XmlInclude(typeof(GroupInfo))]
        public CategoryObject[] TVAPI_CategoriesTree(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nCategoryID, bool bWithChannels)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_CategoriesTree");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod(EnableSession = true)]
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
        public ChannelObject[] TVAPI_CategoryChannels(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nCategoryID)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_CategoryChannels");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod(EnableSession = true)]
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
        public ChannelObject[] TVAPI_UserSavedChannels(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_UserSavedChannels");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
                        return APIWS.api.UserChannels(oInitObj, theInfoStruct, nGroupID, "");
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

        [WebMethod(EnableSession = true)]
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
        public ChannelObject[] TVAPI_UserDeleteChannel(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nChannelID)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_UserDeleteChannel");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod(EnableSession = true)]
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
        public GenericWriteResponse TVAPI_UserSavePlaylist(string sWSUserName, string sWSPassword, InitializationObject oInitObj, Int32[] nMediaIDs, string sPlaylistTitle, bool bRewrite)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_UserSavePlaylist");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod(EnableSession = true)]
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
        public GenericWriteResponse TVAPI_SendMediaByEmail(string sWSUserName, string sWSPassword, InitializationObject oInitObj, Int32 nMediaID,
            string sFromEmail, string sToEmail, string sRecieverName, string sSenderName, string sContent)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_SendMediaByEmail");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod(EnableSession = true)]
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
        public ApiObjects.TagResponseObject[] TVAPI_TagValues(string sWSUserName, string sWSPassword, InitializationObject oInitObj, TagRequestObject[] oTagsDefinition)
        {
            try
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "TVAPI_TagValues");
                if (nGroupID != 0)
                {
                    APIWS.api.InitializeGroupNPlayer(ref oInitObj);
                    if (oInitObj.m_nGroupID == nGroupID)
                    {
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

        [WebMethod]
        public string CheckGeoBlockMedia(string sWSUserName, string sWSPassword, Int32 nMediaID, string sIP)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "CheckGeoBlockMedia");
            if (nGroupID != 0)
            {
                return APIWS.api.CheckGeoBlockMedia(nGroupID, nMediaID, sIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return "";
            }
        }

        /// <summary>
        /// Check subscription Geo Commerce Block Roule
        /// </summary>
        /// <param name="sWSUserName">Set web service user name</param>
        /// <param name="sWSPassword">Set web service password</param>
        /// <param name="SubscriptionGeoCommerceID">set subscription geo commerce roule ID</param>
        /// <param name="sIP">set client IP</param>
        /// <returns>return true if the enable purchase subscription in spasfic country by IP</returns>
        [WebMethod]
        public bool CheckGeoCommerceBlock(string sWSUserName, String sWSPassword, int SubscriptionGeoCommerceID, string sIP)
        {
            int nGroupID = GetGroupID(sWSUserName, sWSPassword, "CheckGeoCommerceBlock");
            if (nGroupID != 0)
            {
                return APIWS.api.IsGeoCommerceBlock(nGroupID, SubscriptionGeoCommerceID, sIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;

            }
        }


        #endregion

        #region Internal API functions
        [WebMethod]
        public Int32 GetMediaFileTypeID(string sWSUserName, string sWSPassword, Int32 nMediaFileID)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetMediaFileTypeID");
            if (nGroupID != 0)
            {
                return APIWS.api.GetMediaFileTypeID(nMediaFileID, nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return 0;
            }
        }

        //[WebMethod]
        //[System.Xml.Serialization.XmlInclude(typeof(UserStatus))]
        //public string SetAdminToken(string sWSUserName, string sWSPassword, string sIP , string sCountryCd2, string sLanguageFullName, string sDeviceName , UserStatus eUserStatus)
        //{
        //    Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "SetAdminToken");
        //    if (nGroupID != 0)
        //    {
        //        return APIWS.api.SetAdminToken(sIP , sCountryCd2 , sLanguageFullName , sDeviceName , eUserStatus , DateTime.UtcNow.AddHours(2) , nGroupID );
        //    }
        //    else
        //    {
        //        if (nGroupID == 0)
        //            HttpContext.Current.Response.StatusCode = 404;
        //        return "";
        //    }
        //}

        [WebMethod]

        public bool GetAdminTokenValues(string sWSUserName, string sWSPassword, string sIP, string sToken, ref string sCountryCd2, ref string sLanguageFullName, ref string sDeviceName, ref UserStatus eUserStatus)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetAdminTokenValues");
            if (nGroupID != 0)
            {
                return APIWS.api.GetAdminTokenValues(sIP, sToken, nGroupID, ref sCountryCd2, ref sLanguageFullName, ref sDeviceName, ref eUserStatus);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        public Int32[] GetChannelsMediaIDs(string sWSUserName, string sWSPassword, Int32[] nChannels, Int32[] nFileTypeIDs,
            bool bWithCache, string sDevice)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetChannelsMediaIDs");
            if (nGroupID != 0)
            {
                Int32[] nMedias = APIWS.api.GetChannelsMediaIDs(nChannels, nFileTypeIDs, bWithCache, nGroupID, sDevice);
                return nMedias;
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(FileTypeContainer))]
        public FileTypeContainer[] GetAvailableFileTypes(string sWSUserName, string sWSPassword)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetAvailableFileTypes");
            if (nGroupID != 0)
            {
                return APIWS.api.GetAvailableFileTypes(nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public Int32[] GetChannelMediaIDs(string sWSUserName, string sWSPassword, Int32 nChannelID, Int32[] nFileTypeIDs,
            bool bWithCache, string sDevice)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetChannelMediaIDs");
            if (nGroupID != 0)
            {
                Int32[] nMedias = APIWS.api.GetChannelsMediaIDs(new int[] { nChannelID }, nFileTypeIDs, bWithCache, nGroupID, sDevice);
                return nMedias;
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public bool DoesMediaBelongToChannels(string sWSUserName, string sWSPassword, Int32[] nChannels, Int32[] nFileTypeIDs,
            Int32 nMediaID, bool bWithCache, string sDevice)
        {

            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "DoesMediaBelongToChannels");
            // Int32 nDeviceID = TVinciShared.ProtocolsFuncs.GetDeviceIdFromName(sDevice, nGroupID);
            if (nGroupID != 0)
            {
                return APIWS.api.DoesMediaBelongToChannels(nChannels, nFileTypeIDs, nMediaID, bWithCache, nGroupID, sDevice);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        public bool ValidateBaseLink(string sWSUserName, string sWSPassword, Int32 nMediaFileID, string sBaseLink)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "ValidateBaseLink");
            if (nGroupID != 0)
            {
                return APIWS.api.ValidateBaseLink(nMediaFileID, sBaseLink, nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(MeidaMaper))]
        public MeidaMaper[] MapMediaFiles(string sWSUserName, string sWSPassword, Int32[] nMediaFileIDs)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "MapMediaFiles");
            if (nGroupID != 0)
            {
                return APIWS.api.MapMediaFiles(nMediaFileIDs, nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(MeidaMaper))]
        public MeidaMaper[] MapMediaFilesST(string sWSUserName, string sWSPassword, string sSeperatedMediaFileIDs)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "MapMediaFilesST");
            if (nGroupID != 0)
            {
                return APIWS.api.MapMediaFilesST(sSeperatedMediaFileIDs, nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(GroupInfo))]
        public GroupInfo[] GetSubGroupsTree(string sWSUserName, string sWSPassword, string sGroupName)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetSubGroupsTree");
            if (nGroupID != 0)
            {
                return APIWS.api.GetSubGroupsTree(sGroupName, nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public string[] GetGroupPlayers(string sWSUserName, string sWSPassword, string sGroupName, bool sIncludeChildGroups)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetGroupPlayers");
            if (nGroupID != 0)
            {
                return APIWS.api.GetGroupPlayers(sGroupName, sIncludeChildGroups, nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public string[] GetGroupMediaNames(string sWSUserName, string sWSPassword, string sGroupName)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetGroupMediaNames");
            if (nGroupID != 0)
            {
                return APIWS.api.GetGroupMediaNames(sGroupName, nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(MediaMarkObject))]
        public MediaMarkObject GetMediaMark(string sWSUserName, string sWSPassword, Int32 nMediaID, string sSiteGuid)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetMediaMark");
            if (nGroupID != 0)
            {
                return APIWS.api.GetMediaMark(nGroupID, nMediaID, sSiteGuid);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;

                MediaMarkObject mmo = new MediaMarkObject();
                mmo.eStatus = MediaMarkObject.MediaMarkObjectStatus.FAILED;

                return mmo;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(RateMediaObject))]
        public RateMediaObject RateMedia(string sWSUserName, string sWSPassword, Int32 nMediaID, string sSiteGuid, Int32 nRateVal)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetMediaMark");
            if (nGroupID != 0)
            {
                return APIWS.api.RateMedia(nGroupID, nMediaID, sSiteGuid, nRateVal);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;

                RateMediaObject rmo = new RateMediaObject();
                rmo.oStatus.Initialize("FAILED", -1);

                return rmo;
            }
        }



        [WebMethod]
        public bool AddUserSocialAction(string sWSUserName, string sWSPassword, Int32 nMediaID, string sSiteGuid, ApiObjects.SocialAction socialAction, ApiObjects.SocialPlatform socialPlatform)
        {
            int nSocialAction = (int)socialAction;
            int nSocialPlatform = (int)socialPlatform;
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "AddUserSocialAction");
            if (nGroupID != 0)
            {
                return APIWS.api.AddUserSocialAction(nGroupID, nMediaID, sSiteGuid, nSocialAction, nSocialPlatform);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return false;
        }




        [WebMethod]
        public bool RunImporter(string sWSUserName, string sWSPassword, string extraParams)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "RunImporter");
            if (nGroupID != 0)
            {
                return APIWS.api.RunImporter(nGroupID, extraParams);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return false;
        }

     
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PurchaseMailRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(PurchaseFailRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(WelcomeMailRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(ForgotPasswordMailRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(SendPasswordMailRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(ChangedPinMailRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(AddUserMailRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(AddDeviceMailRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(EmailNotificationRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(SendAdminTokenRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(ChangePasswordMailRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(PreviewModuleCancelOrRefundRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(PurchaseWithPreviewModuleRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(CinepolisPurchaseMailRequest))]
        [System.Xml.Serialization.XmlInclude(typeof(CinepolisRenewalFailMailRequest))]

        public bool SendMailTemplate(string sWSUserName, string sWSPassword, MailRequestObj request)   
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetGroupMediaRules");
            if (nGroupID != 0)
            {
                return APIWS.api.SendMailTemplate(request);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        public List<GroupRule> GetGroupRules(string sWSUserName, string sWSPassword)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetGroupRules");
            if (nGroupID != 0)
            {
                return APIWS.api.GetGroupRules(nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public List<string> GetAutoCompleteList(string sWSUserName, string sWSPassword, RequestObj request)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetGroupRules");
            if (nGroupID != 0)
            {
                return APIWS.api.GetAutoCompleteList(request, nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public List<GroupRule> GetUserGroupRules(string sWSUserName, string sWSPassword, string sSiteGuid)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetUserGroupRules");
            if (nGroupID != 0)
            {
                return APIWS.api.GetUserDomainGroupRules(nGroupID, sSiteGuid, 0);
                //return APIWS.api.GetUserGroupRules(nGroupID, sSiteGuid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public List<GroupRule> GetDomainGroupRules(string sWSUserName, string sWSPassword, int nDomainID)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetUserGroupRules");
            if (nGroupID != 0)
            {
                return APIWS.api.GetUserDomainGroupRules(nGroupID, null, nDomainID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public bool SendToFriend(string sWSUserName, string sWSPassword, string sSenderName, string sSenderMail, string sMailTo, string sNameTo, int nMediaID)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetUserGroupRules");
            if (nGroupID != 0)
            {
                return APIWS.api.SendToFriend(nGroupID, sSenderName, sSenderMail, sMailTo, sNameTo, nMediaID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        public GroupOperator[] GetGroupOperators(string sWSUserName, string sWSPassword, string sScope = "")
        {           
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetGroupOperators");
            if (nGroupID != 0)
            {
                return APIWS.api.GetGroupOperators(nGroupID, sScope);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public List<GroupOperator> GetOperator(string sWSUserName, string sWSPassword, List<int> operatorIds)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetOperator");
            if (nGroupID != 0) // TODO ask Arik
            {
                return APIWS.api.GetOperators(nGroupID, operatorIds);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual bool SetUserGroupRule(string sWSUserName, string sWSPassword, string sSiteGuid, int nRuleID, string sPIN, int nIsActive)
        {
            if (nIsActive > 1 || nIsActive < 0)
            {
                return false;
            }

            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "SetUserGroupRule");
            if (nGroupID != 0)
            {
                return APIWS.api.SetUserGroupRule(sSiteGuid, nRuleID, nIsActive, sPIN, nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        public virtual bool SetDomainGroupRule(string sWSUserName, string sWSPassword, int nDomainID, int nRuleID, string sPIN, int nIsActive)
        {
            if (nIsActive > 1 || nIsActive < 0)
            {
                return false;
            }

            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "SetDomainGroupRule");
            if (nGroupID != 0)
            {
                return APIWS.api.SetDomainGroupRule(nDomainID, nRuleID, nIsActive, sPIN, nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        public bool SetRuleState(string sWSUserName, string sWSPassword, int nDomainID, string sSiteGUID, int nRuleID, int nStatus)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "CheckParentalPIN");
            if (nGroupID != 0)
            {
                return APIWS.api.SetRuleState(sSiteGUID, nDomainID, nRuleID, nStatus);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }


        [WebMethod]
        public bool CheckParentalPIN(string sWSUserName, string sWSPassword, string sSiteGUID, int nRuleID, string sParentalPIN)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "CheckParentalPIN");
            if (nGroupID != 0)
            {
                return APIWS.api.CheckParentalPIN(sSiteGUID, nRuleID, sParentalPIN);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        public bool CheckDomainParentalPIN(string sWSUserName, string sWSPassword, int nDomainID, int nRuleID, string sParentalPIN)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "CheckDomainParentalPIN");
            if (nGroupID != 0)
            {
                return APIWS.api.CheckDomainParentalPIN(nDomainID, nRuleID, sParentalPIN);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }
        
        [WebMethod]
        public virtual bool SetDefaultRules(string sWSUserName, string sWSPassword, string sSiteGuid)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "SetDefaultRules");
            if (nGroupID != 0)
            {
                return APIWS.api.SetDefaultRules(sSiteGuid, nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return false;

            }
        }

        [WebMethod]
        public virtual DeviceAvailabiltyRule GetAvailableDevices(string sWSUserName, string sWSPassword, int nMediaID)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "SetDefaultRules");
            if (nGroupID != 0)
            {
                return APIWS.api.GetAvailableDevices(nMediaID, nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual bool CleanUserHistory(string sWSUserName, string sWSPassword, string siteGuid, List<int> lMediaIDs)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "CleanUserHistory");
            if (nGroupID != 0)
            {
                return APIWS.api.CleanUserHistory(nGroupID, siteGuid, lMediaIDs);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        //[WebMethod(EnableSession = true)]
        //[System.Xml.Serialization.XmlInclude(typeof(EPGChannelProgrammeObject))]
        //[System.Xml.Serialization.XmlInclude(typeof(List<EPGChannelProgrammeObject>))]
        //public List<EPGChannelProgrammeObject> GetEPGProgramsByScids(string sWSUserName, string sWSPassword, string[] scids, EpgFeeder.Language eLang, int duration)
        //{
        //    List<EPGChannelProgrammeObject> retVal = new List<EPGChannelProgrammeObject>();
        //    Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetEPGProgramsByScids");

        //    if (nGroupID != 0)
        //    {
        //        retVal = EPGLogic.GetEPGProgramsByScids(nGroupID, scids, eLang, duration);
        //    }
        //    return retVal;
        //}

        [WebMethod]
        public Scheduling GetProgramSchedule(string sWSUserName, string sWSPass, int nProgramId)
        {
            Scheduling schedule = null;

            if (!string.IsNullOrEmpty(nProgramId.ToString()))
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPass, "GetProgramSchedule");
                if (nGroupID != 0)
                {
                    schedule = APIWS.api.GetProgramSchedule(nProgramId, nGroupID);
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return schedule;
        }

        [WebMethod]
        public string GetCoGuidByMediaFileId(string sWSUserName, string sWSPassword, int nMediaFileID)
        {
            string sCoGuid = null;
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetCoGuidByMediaFileId");

            if (nGroupID != 0)
            {
                if (!string.IsNullOrEmpty(nMediaFileID.ToString()))
                {
                    sCoGuid = APIWS.api.GetCoGuidByMediaFileId(nMediaFileID);
                }
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return sCoGuid;
        }

        [WebMethod]
        public string[] GetUserStartedWatchingMedias(string sWSUserName, string sWSPassword, string sSiteGuid, int nNumOfItems)
        {
            string[] lMedias = { };
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetUserStartedWatchingMedias");
            if (nGroupID != 0)
            {
                lMedias = APIWS.api.GetUserStartedWatchingMedias(sSiteGuid, nNumOfItems).ToArray();
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return lMedias;
        }

        [WebMethod]
        public bool DoesMediaBelongToSubscription(string sWSUserName, string sWSPassword, int nSubscriptionCode, int[] nFileTypeIDs,
            int nMediaID, string sDevice)
        {

            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "DoesMediaBelongToSubscription");
            if (nGroupID != 0)
            {
                return APIWS.api.DoesMediaBelongToSubscription(nSubscriptionCode, nFileTypeIDs, nMediaID, sDevice, nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(GroupRule))]
        public List<GroupRule> GetGroupMediaRules(string sWSUserName, string sWSPassword, int nMediaID, int siteGuid, string sIP, string deviceUdid)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetGroupMediaRules");
            if (nGroupID != 0)
            {
                return APIWS.api.GetGroupMediaRules(nMediaID, sIP, siteGuid, nGroupID, deviceUdid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(GroupRule))]
        public List<GroupRule> GetEPGProgramRules(string sWSUserName, string sWSPassword, int nMediaId, int nProgramId, int siteGuid, string sIP, string deviceUdid)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetEPGProgramRules");
            if (nGroupID != 0)
            {
                return APIWS.api.GetEPGProgramRules(nProgramId, nMediaId, siteGuid, sIP, nGroupID, deviceUdid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        #endregion

        [WebMethod]
        public List<int> ChannelsContainingMedia(string sWSUserName, string sWSPassword, List<int> lChannels, int nMediaID, int nMediaFileID)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "ChannelsContainingMedia");
            if (nGroupID != 0)
            {
                return APIWS.api.ChannelsContainingMedia(lChannels, nMediaID, nGroupID, nMediaFileID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public List<int> GetSubscriptionMediaIds(string sWSUserName, string sWSPassword, int nSubscriptionCode, int[] nFileTypeIDs, string sDevice)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetSubscriptionMediaIds");
            if (nGroupID != 0)
            {
                return APIWS.api.GetSubscriptionMediaIds(nSubscriptionCode, nFileTypeIDs, sDevice, nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public List<int> GetMediaChannels(string sWSUserName, string sWSPassword, int nMediaId)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "GetMediaChannels");
            if (nGroupID != 0)
            {
                return APIWS.api.GetMediaChannels(nGroupID, nMediaId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(EPGChannelProgrammeObject))]
        public EPGChannelProgrammeObject GetProgramDetails(string sWSUserName, string sWSPass, int nProgramId)
        {
            EPGChannelProgrammeObject epg = new EPGChannelProgrammeObject();
            if (!string.IsNullOrEmpty(nProgramId.ToString()))
            {
                Int32 nGroupID = GetGroupID(sWSUserName, sWSPass, "GetProgramDetails");
                if (nGroupID != 0)
                {
                    epg = APIWS.api.GetProgramDetails(nProgramId, nGroupID);
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return epg;
        }

    }
}
