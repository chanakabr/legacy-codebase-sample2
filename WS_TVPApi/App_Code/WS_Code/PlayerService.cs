using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.DataLoaders;
using System.Xml;
using TVPPro.SiteManager.Services;
using TVPApi;
using TVPApiModule.Services;
using System.ServiceModel;
using TVPPro.Configuration.Technical;
using TVPApiModule.Interfaces;
using KLogMonitor;
using System.Reflection;

namespace TVPApiServices
{
    /// <summary>
    /// Summary description for PlayerService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class PlayerService : WebService, IPlayerService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PlayerService()
        {

        }

        public class ErrorMessageWrapper
        {
            public string MediaId { get; set; }
            public string FileId { get; set; }
            public string Bitrate { get; set; }
            public string Message { get; set; }
        }

        public class MediaWrapper
        {
            public Media Media { get; set; }
            public TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] Rules { get; set; }
        }
        
        /// <summary>
        /// RetrieveMediaXML
        /// </summary>
        /// <param name="meidaID"></param>
        /// <param name="playerParams"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true, Description = "Get information about the media")]
        [System.Web.Script.Services.ScriptMethod()]
        [System.Xml.Serialization.XmlInclude(typeof(XmlDocument))]
        [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
        [System.Xml.Serialization.XmlInclude(typeof(Media.File))]
        public MediaWrapper GetMediaInfo(InitializationObject initObj, long MediaID, string picSize)
        {
            MediaWrapper retMedia = new MediaWrapper();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retMedia.Media = MediaHelper.GetMediaInfo(initObj, MediaID, picSize, groupID);

                    Media.File trailerFile = retMedia.Media.Files.Where(x => x.Format.ToLower() == ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).TechnichalConfiguration.Data.Player.TrailerFileFormat.ToLower()).SingleOrDefault();

                    if (!trailerFile.Equals(default(Media.File)))
                    {
                        retMedia.Media.Files.Remove(trailerFile);

                        trailerFile.Format = "Trailer";

                        retMedia.Media.Files.Insert(0, trailerFile);
                    }

                    Media.File trickPlayFile = retMedia.Media.Files.Where(x => x.Format.ToLower() == ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).TechnichalConfiguration.Data.Player.TrickPlayFileFormat.ToLower()).SingleOrDefault();

                    if (!trickPlayFile.Equals(default(Media.File)))
                    {
                        retMedia.Media.Files.Remove(trickPlayFile);

                        trickPlayFile.Format = "TrickPlay";

                        retMedia.Media.Files.Insert(0, trickPlayFile);
                    }

                    retMedia.Rules = new ApiApiService(groupID, initObj.Platform).GetGroupMediaRules((int)MediaID, int.Parse(initObj.SiteGuid), initObj.UDID);

                    // for debug
                    //retMedia.Rules = new TVPPro.SiteManager.TvinciPlatform.api.GroupRule[]{};
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retMedia;
        }

        [WebMethod(EnableSession = true, Description = "Mark player status")]
        [System.Xml.Serialization.XmlInclude(typeof(ActionHelper.FileHolder))]
        public string MediaMark(InitializationObject initObj, Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action Action, ActionHelper.FileHolder fileParam, 
                                int iLocation, long programId)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    sRet = ActionHelper.MediaMark(initObj, groupID, initObj.Platform, Action, iLocation, string.Empty, fileParam, programId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Mark player position")]
        public string MediaHit(InitializationObject initObj, long iMediaID, long iFileID, int iLocation, long programId)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    sRet = ActionHelper.MediaHit(initObj, groupID, initObj.Platform, iMediaID, iFileID, iLocation, string.Empty, programId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sRet;
        }

        /// <summary>
        /// Get media Last Position
        /// </summary>    
        [WebMethod(EnableSession = true, Description = "get player last position")]
        [System.Web.Script.Services.ScriptMethod()]
        [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
        public string MediaLastPosition(InitializationObject initObj, int MediaId)
        {
            string sLastPosition = "0";
            if (MediaId != 0)
            {
                TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject mediaMark = null;

                int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

                if (groupID > 0)
                {
                    try
                    {
                        //ConnectionHelper.InitServiceConfigs(groupID, initObj.Platform);

                        mediaMark = new ApiApiService(groupID, initObj.Platform).GetMediaMark(initObj.SiteGuid, MediaId);
                    }
                    catch (Exception ex)
                    {
                        HttpContext.Current.Items["Error"] = ex;
                    }
                }
                else
                {
                    HttpContext.Current.Items["Error"] = "Unknown group";
                }

                if (mediaMark != null)
                    sLastPosition = mediaMark.nLocationSec.ToString();
            }
            return sLastPosition;
        }

        [WebMethod(EnableSession = true, Description = "log player errors")]
        [System.Xml.Serialization.XmlInclude(typeof(TVPApi.ActionHelper.FileHolder))]
        public void MediaError(InitializationObject initObj, TVPApi.ActionHelper.FileHolder fileParam, string errorCode, string errorMessage, int location)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    ActionHelper.MediaError(initObj, groupID, initObj.Platform, fileParam, location, errorCode, errorMessage, string.Empty);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
        }

        [WebMethod(EnableSession = true, Description = "Get Media License")]
        public string GetMediaLicenseLink(InitializationObject initObj, int mediaFileID, string baseLink)
        {
            string sResponse = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetMediaLicenseLink", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    sResponse = new ApiConditionalAccessService(groupId, initObj.Platform).GetMediaLicenseLink(initObj.SiteGuid, mediaFileID, baseLink, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return sResponse;
        }

        private void flashVarsStringToDictionary(string str, ref IDictionary<string, string> flashVar)
        {
            string[] splittedParams = str.Split(new String[] { ",", ":", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string keyValue in splittedParams)
            {
                String[] splitKeyValue = keyValue.Split(new String[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitKeyValue.Length != 2) continue;
                String key = splitKeyValue[0];
                String value = splitKeyValue[1];
                flashVar.Add(key, value);
            }
        }

        /// <summary>
        /// Log
        /// </summary>    
        [WebMethod(EnableSession = true, Description = "Log Errors and Trace")]
        [System.Web.Script.Services.ScriptMethod()]
        [System.Xml.Serialization.XmlInclude(typeof(ErrorMessageWrapper))]
        [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
        public void Log(InitializationObject initObj, ErrorMessageWrapper message)
        {
            logger.DebugFormat(String.Format("Silverlight Player Log: {0}", message.Message));
        }

        [WebMethod(EnableSession = true, Description = "Check Parental PIN")]
        [System.Web.Script.Services.ScriptMethod()]
        [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
        public bool CheckParentalPIN(InitializationObject initObj, int ruleID, string parentalPIN)
        {
            bool retVal = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retVal = new ApiApiService(groupID, initObj.Platform).CheckParentalPIN(initObj.SiteGuid, ruleID, parentalPIN);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retVal;
        }
    }
}
