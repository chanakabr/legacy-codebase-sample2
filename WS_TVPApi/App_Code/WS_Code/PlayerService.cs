using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using log4net;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.DataLoaders;
using System.Xml;
using TVPPro.SiteManager.Services;
using TVPApi;
using TVPApiModule.Services;
using System.ServiceModel;
using TVPPro.Configuration.Technical;

namespace TVPApiServices
{
    /// <summary>
    /// Summary description for PlayerService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class PlayerService : System.Web.Services.WebService
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(PlayerService));

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

            logger.InfoFormat("GetMediaInfo-> [{0}, {1}], Params: [MediaID: {2}, picSize: {3}]", groupID, initObj.Platform, MediaID, picSize);

            if (groupID > 0)
            {
                try
                {

                    retMedia.Media = MediaHelper.GetMediaInfo(initObj, MediaID, picSize, groupID);

                    retMedia.Rules = new ApiApiService(groupID, initObj.Platform).GetGroupMediaRules((int)MediaID);

                    for (int i = 0; i < retMedia.Media.Files.Count; i++)
                    {
                        if (retMedia.Media.Files[i].Format == ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat)
                        {
                            Media.File file = retMedia.Media.Files[i];
                            file.Format = "1";
                            retMedia.Media.Files.RemoveAt(i);
                            retMedia.Media.Files.Insert(i, file);
                        }
                        if (retMedia.Media.Files[i].Format == ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).TechnichalConfiguration.Data.TVM.FlashVars.SubFileFormat)
                        {
                            Media.File file = retMedia.Media.Files[i];
                            file.Format = "0";
                            retMedia.Media.Files.RemoveAt(i);
                            retMedia.Media.Files.Insert(i, file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("GetMediaInfo->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetMediaInfo-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }           

            return retMedia;                                  
        }     

        [WebMethod(EnableSession = true, Description = "Mark player status")]
        [System.Xml.Serialization.XmlInclude(typeof(TVPApi.ActionHelper.FileHolder))]
        public string MediaMark(InitializationObject initObj, Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action Action, TVPApi.ActionHelper.FileHolder fileParam, int iLocation)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("MediaMark-> [{0}, {1}], Params:[ChannelID: {2}, picSize: {3}, pageSize: {4}, pageIndex: {5}]", groupID, initObj.Platform);

            if (groupID > 0)
            {
                try
                {
                    //ConnectionHelper.InitServiceConfigs(groupID, initObj.Platform);

                    sRet = ActionHelper.MediaMark(initObj, groupID, initObj.Platform, Action, fileParam, iLocation);
                }
                catch (Exception ex)
                {
                    logger.Error("MediaMark->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("MediaMark-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Mark player position")]
        public string MediaHit(InitializationObject initObj, long iMediaID, long iFileID, int iLocation)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "MediaMark", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("MediaHit-> [{0}, {1}], Params:[ChannelID: {2}, picSize: {3}, pageSize: {4}, pageIndex: {5}]", groupID, initObj.Platform);

            if (groupID > 0)
            {
                try
                {
                    //ConnectionHelper.InitServiceConfigs(groupID, initObj.Platform);
                    sRet = ActionHelper.MediaHit(initObj, groupID, initObj.Platform, iMediaID, iFileID, iLocation);
                }
                catch (Exception ex)
                {
                    logger.Error("MediaHit->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("MediaHit-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
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

                logger.InfoFormat("GetMediaMark-> [{0}, {1}], Params:[ChannelID: {2}, picSize: {3}, pageSize: {4}, pageIndex: {5}]", groupID, initObj.Platform);

                if (groupID > 0)
                {
                    try
                    {
                        //ConnectionHelper.InitServiceConfigs(groupID, initObj.Platform);

                        mediaMark = new ApiApiService(groupID, initObj.Platform).GetMediaMark(initObj.SiteGuid, MediaId);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("GetMediaMark->", ex);
                    }
                }
                else
                {
                    logger.ErrorFormat("GetMediaMark-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
                }

                if (mediaMark != null)
                    sLastPosition = mediaMark.nLocationSec.ToString();
            }
            return sLastPosition;
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
            logger.Debug(String.Format("Silverlight Player Log: {0}", message.Message));           
        }

        [WebMethod(EnableSession = true, Description = "Get Media License")]
        public string GetMediaLicenseLink(InitializationObject initObj, int mediaFileID, string baseLink)
        {
            string sResponse = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetMediaLicenseLink", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetMediaLicenseLink-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    sResponse = new ApiConditionalAccessService(groupId, initObj.Platform).GetMediaLicenseLink(initObj.SiteGuid, mediaFileID, baseLink, initObj.UDID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetMediaLicenseLink->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetMediaLicenseLink-> 'Unknown group' Username: {0}, Password: {1}, mediaFileID: {2}", initObj.ApiUser, initObj.ApiPass, mediaFileID);
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

        [WebMethod(EnableSession = true, Description = "Check Parental PIN")]
        [System.Web.Script.Services.ScriptMethod()]
        [System.Xml.Serialization.XmlInclude(typeof(InitializationObject))]
        public bool CheckParentalPIN(InitializationObject initObj, string parentalPIN)
        {
            bool retVal = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("CheckParentalPIN-> [{0}, {1}], Params: [ParentalPIN: {2}]", groupID, initObj.Platform, parentalPIN);

            if (groupID > 0)
            {
                try
                {
                    retVal = new ApiConditionalAccessService(groupID, initObj.Platform).CheckParentalPIN(initObj.SiteGuid, parentalPIN);
                }
                catch (Exception ex)
                {
                    logger.Error("CheckParentalPIN->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("CheckParentalPIN-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retVal;
        }     
    }
}
