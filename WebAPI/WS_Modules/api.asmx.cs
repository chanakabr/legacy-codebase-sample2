using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;
using APILogic;
//using APILogic.Catalog;
using ApiObjects;
using ApiObjects.BulkExport;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.Rules;
using KLogMonitor;
using ApiObjects.TimeShiftedTv;
using ScheduledTasks;
using Core.Catalog.Response;


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
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static protected Int32 GetGroupID(string sWSUserName, string sWSPassword)
        {
            Credentials oCredentials = new Credentials(sWSUserName, sWSPassword);
            Int32 nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.API, oCredentials);

            if (nGroupID == 0)
                log.Debug("WS ignored eWSModules: eWSModules.API " + " UN: " + sWSUserName + " Pass: " + sWSPassword);

            return nGroupID;
        }

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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_GetTvinciGUID(nGroupID, oInitObj);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_GetMedias(nGroupID, oInitObj, nMediaIDs, theInfoStruct);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_GetMediaStructure(nGroupID, oInitObj);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
                throw ex;
            }
        }

        [WebMethod(EnableSession = true)]
        public int InsertEPGSchedule(string sWSUserName, string sWSPassword, int channelID, string fileName, bool isDelete)
        {
            int retVal = 0;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.InsertEPGSchedule(nGroupID, channelID, fileName, isDelete);
            }
            return retVal;
        }
        [WebMethod(EnableSession = true)]
        public List<EPGChannelObject> GetEPGChannel(string sWSUserName, string sWSPassword, string sPicSize)
        {


            List<EPGChannelObject> retVal = new List<EPGChannelObject>();
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Api.Module.GetEPGChannel(nGroupID, sPicSize);
            }
            return retVal;
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
        public ChannelObject TVAPI_GetMediaInfo(string sWSUserName, string sWSPassword, InitializationObject oInitObj, Int32[] nMediaIDs, MediaInfoStructObject theInfoStruct)
        {
            try
            {
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_GetMediaInfo(nGroupID, oInitObj, nMediaIDs, theInfoStruct);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_SearchMedia(nGroupID, oInitObj, oSearchDefinitionObj, theInfoStruct);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_SearchRelated(nGroupID, oInitObj, theInfoStruct, thePageDef, nMediaID);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_NowPlaying(nGroupID, oInitObj, theInfoStruct, thePageDef);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

                if (nGroupID != 0)
                {
                    return Core.Api.Module.AdminSignIn(nGroupID, username, pass);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_UserLastWatched(nGroupID, oInitObj, theInfoStruct, thePageDef);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_PeopleWhoWatchedAlsoWatched(nGroupID, oInitObj, theInfoStruct, nMediaID, nMediaFileID);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_ChannelsMedia(nGroupID, oInitObj, theInfoStruct, theChannelsRequestObj);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_CategoriesTree(nGroupID, oInitObj, theInfoStruct, nCategoryID, bWithChannels);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Debug("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_CategoryChannels(nGroupID, oInitObj, theInfoStruct, nCategoryID);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_UserSavedChannels(nGroupID, oInitObj, theInfoStruct);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_UserDeleteChannel(nGroupID, oInitObj, theInfoStruct, nChannelID);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_UserSavePlaylist(nGroupID, oInitObj, nMediaIDs, sPlaylistTitle, bRewrite);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_SendMediaByEmail(nGroupID, oInitObj, nMediaID, sFromEmail, sToEmail, sRecieverName, sSenderName, sContent);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
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
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.TVAPI_TagValues(nGroupID, oInitObj, oTagsDefinition);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
                throw ex;
            }
        }

        [WebMethod]
        public string CheckGeoBlockMedia(string sWSUserName, string sWSPassword, Int32 nMediaID, string sIP)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.CheckGeoBlockMedia(nGroupID, nMediaID, sIP);
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
            int nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.CheckGeoCommerceBlock(nGroupID, SubscriptionGeoCommerceID, sIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;

            }
        }

        [WebMethod]
        public Int32 GetMediaFileTypeID(string sWSUserName, string sWSPassword, Int32 nMediaFileID)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetMediaFileTypeID(nGroupID, nMediaFileID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return 0;
            }
        }

        [WebMethod]
        public string GetMediaFileTypeDescription(string sWSUserName, string sWSPassword, Int32 nMediaFileID)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetMediaFileTypeDescription(nGroupID, nMediaFileID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return string.Empty;
            }
        }

        [WebMethod]
        public bool GetAdminTokenValues(string sWSUserName, string sWSPassword, string sIP, string sToken, ref string sCountryCd2, ref string sLanguageFullName, ref string sDeviceName, ref UserStatus eUserStatus)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetAdminTokenValues(nGroupID, sIP, sToken, ref sCountryCd2, ref sLanguageFullName, ref sDeviceName, ref eUserStatus);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetChannelsMediaIDs(nGroupID, nChannels, nFileTypeIDs, bWithCache, sDevice);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        public List<int> GetChannelsAssetsIDs(string sWSUserName, string sWSPassword, Int32[] nChannels, Int32[] nFileTypeIDs, bool bWithCache, string sDevice, bool activeAssets, bool useStartDate)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetChannelsAssetsIDs(nGroupID, nChannels, nFileTypeIDs, bWithCache, sDevice, activeAssets, useStartDate);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
       

     

        [WebMethod]
        public UnifiedSearchResult[] GetChannelAssets(string username, string password, int channelId, int pageIndex, int pageSize)
        {
            int nGroupID = Utils.GetGroupID(username, password);

            if (nGroupID != 0)
            {
                return Core.Api.Module.GetChannelAssets(nGroupID, channelId, pageIndex, pageSize);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public UnifiedSearchResult[] SearchAssets(string username, string password, string filter, int pageIndex, int pageSize, bool OnlyIsActive, int languageID, bool UseStartDate,
               string Udid, string UserIP, string SiteGuid, int DomainId, int ExectGroupId, bool IgnoreDeviceRule)
        {
            int nGroupID = Utils.GetGroupID(username, password);

            if (nGroupID != 0)
            {
                return Core.Api.Module.SearchAssets(nGroupID, filter, pageIndex, pageSize, OnlyIsActive, languageID, UseStartDate, Udid, UserIP, SiteGuid, DomainId, ExectGroupId, IgnoreDeviceRule);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetAvailableFileTypes(nGroupID);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetChannelMediaIDs(nGroupID, nChannelID, nFileTypeIDs, bWithCache, sDevice);
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

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            // Int32 nDeviceID = TVinciShared.ProtocolsFuncs.GetDeviceIdFromName(sDevice, nGroupID);
            if (nGroupID != 0)
            {
                return Core.Api.Module.DoesMediaBelongToChannels(nGroupID, nChannels, nFileTypeIDs, nMediaID, bWithCache, sDevice);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.ValidateBaseLink(nGroupID, nMediaFileID, sBaseLink);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.MapMediaFiles(nGroupID, nMediaFileIDs);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.MapMediaFilesST(nGroupID, sSeperatedMediaFileIDs);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetSubGroupsTree(nGroupID, sGroupName);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetGroupPlayers(nGroupID, sGroupName, sIncludeChildGroups);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetGroupMediaNames(nGroupID, sGroupName);
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
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetMediaMark(nGroupID, nMediaID, sSiteGuid);
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
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.RateMedia(nGroupID, nMediaID, sSiteGuid, nRateVal);
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
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            int nSocialAction = (int)socialAction;
            int nSocialPlatform = (int)socialPlatform;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.AddUserSocialAction(nGroupID, nMediaID, sSiteGuid, socialAction, socialPlatform);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.RunImporter(nGroupID, extraParams);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.SendMailTemplate(nGroupID, request);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetGroupRules(nGroupID);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetAutoCompleteList(nGroupID, request);
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
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetUserGroupRules(nGroupID, sSiteGuid);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetDomainGroupRules(nGroupID, nDomainID);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.SendToFriend(nGroupID, sSenderName, sSenderMail, sMailTo, sNameTo, nMediaID);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetGroupOperators(nGroupID, sScope);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetOperator(nGroupID, operatorIds);
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
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            if (nIsActive > 1 || nIsActive < 0)
            {
                return false;
            }

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.SetUserGroupRule(nGroupID, sSiteGuid, nRuleID, sPIN, nIsActive);
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

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.SetDomainGroupRule(nGroupID, nDomainID, nRuleID, sPIN, nIsActive);
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
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.SetRuleState(nGroupID, nDomainID, sSiteGUID, nRuleID, nStatus);
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
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.CheckParentalPIN(nGroupID, sSiteGUID, nRuleID, sParentalPIN);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.CheckDomainParentalPIN(nGroupID, nDomainID, nRuleID, sParentalPIN);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [Obsolete]
        public virtual bool SetDefaultRules(string sWSUserName, string sWSPassword, string sSiteGuid)
        {
            return true;
            //// add siteguid to logs/monitor
            //HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            //Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            //if (nGroupID != 0)
            //{
            //    return Core.Api.Module.SetDefaultRules(sSiteGuid, nGroupID);
            //}
            //else
            //{
            //    if (nGroupID == 0)
            //    {
            //        HttpContext.Current.Response.StatusCode = 404;
            //    }
            //    return false;

            //}
        }

        [WebMethod]
        public virtual DeviceAvailabiltyRule GetAvailableDevices(string sWSUserName, string sWSPassword, int nMediaID)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetAvailableDevices(nGroupID, nMediaID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual ApiObjects.Response.Status CleanUserHistory(string sWSUserName, string sWSPassword, string siteGuid, List<int> lMediaIDs)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.CleanUserHistory(nGroupID, siteGuid, lMediaIDs);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
            }
        }

        [WebMethod]
        public Scheduling GetProgramSchedule(string sWSUserName, string sWSPass, int nProgramId)
        {
            if (!string.IsNullOrEmpty(nProgramId.ToString()))
            {
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPass);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.GetProgramSchedule(nGroupID, nProgramId);
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return null;
        }

        [WebMethod]
        public string GetCoGuidByMediaFileId(string sWSUserName, string sWSPassword, int nMediaFileID)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                if (!string.IsNullOrEmpty(nMediaFileID.ToString()))
                {
                    return Core.Api.Module.GetCoGuidByMediaFileId(nGroupID, nMediaFileID);
                }
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return null;
        }

        [WebMethod]
        public string[] GetUserStartedWatchingMedias(string sWSUserName, string sWSPassword, string sSiteGuid, int nNumOfItems)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetUserStartedWatchingMedias(nGroupID, sSiteGuid, nNumOfItems);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            string[] lMedias = { };
            return lMedias;
        }

        [WebMethod]
        public bool DoesMediaBelongToSubscription(string sWSUserName, string sWSPassword, int nSubscriptionCode, int[] nFileTypeIDs,
            int nMediaID, string sDevice)
        {

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.DoesMediaBelongToSubscription(nGroupID, nSubscriptionCode, nFileTypeIDs, nMediaID, sDevice);
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
        public bool DoesMediaBelongToCollection(string sWSUserName, string sWSPassword, int nCollectionCode, int[] nFileTypeIDs,
            int nMediaID, string sDevice)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.DoesMediaBelongToCollection(nGroupID, nCollectionCode, nFileTypeIDs, nMediaID, sDevice);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetGroupMediaRules(nGroupID, nMediaID, siteGuid, sIP, deviceUdid);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetEPGProgramRules(nGroupID, nMediaId, nProgramId, siteGuid, sIP, deviceUdid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(GroupRule))]
        public List<GroupRule> GetNpvrRules(string sWSUserName, string sWSPassword, RecordedEPGChannelProgrammeObject recordedProgram, int siteGuid, string sIP, string deviceUdid)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetNpvrRules(nGroupID, recordedProgram, siteGuid, sIP, deviceUdid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public List<int> ChannelsContainingMedia(string sWSUserName, string sWSPassword, List<int> lChannels, int nMediaID, int nMediaFileID)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.ChannelsContainingMedia(nGroupID, lChannels, nMediaID, nMediaFileID);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetSubscriptionMediaIds(nGroupID, nSubscriptionCode, nFileTypeIDs, sDevice);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public List<int> GetCollectionMediaIds(string sWSUserName, string sWSPassword, int nCollectionCode, int[] nFileTypeIDs, string sDevice)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetCollectionMediaIds(nGroupID, nCollectionCode, nFileTypeIDs, sDevice);
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
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetMediaChannels(nGroupID, nMediaId);
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
            if (!string.IsNullOrEmpty(nProgramId.ToString()))
            {
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPass);
                if (nGroupID != 0)
                {
                    return Core.Api.Module.GetProgramDetails(nGroupID, nProgramId);
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new EPGChannelProgrammeObject();
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(MediaConcurrencyRule))]
        public List<MediaConcurrencyRule> GetMediaConcurrencyRules(string sWSUserName, string sWSPassword, int nMediaID, string sIP, int bmID, eBusinessModule eType)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Api.Module.GetMediaConcurrencyRules(nGroupID, nMediaID, sIP, bmID, eType);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public RegionsResponse GetRegions(string sWSUserName, string sWSPassword, List<string> externalRegionList, RegionOrderBy orderBy)
        {
            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (groupID != 0)
            {
                return Core.Api.Module.GetRegions(groupID, externalRegionList, orderBy);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                RegionsResponse response = new RegionsResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                return response;
            }
        }

        [WebMethod]
        public List<LanguageObj> GetGroupLanguages(string sWSUserName, string sWSPassword)
        {
            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (groupID != 0)
            {
                return Core.Api.Module.GetGroupLanguages(groupID);
            }

            return null;
        }

        #region Parental Rules

        /// <summary>
        /// All of the parental rules for the account.
        /// Includes specification of what of which is the default rule/s for the account
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [WebMethod]
        public ParentalRulesResponse GetParentalRules(string userName, string password)
        {
            ParentalRulesResponse response = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetParentalRules(groupId);
            }
            else
            {
                response = new ParentalRulesResponse()
                {
                    status = new Status()
                    {
                        Code = (int)eResponseStatus.Error,
                        Message = "Error"
                    }
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Gets the parental rules that applies for the domain
        /// Includes distinction if rule was defined at account, HH or user level
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        [WebMethod]
        public ParentalRulesResponse GetDomainParentalRules(string userName, string password, int domainId)
        {
            ParentalRulesResponse response = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetDomainParentalRules(groupId, domainId);
            }
            else
            {
                response = new ParentalRulesResponse()
                {
                    status = new Status()
                    {
                        Code = (int)eResponseStatus.Error,
                        Message = "Error"
                    }
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Gets the parental rules that applies for the User.
        /// Includes distinction if rule was defined at account, HH or user level
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        [WebMethod]
        public ParentalRulesResponse GetUserParentalRules(string userName, string password, string siteGuid, int domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ParentalRulesResponse response = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetUserParentalRules(groupId, siteGuid, domainId);
            }
            else
            {
                response = new ParentalRulesResponse()
                {
                    status = new Status()
                    {
                        Code = (int)eResponseStatus.Error,
                        Message = "Error"
                    }
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Enable or disable a parental rule for the user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="webServicePassword"></param>
        /// <param name="siteGuid"></param>
        /// <param name="ruleId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        [WebMethod]
        public Status SetUserParentalRules(string userName, string webServicePassword, string siteGuid, long ruleId, int isActive, int domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            Status status = null;

            int groupId = Utils.GetGroupID(userName, webServicePassword);

            if (groupId > 0)
            {
                status = Core.Api.Module.SetUserParentalRules(groupId, siteGuid, ruleId, isActive, domainId);
            }
            else
            {
                status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        /// <summary>
        /// Enable or disable a parental rule for the domain 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="webServicePassword"></param>
        /// <param name="domainId"></param>
        /// <param name="ruleId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        [WebMethod]
        public Status SetDomainParentalRules(string userName, string webServicePassword, int domainId, long ruleId, int isActive)
        {
            Status status = null;

            int groupId = Utils.GetGroupID(userName, webServicePassword);

            if (groupId > 0)
            {
                status = Core.Api.Module.SetDomainParentalRules(groupId, domainId, ruleId, isActive);
            }
            else
            {
                status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        /// <summary>
        /// Get the parental PIN for the household or user.
        /// Includes specification of where the PIN was defined at – account, household or user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        [WebMethod]
        public PinResponse GetParentalPIN(string userName, string password, int domainId, string siteGuid, int? ruleId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PinResponse response = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetParentalPIN(groupId, domainId, siteGuid, ruleId);
            }
            else
            {
                response = new PinResponse()
                {
                    status = new Status()
                    {
                        Code = (int)eResponseStatus.Error,
                        Message = "Error"
                    }
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }


        /// <summary>
        /// Set a parental PIN for the household or user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        [WebMethod]
        public PinResponse UpdateParentalPIN(string userName, string password, int domainId, string siteGuid, string pin, int? ruleId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PinResponse response = new PinResponse();

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response.status = Core.Api.Module.SetParentalPIN(groupId, domainId, siteGuid, pin, ruleId);
                if (response.status.Code == (int)eResponseStatus.OK)
                    response = Core.Api.Module.GetParentalPIN(groupId, domainId, siteGuid, ruleId);
            }
            else
            {
                response.status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Set a parental PIN for the household or user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        [WebMethod]
        public Status SetParentalPIN(string userName, string password, int domainId, string siteGuid, string pin, int? ruleId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            Status status = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                status = Core.Api.Module.SetParentalPIN(groupId, domainId, siteGuid, pin, ruleId);
            }
            else
            {
                status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        /// <summary>
        /// Get purchase settings.
        /// Includes specification of where these settings were defined – account, household or user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        [WebMethod]
        public PurchaseSettingsResponse GetPurchaseSettings(string userName, string password, int domainId, string siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PurchaseSettingsResponse response = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetPurchaseSettings(groupId, domainId, siteGuid);
            }
            else
            {
                response = new PurchaseSettingsResponse()
                {
                    status = new Status()
                    {
                        Code = (int)eResponseStatus.Error,
                        Message = "Error"
                    }
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Set purchase settings for the household or user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        [WebMethod]
        [Obsolete]
        public Status SetPurchaseSettings(string userName, string password, int domainId, string siteGuid, int setting)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            Status status = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                status = Core.Api.Module.SetPurchaseSettings(groupId, domainId, siteGuid, setting);
            }
            else
            {
                status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        /// <summary>
        /// Set purchase settings for the household or user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        [WebMethod]
        public PurchaseSettingsResponse UpdatePurchaseSettings(string userName, string password, int domainId, string siteGuid, int setting)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PurchaseSettingsResponse response = new PurchaseSettingsResponse();

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response.status = Core.Api.Module.SetPurchaseSettings(groupId, domainId, siteGuid, setting);
                if (response.status.Code == (int)eResponseStatus.OK)
                    response = Core.Api.Module.GetPurchaseSettings(groupId, domainId, siteGuid);
            }
            else
            {
                response.status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Get the purchase PIN for the household or user.
        /// Includes specification of where the PIN was defined at – household or user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        [WebMethod]
        public PurchaseSettingsResponse GetPurchasePIN(string userName, string password, int domainId, string siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PurchaseSettingsResponse response = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetPurchasePIN(groupId, domainId, siteGuid);
            }
            else
            {
                response = new PurchaseSettingsResponse()
                {
                    status = new Status()
                    {
                        Code = (int)eResponseStatus.Error,
                        Message = "Error"
                    }
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Set purchase pin for household or user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        [WebMethod]
        public PurchaseSettingsResponse UpdatePurchasePIN(string userName, string password, int domainId, string siteGuid, string pin)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PurchaseSettingsResponse response = new PurchaseSettingsResponse();

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response.status = Core.Api.Module.SetPurchasePIN(groupId, domainId, siteGuid, pin);
                if (response.status.Code == (int)eResponseStatus.OK)
                    response = Core.Api.Module.GetPurchasePIN(groupId, domainId, siteGuid);
            }
            else
            {
                response.status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Set purchase pin for household or user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        [WebMethod]
        [Obsolete]
        public Status SetPurchasePIN(string userName, string password, int domainId, string siteGuid, string pin)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            Status status = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                status = Core.Api.Module.SetPurchasePIN(groupId, domainId, siteGuid, pin);
            }
            else
            {
                status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        /// <summary>
        /// Validate that a given parental PIN for a user is valid.
        /// Take into account PIN definition hierarchy.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="siteGuid"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        [WebMethod]
        public Status ValidateParentalPIN(string userName, string password, string siteGuid, string pin, int domainId, int? ruleId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            Status status = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                status = Core.Api.Module.ValidateParentalPIN(groupId, siteGuid, pin, domainId, ruleId);
            }
            else
            {
                status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        /// <summary>
        /// Validate that a given purchase PIN for a user is valid.
        /// Take into account PIN definition hierarchy.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="siteGuid"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        [WebMethod]
        public Status ValidatePurchasePIN(string userName, string password, string siteGuid, string pin, int domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            Status status = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                status = Core.Api.Module.ValidatePurchasePIN(groupId, siteGuid, pin, domainId);
            }
            else
            {
                status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        /// <summary>
        /// Get all the rules that applies for a specific media and a specific user according to the user parental settings.
        /// Take into account that rules are hierarchically defined – and get the rules that applies for the user regardless of where the rule was defined
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="siteGuid"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        [WebMethod]
        public ParentalRulesResponse GetParentalMediaRules(string userName, string password, string siteGuid, long mediaId, long domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ParentalRulesResponse response = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetParentalMediaRules(groupId, siteGuid, mediaId, domainId);
            }
            else
            {
                response = new ParentalRulesResponse()
                {
                    status = new Status()
                    {
                        Code = (int)eResponseStatus.Error,
                        Message = "Error"
                    }
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Get all the rules that applies for a specific EPG and a specific user according to the user parental settings.
        /// Take into account that rules are hierarchically defined – and get the rules that applies for the user regardless of where the rule was defined
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="siteGuid"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        [WebMethod]
        public ParentalRulesResponse GetParentalEPGRules(string userName, string password, string siteGuid, long epgId, long domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            ParentalRulesResponse response = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetParentalEPGRules(groupId, siteGuid, epgId, domainId);
            }
            else
            {
                response = new ParentalRulesResponse()
                {
                    status = new Status()
                    {
                        Code = (int)eResponseStatus.Error,
                        Message = "Error"
                    }
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }


        /// <summary>
        /// Disable the default parental rule for the user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="webServicePassword"></param>
        /// <param name="siteGuid"></param>
        /// <param name="ruleId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        [WebMethod]
        public Status DisableUserDefaultParentalRule(string userName, string webServicePassword, string siteGuid, int domainId)
        {
            Status status = null;

            int groupId = GetGroupID(userName, webServicePassword);

            if (groupId > 0)
            {
                status = Core.Api.Module.SetUserParentalRules(groupId, siteGuid, -1, 1, domainId);
            }
            else
            {
                status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        /// <summary>
        /// Disable the default parental rule for the domain 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="webServicePassword"></param>
        /// <param name="domainId"></param>
        /// <param name="ruleId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        [WebMethod]
        public Status DisableDomainDefaultParentalRule(string userName, string webServicePassword, int domainId)
        {
            Status status = null;

            int groupId = GetGroupID(userName, webServicePassword);

            if (groupId > 0)
            {
                status = Core.Api.Module.SetDomainParentalRules(groupId, domainId, -1, 1);
            }
            else
            {
                status = new Status()
                {
                    Code = (int)eResponseStatus.Error,
                    Message = "Error"
                };

                HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        /// <summary>
        /// Retrieve all the rules (parental, geo, device or user-type) that applies for this user and media 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="webServicePassword"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="mediaId"></param>
        /// <param name="ip"></param>
        /// <param name="udid"></param>
        /// <returns></returns>
        [WebMethod]
        public GenericRuleResponse GetMediaRules(string userName, string webServicePassword, string siteGuid, long mediaId, long domainId, string ip, string udid, GenericRuleOrderBy orderBy)
        {
            GenericRuleResponse response = null;

            int groupId = GetGroupID(userName, webServicePassword);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetMediaRules(groupId, siteGuid, mediaId, domainId, ip, udid, orderBy);
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Retrieve all the rules (parental and not parental) that applies for this EPG program 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="webServicePassword"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param
        /// <param name="epgId"></param>>
        /// <param name="channelMediaId"></param>
        /// <returns></returns>
        [WebMethod]
        public GenericRuleResponse GetEpgRules(string userName, string webServicePassword, string siteGuid, long epgId, long channelMediaId, long domainId, string ip, GenericRuleOrderBy orderBy)
        {
            GenericRuleResponse response = null;

            int groupId = GetGroupID(userName, webServicePassword);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetEpgRules(groupId, siteGuid, epgId, channelMediaId, domainId, ip, orderBy);
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");

                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ParentalRulesTagsResponse GetUserParentalRuleTags(string userName, string password, string siteGuid, long domainId)
        {

            if (HttpContext.Current != null && HttpContext.Current.Items != null)
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";
            }

            ParentalRulesTagsResponse response = null;

            int groupId = Utils.GetGroupID(userName, password);

            if (groupId > 0)
            {
                response = Core.Api.Module.GetUserParentalRuleTags(groupId, siteGuid, domainId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        #endregion

        [WebMethod]
        public bool BuildIPToCountryIndex(string userName, string password)
        {
            int groupID = Utils.GetGroupID(userName, password);

            if (groupID != 0)
            {
                return Core.Api.Module.BuildIPToCountryIndex(groupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return false;
        }

        [WebMethod]
        public StatusErrorCodesResponse GetErrorCodesDictionary(string userName, string webServicePassword)
        {
            int groupId = GetGroupID(userName, webServicePassword);

            if (groupId > 0)
            {
                return Core.Api.Module.GetErrorCodesDictionary();
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new StatusErrorCodesResponse();
        }

        #region OSS Adpater
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(OSSAdapterResponse))]
        public OSSAdapterResponse InsertOSSAdapter(string sWSUserName, string sWSPassword, OSSAdapter ossAdapter)
        {
            OSSAdapterResponse response = new OSSAdapterResponse();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.InsertOSSAdapter(groupID, ossAdapter);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status DeleteOSSAdapter(string sWSUserName, string sWSPassword, int ossAdapterID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.DeleteOSSAdapter(groupID, ossAdapterID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public OSSAdapterResponse SetOSSAdapter(string sWSUserName, string sWSPassword, OSSAdapter ossAdapter)
        {
            OSSAdapterResponse response = new OSSAdapterResponse();


            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.SetOSSAdapter(groupID, ossAdapter);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(OSSAdapterResponseList))]
        public OSSAdapterResponseList GetOSSAdapter(string sWSUserName, string sWSPassword)
        {
            OSSAdapterResponseList response = new OSSAdapterResponseList();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.GetOSSAdapter(groupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new OSSAdapterResponseList();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(OSSAdapterResponseList))]
        public OSSAdapterResponse GetOSSAdapterProfile(string sWSUserName, string sWSPassword, int ossAdapterId)
        {
            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.GetOSSAdapterProfile(groupID, ossAdapterId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                OSSAdapterResponse  response = new OSSAdapterResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status InsertOSSAdapterSettings(string sWSUserName, string sWSPassword, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.InsertOSSAdapterSettings(groupID, ossAdapterId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status SetOSSAdapterSettings(string sWSUserName, string sWSPassword, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.SetOSSAdapterSettings(groupID, ossAdapterId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status DeleteOSSAdapterSettings(string sWSUserName, string sWSPassword, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.DeleteOSSAdapterSettings(groupID, ossAdapterId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(OSSAdapterSettingsResponse))]
        public OSSAdapterSettingsResponse GetOSSAdapterSettings(string sWSUserName, string sWSPassword)
        {
            OSSAdapterSettingsResponse response = new OSSAdapterSettingsResponse();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.GetOSSAdapterSettings(groupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new OSSAdapterSettingsResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public OSSAdapterBillingDetailsResponse GetUserBillingDetails(string sWSUserName, string sWSPassword, long householdId, int ossAdapterId, string userIP)
        {
            OSSAdapterBillingDetailsResponse response = new OSSAdapterBillingDetailsResponse();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.GetUserBillingDetails(groupID, householdId, ossAdapterId, userIP);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new OSSAdapterBillingDetailsResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status SetOSSAdapterConfiguration(string sWSUserName, string sWSPassword, int ossAdapterId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.SetOSSAdapterConfiguration(groupID, ossAdapterId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public OSSAdapterResponse GenerateOSSSharedSecret(string sWSUserName, string sWSPassword, int ossAdapterId)
        {
            OSSAdapterResponse response = new OSSAdapterResponse();


            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.GenerateOSSSharedSecret(groupID, ossAdapterId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }


        #endregion

        [WebMethod]
        public bool UpdateCache(int groupId, string bucket, string[] keys)
        {
            return Core.Api.Module.UpdateCache(groupId, bucket, keys);
        }

        [WebMethod]
        public bool UpdateGeoBlockRulesCache(int groupId)
        {
            return Core.Api.Module.UpdateGeoBlockRulesCache(groupId);
        }

        #region Recommendation Engine
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(RecommendationEngineResponse))]
        public RecommendationEngineResponse InsertRecommendationEngine(string sWSUserName, string sWSPassword, RecommendationEngine recommendationEngine)
        {
            RecommendationEngineResponse response = new RecommendationEngineResponse();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.InsertRecommendationEngine(groupID, recommendationEngine);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status DeleteRecommendationEngine(string sWSUserName, string sWSPassword, int recommendationEngineId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.DeleteRecommendationEngine(groupID, recommendationEngineId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public RecommendationEngineResponse SetRecommendationEngine(string sWSUserName, string sWSPassword, RecommendationEngine recommendationEngine)
        {
            RecommendationEngineResponse response = new RecommendationEngineResponse();


            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.SetRecommendationEngine(groupID, recommendationEngine);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(RecommendationEnginesResponseList))]
        public RecommendationEnginesResponseList GetRecommendationEngines(string sWSUserName, string sWSPassword)
        {
            RecommendationEnginesResponseList response = new RecommendationEnginesResponseList();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.GetRecommendationEngines(groupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new RecommendationEnginesResponseList();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(RecommendationEnginesResponseList))]
        public RecommendationEnginesResponseList ListRecommendationEngines(string sWSUserName, string sWSPassword)
        {
            RecommendationEnginesResponseList response = new RecommendationEnginesResponseList();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.ListRecommendationEngines(groupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new RecommendationEnginesResponseList();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status InsertRecommendationEngineSettings(string sWSUserName, string sWSPassword, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.InsertRecommendationEngineSettings(groupID, recommendationEngineId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status SetRecommendationEngineSettings(string sWSUserName, string sWSPassword, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.SetRecommendationEngineSettings(groupID, recommendationEngineId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status DeleteRecommendationEngineSettings(string sWSUserName, string sWSPassword, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.DeleteRecommendationEngineSettings(groupID, recommendationEngineId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(RecommendationEngineSettinsResponse))]
        public RecommendationEngineSettinsResponse GetRecommendationEngineSettings(string sWSUserName, string sWSPassword)
        {
            RecommendationEngineSettinsResponse response = new RecommendationEngineSettinsResponse();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.GetRecommendationEngineSettings(groupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new RecommendationEngineSettinsResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(RecommendationEngineSettinsResponse))]
        public RecommendationEngineResponse UpdateRecommendationEngineConfiguration(string userName, string password, int recommendationEngineId)
        {
            RecommendationEngineResponse response = new RecommendationEngineResponse();

            Int32 groupID = Utils.GetGroupID(userName, password);

            if (groupID != 0)
            {
                return Core.Api.Module.UpdateRecommendationEngineConfiguration(groupID, recommendationEngineId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new RecommendationEngineResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public RecommendationEngineResponse GenerateRecommendationEngineSharedSecret(string sWSUserName, string sWSPassword, int recommendationEngineId)
        {
            RecommendationEngineResponse response = new RecommendationEngineResponse();


            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.GenerateRecommendationEngineSharedSecret(groupID, recommendationEngineId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        #endregion

        #region External Channel
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ExternalChannelResponse))]
        public ExternalChannelResponse
            InsertExternalChannel(string sWSUserName, string sWSPassword, ExternalChannel externalChannel)
        {
            ExternalChannelResponse response = new ExternalChannelResponse();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.InsertExternalChannel(groupID, externalChannel);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status DeleteExternalChannel(string sWSUserName, string sWSPassword, int externalChannelId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.DeleteExternalChannel(groupID, externalChannelId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ExternalChannelResponse SetExternalChannel(string sWSUserName, string sWSPassword, ExternalChannel externalChannel)
        {
            ExternalChannelResponse response = new ExternalChannelResponse();


            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.SetExternalChannel(groupID, externalChannel);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ExternalChannelResponseList))]
        public ExternalChannelResponseList GetExternalChannels(string sWSUserName, string sWSPassword)
        {
            ExternalChannelResponseList response = new ExternalChannelResponseList();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.GetExternalChannels(groupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ExternalChannelResponseList();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ExternalChannelResponseList))]
        public ExternalChannelResponseList ListExternalChannels(string sWSUserName, string sWSPassword)
        {
            ExternalChannelResponseList response = new ExternalChannelResponseList();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return Core.Api.Module.ListExternalChannels(groupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ExternalChannelResponseList();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }
        #endregion

        [WebMethod]
        public BulkExportTaskResponse AddBulkExportTask(string sWSUserName, string sWSPassword, string externalKey, string name, eBulkExportDataType dataType, string filter,
            eBulkExportExportType exportType, long frequency, string notificationUrl, List<int> vodTypes, bool isActive)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.AddBulkExportTask(groupId, externalKey, name, dataType, filter, exportType, frequency, notificationUrl, vodTypes, isActive);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new BulkExportTaskResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };;
        }

        [WebMethod]
        public BulkExportTaskResponse UpdateBulkExportTask(string sWSUserName, string sWSPassword, long id, string externalKey, string name, eBulkExportDataType dataType,
            string filter, eBulkExportExportType exportType, long frequency, string notificationUrl, List<int> vodTypes, bool? isActive)
        {
            BulkExportTaskResponse response = new BulkExportTaskResponse();

            int groupId = GetGroupID(sWSUserName, sWSPassword);

            if (groupId > 0)
            {
                return Core.Api.Module.UpdateBulkExportTask(groupId, id, externalKey, name, dataType, filter, exportType, frequency, notificationUrl, vodTypes, isActive);
            }
            else
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public Status DeleteBulkExportTask(string sWSUserName, string sWSPassword, long id, string externalKey)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.DeleteBulkExportTask(groupId, id, externalKey);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

        [WebMethod]
        public BulkExportTasksResponse GetBulkExportTasks(string sWSUserName, string sWSPassword, List<long> ids, List<string> externalKeys, BulkExportTaskOrderBy orderBy)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetBulkExportTasks(groupId, ids, externalKeys, orderBy);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new BulkExportTasksResponse() { Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };
        }

        [WebMethod]
        public bool Export(string sWSUserName, string sWSPassword, long taskId, string version)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.Export(groupId, taskId, version);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return false;
        }

        [WebMethod]
        public Status EnqueueExportTask(string sWSUserName, string sWSPassword, long taskId)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.EnqueueExportTask(groupId, taskId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

        [WebMethod]
        public Status MessageRecovery(string sWSUserName, string sWSPassword, long baseDateSec, List<string> messageDataTypes)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.MessageRecovery(groupId, baseDateSec, messageDataTypes);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

        [WebMethod]
        [XmlInclude(typeof(ApiActionPermissionItem))]
        [XmlInclude(typeof(GroupPermission))]
        public RolesResponse GetRoles(string sWSUserName, string sWSPassword, List<long> roleIds)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetRoles(groupId, roleIds);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new RolesResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };
        }

        [WebMethod]
        [XmlInclude(typeof(ApiActionPermissionItem))]
        [XmlInclude(typeof(ApiParameterPermissionItem))]
        [XmlInclude(typeof(ApiArgumentPermissionItem))]
        [XmlInclude(typeof(GroupPermission))]
        public PermissionsResponse GetPermissions(string sWSUserName, string sWSPassword, List<long> permissionIds)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetPermissions(groupId, permissionIds);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new PermissionsResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };
        }

        [WebMethod]
        [XmlInclude(typeof(ApiActionPermissionItem))]
        [XmlInclude(typeof(GroupPermission))]
        public PermissionResponse AddPermission(string sWSUserName, string sWSPassword, string name, List<long> permissionItemsIds, ePermissionType type, string usersGroup, long updaterId)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.AddPermission(groupId, name, permissionItemsIds, type, usersGroup, updaterId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new PermissionResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };
        }

        [WebMethod]
        public Status AddPermissionToRole(string sWSUserName, string sWSPassword, long roleId, long permissionId)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.AddPermissionToRole(groupId, roleId, permissionId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

        [WebMethod]
        public Status AddPermissionItemToPermission(string sWSUserName, string sWSPassword, long permissionId, long permissionItemId)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.AddPermissionItemToPermission(groupId, permissionId, permissionItemId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

        #region KSQL Channel
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(KSQLChannelResponse))]
        public KSQLChannelResponse
            InsertKSQLChannel(string sWSUserName, string sWSPassword, KSQLChannel channel)
        {
            KSQLChannelResponse response = new KSQLChannelResponse();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return APILogic.CRUD.KSQLChannelsManager.Insert(groupID, channel);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status DeleteKSQLChannel(string sWSUserName, string sWSPassword, int channelId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return APILogic.CRUD.KSQLChannelsManager.Delete(groupID, channelId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public KSQLChannelResponse SetKSQLChannel(string sWSUserName, string sWSPassword, KSQLChannel channel)
        {
            KSQLChannelResponse response = new KSQLChannelResponse();


            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return APILogic.CRUD.KSQLChannelsManager.Set(groupID, channel);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ExternalChannelResponseList))]
        public KSQLChannelResponse GetKSQLChannel(string sWSUserName, string sWSPassword, int channelId)
        {
            KSQLChannelResponse response = new KSQLChannelResponse();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return APILogic.CRUD.KSQLChannelsManager.Get(groupID, channelId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new KSQLChannelResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ExternalChannelResponseList))]
        public KSQLChannelResponseList GetKSQLChannels(string sWSUserName, string sWSPassword)
        {
            KSQLChannelResponseList response = new KSQLChannelResponseList();

            Int32 groupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (groupID != 0)
            {
                return APILogic.CRUD.KSQLChannelsManager.List(groupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new KSQLChannelResponseList();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }
        #endregion

        [WebMethod]
        [XmlInclude(typeof(eTableStatus))]
        public bool UpdateImageState(string sWSUserName, string sWSPassword, long rowId, int version, eMediaType mediaType, eTableStatus status)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.UpdateImageState(groupId, rowId, version, mediaType, status);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return false;
        }

        [WebMethod]
        public OSSAdapterEntitlementsResponse GetExternalEntitlements(string sWSUserName, string sWSPassword, string userId)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetExternalEntitlements(groupId, userId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new OSSAdapterEntitlementsResponse();
        }


        [WebMethod]
        public bool ModifyCB(string sWSUserName, string sWSPassword, string bucket, string key, eDbActionType action, string data, long ttlMinutes)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.ModifyCB(groupId, bucket, key, action, data, ttlMinutes);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return false;
        }

        [WebMethod]
        public RegistryResponse GetAllRegistry(string sWSUserName, string sWSPassword)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetAllRegistry(groupId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                RegistryResponse response = new RegistryResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public int GetGroupIdByUsernamePassword(string username, string password)
        {
            try
            {
                return Core.Api.Module.GetGroupIdByUsernamePassword(username, password);
            }
            catch (Exception ex)
            {
                log.Error("Error while GetGroupIdByUsernamePassword", ex);
            }
            return 0;
        }

        [WebMethod]
        public bool InitializeFreeItemsUpdate(string userName, string password)
        {
            int groupID = Utils.GetGroupID(userName, password);
            if (groupID != 0)
            {
                return Core.Api.Module.InitializeFreeItemsUpdate(groupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return false;
        }

        [WebMethod]
        public bool UpdateFreeFileTypeOfModule(int groupID, int moduleID)
        {
            if (groupID > 0)
            {
                return Core.Api.Module.UpdateFreeFileTypeOfModule(groupID, moduleID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return false;
        }

        [WebMethod]
        public TimeShiftedTvPartnerSettingsResponse GetTimeShiftedTvPartnerSettings(string sWSUserName, string sWSPassword)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetTimeShiftedTvPartnerSettings(groupId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                // just init response - the constructor creates status as error and settings as null
                return new TimeShiftedTvPartnerSettingsResponse();
            }
        }

        [WebMethod]
        public Status UpdateTimeShiftedTvPartnerSettings(string sWSUserName, string sWSPassword, TimeShiftedTvPartnerSettings settings)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);

            if (groupId > 0)
            {
                return Core.Api.Module.UpdateTimeShiftedTvPartnerSettings(groupId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                Status response = new Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }


        [WebMethod]
        public Status UpdateTimeShiftedTvEpgChannelsSettings(string sWSUserName, string sWSPassword, TimeShiftedTvPartnerSettings settings)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.UpdateTimeShiftedTvEpgChannelsSettings(groupId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                Status response = new Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.CDNAdapter.CDNAdapterListResponse GetCDNAdapters(string sWSUserName, string sWSPassword)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetCDNAdapters(groupId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                ApiObjects.CDNAdapter.CDNAdapterListResponse response = new ApiObjects.CDNAdapter.CDNAdapterListResponse();
                response.Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        [WebMethod]
        public Status DeleteCDNAdapter(string sWSUserName, string sWSPassword, int adapterId)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.DeleteCDNAdapter(groupId, adapterId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                response = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.CDNAdapter.CDNAdapterResponse InsertCDNAdapter(string sWSUserName, string sWSPassword, ApiObjects.CDNAdapter.CDNAdapter adapter)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.InsertCDNAdapter(groupId, adapter);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                ApiObjects.CDNAdapter.CDNAdapterResponse response = new ApiObjects.CDNAdapter.CDNAdapterResponse();
                response.Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.CDNAdapter.CDNAdapterResponse GenerateCDNSharedSecret(string sWSUserName, string sWSPassword, int adapterId)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);

            if (groupId > 0)
            {
                return Core.Api.Module.GenerateCDNSharedSecret(groupId, adapterId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                ApiObjects.CDNAdapter.CDNAdapterResponse response = new ApiObjects.CDNAdapter.CDNAdapterResponse();
                response.Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.CDNAdapter.CDNAdapterResponse SetCDNAdapter(string sWSUserName, string sWSPassword, ApiObjects.CDNAdapter.CDNAdapter adapter, int adapterID)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.SetCDNAdapter(groupId, adapter, adapterID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                ApiObjects.CDNAdapter.CDNAdapterResponse response = new ApiObjects.CDNAdapter.CDNAdapterResponse();
                response.Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        [WebMethod]
        public ApiObjects.CDNAdapter.CDNAdapterResponse SendCDNAdapterConfiguration(string sWSUserName, string sWSPassword, int adapterID)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.SendCDNAdapterConfiguration(groupId, adapterID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                ApiObjects.CDNAdapter.CDNAdapterResponse response = new ApiObjects.CDNAdapter.CDNAdapterResponse();
                response.Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return response;
            }
        }

        [WebMethod]
        public CDNPartnerSettingsResponse GetCDNPartnerSettings(string sWSUserName, string sWSPassword)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetCDNPartnerSettings(groupId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new CDNPartnerSettingsResponse();
        }

        [WebMethod]
        public CDNPartnerSettingsResponse UpdateCDNPartnerSettings(string sWSUserName, string sWSPassword, CDNPartnerSettings settings)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.UpdateCDNPartnerSettings(groupId, settings);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new CDNPartnerSettingsResponse();
        }

        [WebMethod]
        public ApiObjects.CDNAdapter.CDNAdapterResponse GetCDNAdapter(string sWSUserName, string sWSPassword, int adapterId)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetCDNAdapter(groupId, adapterId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new ApiObjects.CDNAdapter.CDNAdapterResponse();
        }

        [WebMethod]
        public ApiObjects.CDNAdapter.CDNAdapterResponse GetGroupDefaultCDNAdapter(string sWSUserName, string sWSPassword, eAssetTypes assetType)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetGroupDefaultCDNAdapter(groupId, assetType);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return new ApiObjects.CDNAdapter.CDNAdapterResponse();
        }

        [WebMethod]
        public bool MigrateStatistics(string userName, string password, DateTime? startDate)
        {
            int groupID = Utils.GetGroupID(userName, password);
            if (groupID != 0)
            {
                return Core.Api.Module.MigrateStatistics(groupID, startDate);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return false;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ScheduledTaskLastRunDetails))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseScheduledTaskLastRunDetails))]
        public ScheduledTaskLastRunDetails GetScheduledTaskLastRun(ApiObjects.ScheduledTaskType scheduledTaskType)
        {
            return Core.Api.Module.GetScheduledTaskLastRun(scheduledTaskType);
        }

        [WebMethod]
        public bool UpdateScheduledTaskNextRunIntervalInSeconds(ApiObjects.ScheduledTaskType scheduledTaskType, double nextRunIntervalInSeconds)
        {
            return Core.Api.Module.UpdateScheduledTaskNextRunIntervalInSeconds(scheduledTaskType, nextRunIntervalInSeconds);
        }

        [WebMethod]
        public DeviceFamilyResponse GetDeviceFamilyList(string sWSUserName, string sWSPassword)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetDeviceFamilyList(groupId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return null;
        }

        [WebMethod]
        public DeviceBrandResponse GetDeviceBrandList(string sWSUserName, string sWSPassword)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetDeviceBrandList(groupId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return null;
        }

        [WebMethod]
        public ApiObjects.CountryResponse GetCountryList(string sWSUserName, string sWSPassword, List<int> countryIds)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetCountryList(groupId, countryIds);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return null;
        }

        [WebMethod]
        public MetaResponse GetGroupMetaList(string sWSUserName, string sWSPassword, eAssetTypes assetType, MetaType metaType, MetaFieldName fieldNameEqual, MetaFieldName fieldNameNotEqual)
        {
            int groupId = GetGroupID(sWSUserName, sWSPassword);
            if (groupId > 0)
            {
                return Core.Api.Module.GetGroupMetaList(groupId, assetType, metaType, fieldNameEqual, fieldNameNotEqual);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return null;
        }
    }
}
