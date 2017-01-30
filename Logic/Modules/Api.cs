using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;
using APILogic;
using ApiObjects;
using ApiObjects.BulkExport;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.Rules;
using KLogMonitor;
using ApiObjects.TimeShiftedTv;
using ScheduledTasks;
using Core.Catalog.Response;


namespace Core.Api
{
    public class Module
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static UserIMRequestObject TVAPI_GetTvinciGUID(int groupId, InitializationObject oInitObj)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                    return Core.Api.api.GetTvinciGUID(oInitObj, groupId);
                else
                {
                    log.Debug("WS ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static ChannelObject TVAPI_GetMedias(int groupId, InitializationObject oInitObj, Int32[] nMediaIDs, MediaInfoStructObject theInfoStruct)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                    return Core.Api.api.SingleMedia(oInitObj, groupId, nMediaIDs, theInfoStruct);
                else
                {
                    log.Debug("WS: TVAPI_GetMedias ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static MediaInfoStructObject TVAPI_GetMediaStructure(int groupId, InitializationObject oInitObj)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                    return Core.Api.api.GetMediaStructure(oInitObj, groupId);
                else
                {
                    log.Debug("WS: TVAPI_GetMediaStructure ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static int InsertEPGSchedule(int groupId, int channelID, string fileName, bool isDelete)
        {
            string xml = GetXmlString(fileName);
            return Core.Api.api.InserEPGScheduleToChannel(groupId, channelID, xml, isDelete);
        }

        public static List<EPGChannelObject> GetEPGChannel(int groupId, string sPicSize)
        {
            return Core.Api.api.GetEPGChannel(groupId, sPicSize);
        }

        public static ChannelObject TVAPI_GetMediaInfo(int groupId, InitializationObject oInitObj, Int32[] nMediaIDs, MediaInfoStructObject theInfoStruct)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    oInitObj.m_oFileRequestObjects = null;

                    return Core.Api.api.SingleMedia(oInitObj, groupId, nMediaIDs, theInfoStruct);
                }
                else
                {
                    log.Debug("WS: TVAPI_GetMediaInfo ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static ChannelObject TVAPI_SearchMedia(int groupId, InitializationObject oInitObj, SearchDefinitionObject oSearchDefinitionObj, MediaInfoStructObject theInfoStruct)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.SearchMedia(oInitObj, oSearchDefinitionObj, theInfoStruct, groupId);
                }
                else
                {
                    log.Debug("WS: TVAPI_SearchMedia ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static ChannelObject TVAPI_SearchRelated(int groupId, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nMediaID)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.SearchRelated(oInitObj, theInfoStruct, thePageDef, nMediaID, groupId);
                }
                else
                {
                    log.Debug("WS: TVAPI_SearchRelated ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static ChannelObject TVAPI_NowPlaying(int groupId, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.NowPlaying(oInitObj, theInfoStruct, thePageDef, groupId);
                }
                else
                {
                    log.Debug("WS: TVAPI_NowPlaying ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static AdminAccountUserResponse AdminSignIn(int groupId, string username, string pass)
        {
            try
            {
                return Core.Api.api.GetAdminUserAccount(username, pass);
            }
            catch (Exception ex)
            {
                log.Error("WS Exception: " + ex.Message + " || " + ex.StackTrace, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }
            return null;
        }

        public static ChannelObject TVAPI_UserLastWatched(int groupId, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.UserLastWatched(oInitObj, theInfoStruct, thePageDef, groupId);
                }
                else
                {
                    log.Debug("WS: TVAPI_UserLastWatched ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static ChannelObject TVAPI_PeopleWhoWatchedAlsoWatched(int groupId, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nMediaID, Int32 nMediaFileID)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.PeopleWhoWatchedAlsoWatched(oInitObj, theInfoStruct, groupId, nMediaID, nMediaFileID);
                }
                else
                {
                    log.Debug("WS: TVAPI_PeopleWhoWatchedAlsoWatched ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static ChannelObject[] TVAPI_ChannelsMedia(int groupId, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, ChannelRequestObject[] theChannelsRequestObj)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.ChannelsMedia(oInitObj, theInfoStruct, theChannelsRequestObj, groupId);
                }
                else
                {
                    log.Debug("WS: TVAPI_ChannelsMedia ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static CategoryObject[] TVAPI_CategoriesTree(int groupId, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nCategoryID, bool bWithChannels)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.CategoriesTree(oInitObj, theInfoStruct, groupId, nCategoryID, bWithChannels);
                }
                else
                {
                    log.Debug("WS: TVAPI_CategoriesTree ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static ChannelObject[] TVAPI_CategoryChannels(int groupId, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nCategoryID)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.CategoryChannels(oInitObj, theInfoStruct, groupId, nCategoryID);
                }
                else
                {
                    log.Debug("WS: TVAPI_CategoryChannels ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static ChannelObject[] TVAPI_UserSavedChannels(int groupId, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.UserChannels(oInitObj, theInfoStruct, groupId, "");
                }
                else
                {
                    log.Debug("WS: TVAPI_UserSavedChannels ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static ChannelObject[] TVAPI_UserDeleteChannel(int groupId, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nChannelID)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.DeleteUserChannel(oInitObj, theInfoStruct, groupId, nChannelID);
                }
                else
                {
                    log.Debug("WS: TVAPI_UserDeleteChannel ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static GenericWriteResponse TVAPI_UserSavePlaylist(int groupId, InitializationObject oInitObj, Int32[] nMediaIDs, string sPlaylistTitle, bool bRewrite)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.saveUserPlaylist(oInitObj, groupId, nMediaIDs, sPlaylistTitle, bRewrite);
                }
                else
                {
                    log.Debug("WS: TVAPI_UserSavePlaylist ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static GenericWriteResponse TVAPI_SendMediaByEmail(int groupId, InitializationObject oInitObj, Int32 nMediaID,
            string sFromEmail, string sToEmail, string sRecieverName, string sSenderName, string sContent)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.SendMediaByEmail(oInitObj, groupId, nMediaID, sFromEmail, sToEmail, sRecieverName, sSenderName, sContent);
                }
                else
                {
                    log.Debug("WS: TVAPI_SendMediaByEmail ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static ApiObjects.TagResponseObject[] TVAPI_TagValues(int groupId, InitializationObject oInitObj, TagRequestObject[] oTagsDefinition)
        {
            try
            {
                Core.Api.api.InitializeGroupNPlayer(ref oInitObj);
                if (oInitObj.m_nGroupID == groupId)
                {
                    return Core.Api.api.TagValues(oInitObj, oTagsDefinition, groupId);
                }
                else
                {
                    log.Debug("WS: TVAPI_TagValues ignored due to group mismatch - Players: " + oInitObj.m_nGroupID.ToString() + ",UN+Pass: " + groupId.ToString());
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

        public static string CheckGeoBlockMediaOld(int groupId, Int32 nMediaID, string sIP)
        {
            string ruleName;
            return Core.Api.api.CheckGeoBlockMediaOld(groupId, nMediaID, sIP, out ruleName);
        }

        /// <summary>
        /// Check subscription Geo Commerce Block Roule
        /// </summary>
        /// <param name="sWSUserName">Set web service user name</param>
        /// <param name="sWSPassword">Set web service password</param>
        /// <param name="SubscriptionGeoCommerceID">set subscription geo commerce roule ID</param>
        /// <param name="sIP">set client IP</param>
        /// <returns>return true if the enable purchase subscription in spasfic country by IP</returns>
        public static bool CheckGeoCommerceBlock(int groupId, int SubscriptionGeoCommerceID, string sIP)
        {
            return Core.Api.api.IsGeoCommerceBlock(groupId, SubscriptionGeoCommerceID, sIP);
        }

        public static Int32 GetMediaFileTypeID(int groupId, Int32 nMediaFileID)
        {
            return Core.Api.api.GetMediaFileTypeID(nMediaFileID, groupId);
        }

        public static string GetMediaFileTypeDescription(int groupId, Int32 nMediaFileID)
        {
            return Core.Api.api.GetMediaFileTypeDescription(nMediaFileID, groupId);
        }

        public static bool GetAdminTokenValues(int groupId, string sIP, string sToken, ref string sCountryCd2, ref string sLanguageFullName, ref string sDeviceName, ref UserStatus eUserStatus)
        {
            return Core.Api.api.GetAdminTokenValues(sIP, sToken, groupId, ref sCountryCd2, ref sLanguageFullName, ref sDeviceName, ref eUserStatus);
        }

        public static Int32[] GetChannelsMediaIDs(int groupId, Int32[] nChannels, Int32[] nFileTypeIDs,
            bool bWithCache, string sDevice)
        {
            return Core.Api.api.GetChannelsMediaIDs(nChannels, nFileTypeIDs, bWithCache, groupId, sDevice);
        }


        public static List<int> GetChannelsAssetsIDs(int groupId, Int32[] nChannels, Int32[] nFileTypeIDs, bool bWithCache, string sDevice, bool activeAssets, bool useStartDate)
        {
            return Core.Api.api.GetChannelsMediaIDs(nChannels, nFileTypeIDs, bWithCache, groupId, sDevice, activeAssets, useStartDate);
        }




        public static UnifiedSearchResult[] GetChannelAssets(int groupId, int channelId, int pageIndex, int pageSize)
        {
            return Api.Module.GetChannelAssets(channelId, groupId, pageIndex, pageSize);
        }

        public static UnifiedSearchResult[] SearchAssets(int groupId, string filter, int pageIndex, int pageSize, bool OnlyIsActive, int languageID, bool UseStartDate,
               string Udid, string UserIP, string SiteGuid, int DomainId, int ExectgroupId, bool IgnoreDeviceRule)
        {
            return Api.Module.SearchAssets(groupId, filter, pageIndex, pageSize, OnlyIsActive, languageID, UseStartDate,
               Udid, UserIP, SiteGuid, DomainId, ExectgroupId, IgnoreDeviceRule);
        }


        public static FileTypeContainer[] GetAvailableFileTypes(int groupId)
        {
            return Core.Api.api.GetAvailableFileTypes(groupId);
        }

        public static Int32[] GetChannelMediaIDs(int groupId, Int32 nChannelID, Int32[] nFileTypeIDs,
            bool bWithCache, string sDevice)
        {
            return Core.Api.api.GetChannelsMediaIDs(new int[] { nChannelID }, nFileTypeIDs, bWithCache, groupId, sDevice);
        }

        public static bool DoesMediaBelongToChannels(int groupId, Int32[] nChannels, Int32[] nFileTypeIDs,
            Int32 nMediaID, bool bWithCache, string sDevice)
        {

            return Core.Api.api.DoesMediaBelongToChannels(nChannels, nFileTypeIDs, nMediaID, bWithCache, groupId, sDevice);
        }

        public static bool ValidateBaseLink(int groupId, Int32 nMediaFileID, string sBaseLink)
        {
            return Core.Api.api.ValidateBaseLink(nMediaFileID, sBaseLink, groupId);
        }

        public static MeidaMaper[] MapMediaFiles(int groupId, Int32[] nMediaFileIDs)
        {
            return Core.Api.api.MapMediaFiles(nMediaFileIDs, groupId);
        }

        public static MeidaMaper[] MapMediaFilesST(int groupId, string sSeperatedMediaFileIDs)
        {
            return Core.Api.api.MapMediaFilesST(sSeperatedMediaFileIDs, groupId);
        }

        public static GroupInfo[] GetSubGroupsTree(int groupId, string sGroupName)
        {
            return Core.Api.api.GetSubGroupsTree(sGroupName, groupId);
        }

        public static string[] GetGroupPlayers(int groupId, string sGroupName, bool sIncludeChildGroups)
        {
            return Core.Api.api.GetGroupPlayers(sGroupName, sIncludeChildGroups, groupId);
        }

        public static string[] GetGroupMediaNames(int groupId, string sGroupName)
        {
            return Core.Api.api.GetGroupMediaNames(sGroupName, groupId);
        }

        public static MediaMarkObject GetMediaMark(int groupId, Int32 nMediaID, string sSiteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            return Core.Api.api.GetMediaMark(groupId, nMediaID, sSiteGuid);
        }

        public static RateMediaObject RateMedia(int groupId, Int32 nMediaID, string sSiteGuid, Int32 nRateVal)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            return Core.Api.api.RateMedia(groupId, nMediaID, sSiteGuid, nRateVal);
        }

        public static bool AddUserSocialAction(int groupId, Int32 nMediaID, string sSiteGuid, ApiObjects.SocialAction socialAction, ApiObjects.SocialPlatform socialPlatform)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            int nSocialAction = (int)socialAction;
            int nSocialPlatform = (int)socialPlatform;
            return Core.Api.api.AddUserSocialAction(groupId, nMediaID, sSiteGuid, nSocialAction, nSocialPlatform);
        }

        public static bool RunImporter(int groupId, string extraParams)
        {
            return Core.Api.api.RunImporter(groupId, extraParams);
        }

        public static bool SendMailTemplate(int groupId, MailRequestObj request)
        {
            return Core.Api.api.SendMailTemplate(request);
        }

        public static List<GroupRule> GetGroupRules(int groupId)
        {
            return Core.Api.api.GetGroupRules(groupId);
        }

        public static List<string> GetAutoCompleteList(int groupId, RequestObj request)
        {
            return Core.Api.api.GetAutoCompleteList(request, groupId);
        }

        public static List<GroupRule> GetUserGroupRules(int groupId, string sSiteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            return Core.Api.api.GetUserDomainGroupRules(groupId, sSiteGuid, 0);
        }

        public static List<GroupRule> GetDomainGroupRules(int groupId, int nDomainID)
        {
            return Core.Api.api.GetUserDomainGroupRules(groupId, null, nDomainID);
        }

        public static bool SendToFriend(int groupId, string sSenderName, string sSenderMail, string sMailTo, string sNameTo, int nMediaID)
        {
            return Core.Api.api.SendToFriend(groupId, sSenderName, sSenderMail, sMailTo, sNameTo, nMediaID);
        }

        public static GroupOperator[] GetGroupOperators(int groupId, string sScope = "")
        {
            return Core.Api.api.GetGroupOperators(groupId, sScope);
        }

        public static List<GroupOperator> GetOperator(int groupId, List<int> operatorIds)
        {
            return Core.Api.api.GetOperators(groupId, operatorIds);
        }

        public static bool SetUserGroupRule(int groupId, string sSiteGuid, int nRuleID, string sPIN, int nIsActive)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            if (nIsActive > 1 || nIsActive < 0)
            {
                return false;
            }

            return Core.Api.api.SetUserGroupRule(sSiteGuid, nRuleID, nIsActive, sPIN, groupId);
        }

        public static bool SetDomainGroupRule(int groupId, int nDomainID, int nRuleID, string sPIN, int nIsActive)
        {
            if (nIsActive > 1 || nIsActive < 0)
            {
                return false;
            }

            return Core.Api.api.SetDomainGroupRule(nDomainID, nRuleID, nIsActive, sPIN, groupId);
        }

        public static bool SetRuleState(int groupId, int nDomainID, string sSiteGUID, int nRuleID, int nStatus)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            return Core.Api.api.SetRuleState(sSiteGUID, nDomainID, nRuleID, nStatus, groupId);
        }


        public static bool CheckParentalPIN(int groupId, string sSiteGUID, int nRuleID, string sParentalPIN)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            return Core.Api.api.CheckParentalPIN(sSiteGUID, nRuleID, sParentalPIN, groupId);
        }

        public static bool CheckDomainParentalPIN(int groupId, int nDomainID, int nRuleID, string sParentalPIN)
        {
            return Core.Api.api.CheckDomainParentalPIN(nDomainID, nRuleID, sParentalPIN, groupId);
        }

        public static DeviceAvailabiltyRule GetAvailableDevices(int groupId, int nMediaID)
        {
            return Core.Api.api.GetAvailableDevices(nMediaID, groupId);
        }

        public static ApiObjects.Response.Status CleanUserHistory(int groupId, string siteGuid, List<int> lMediaIDs)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.CleanUserHistory(groupId, siteGuid, lMediaIDs);
        }

        public static Scheduling GetProgramSchedule(int groupId, int nProgramId)
        {
            return Core.Api.api.GetProgramSchedule(nProgramId, groupId);
        }

        public static string GetCoGuidByMediaFileId(int groupId, int nMediaFileID)
        {
            return Core.Api.api.GetCoGuidByMediaFileId(nMediaFileID);
        }

        public static string[] GetUserStartedWatchingMedias(int groupId, string sSiteGuid, int nNumOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            return Core.Api.api.GetUserStartedWatchingMedias(sSiteGuid, nNumOfItems, groupId).ToArray();
        }

        public static bool DoesMediaBelongToSubscription(int groupId, int nSubscriptionCode, int[] nFileTypeIDs,
            int nMediaID, string sDevice)
        {

            return Core.Api.api.DoesMediaBelongToBundle(nSubscriptionCode, nFileTypeIDs, nMediaID, sDevice, groupId, Btype.SUBSCRIPTION);
        }

        public static bool DoesMediaBelongToCollection(int groupId, int nCollectionCode, int[] nFileTypeIDs,
            int nMediaID, string sDevice)
        {

            return Core.Api.api.DoesMediaBelongToBundle(nCollectionCode, nFileTypeIDs, nMediaID, sDevice, groupId, Btype.COLLECTION);
        }

        public static List<GroupRule> GetGroupMediaRules(int groupId, int nMediaID, int siteGuid, string sIP, string deviceUdid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            return Core.Api.api.GetGroupMediaRules(nMediaID, sIP, siteGuid.ToString(), groupId, deviceUdid);
        }

        public static List<GroupRule> GetEPGProgramRules(int groupId, int nMediaId, int nProgramId, int siteGuid, string sIP, string deviceUdid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            return Core.Api.api.GetEPGProgramRules(nProgramId, nMediaId, siteGuid.ToString(), sIP, groupId, deviceUdid);
        }

        public static List<GroupRule> GetNpvrRules(int groupId, RecordedEPGChannelProgrammeObject recordedProgram, int siteGuid, string sIP, string deviceUdid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            return Core.Api.api.GetNpvrRules(recordedProgram, siteGuid, sIP, groupId, deviceUdid);
        }

        public static List<int> ChannelsContainingMedia(int groupId, List<int> lChannels, int nMediaID, int nMediaFileID)
        {
            return Core.Api.api.ChannelsContainingMedia(lChannels, nMediaID, groupId, nMediaFileID);
        }

        public static List<int> GetSubscriptionMediaIds(int groupId, int nSubscriptionCode, int[] nFileTypeIDs, string sDevice)
        {
            return Core.Api.api.GetBundleMediaIds(nSubscriptionCode, nFileTypeIDs, sDevice, groupId, Btype.SUBSCRIPTION);
        }

        public static List<int> GetCollectionMediaIds(int groupId, int nCollectionCode, int[] nFileTypeIDs, string sDevice)
        {
            return Core.Api.api.GetBundleMediaIds(nCollectionCode, nFileTypeIDs, sDevice, groupId, Btype.COLLECTION);
        }

        public static List<int> GetMediaChannels(int groupId, int nMediaId)
        {
            return Core.Api.api.GetMediaChannels(groupId, nMediaId);
        }

        public static EPGChannelProgrammeObject GetProgramDetails(int groupId, int nProgramId)
        {
            return Core.Api.api.GetProgramDetails(nProgramId, groupId);
        }

        public static List<MediaConcurrencyRule> GetMediaConcurrencyRules(int groupId, int nMediaID, string sIP, int bmID, eBusinessModule eType)
        {
            return Core.Api.api.GetMediaConcurrencyRules(nMediaID, sIP, groupId, bmID, eType);
        }

        public static RegionsResponse GetRegions(int groupId, List<string> externalRegionList, RegionOrderBy orderBy)
        {
            return Core.Api.api.GetRegions(groupId, externalRegionList, orderBy);
        }

        public static List<LanguageObj> GetGroupLanguages(int groupId)
        {
            return Core.Api.api.GetGroupLanguages(groupId);
        }

        #region Parental Rules

        /// <summary>
        /// All of the parental rules for the account.
        /// Includes specification of what of which is the default rule/s for the account
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ParentalRulesResponse GetParentalRules(int groupId)
        {
            return Core.Api.api.GetParentalRules(groupId);
        }

        /// <summary>
        /// Gets the parental rules that applies for the domain
        /// Includes distinction if rule was defined at account, HH or user level
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static ParentalRulesResponse GetDomainParentalRules(int groupId, int domainId)
        {
            return Core.Api.api.GetDomainParentalRules(groupId, domainId);
        }

        /// <summary>
        /// Gets the parental rules that applies for the User.
        /// Includes distinction if rule was defined at account, HH or user level
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        public static ParentalRulesResponse GetUserParentalRules(int groupId, string siteGuid, int domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.GetUserParentalRules(groupId, siteGuid, domainId);
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
        public static Status SetUserParentalRules(int groupId, string siteGuid, long ruleId, int isActive, int domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.SetUserParentalRules(groupId, siteGuid, ruleId, isActive, domainId);
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
        public static Status SetDomainParentalRules(int groupId, int domainId, long ruleId, int isActive)
        {
            return Core.Api.api.SetDomainParentalRules(groupId, domainId, ruleId, isActive);
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
        public static PinResponse GetParentalPIN(int groupId, int domainId, string siteGuid, int? ruleId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.GetParentalPIN(groupId, domainId, siteGuid, ruleId);
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
        public static PinResponse UpdateParentalPIN(int groupId, int domainId, string siteGuid, string pin, int? ruleId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PinResponse response = new PinResponse();
            response.status = Core.Api.api.SetParentalPIN(groupId, domainId, siteGuid, pin, ruleId);
            if (response.status.Code == (int)eResponseStatus.OK)
                response = Core.Api.api.GetParentalPIN(groupId, domainId, siteGuid, ruleId);

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
        public static Status SetParentalPIN(int groupId, int domainId, string siteGuid, string pin, int? ruleId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            
            if (ruleId == 0)
            {
                return Core.Api.api.SetParentalPIN(groupId, domainId, siteGuid, pin);
            }
            else
            {
                return Core.Api.api.SetParentalPIN(groupId, domainId, siteGuid, pin, ruleId);
            }
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
        public static PurchaseSettingsResponse GetPurchaseSettings(int groupId, int domainId, string siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.GetPurchaseSettings(groupId, domainId, siteGuid);
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
        [Obsolete]
        public static Status SetPurchaseSettings(int groupId, int domainId, string siteGuid, int setting)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.SetPurchaseSettings(groupId, domainId, siteGuid, setting);
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
        public static PurchaseSettingsResponse UpdatePurchaseSettings(int groupId, int domainId, string siteGuid, int setting)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PurchaseSettingsResponse response = new PurchaseSettingsResponse();

            
            response.status = Core.Api.api.SetPurchaseSettings(groupId, domainId, siteGuid, setting);
            if (response.status.Code == (int)eResponseStatus.OK)
                response = Core.Api.api.GetPurchaseSettings(groupId, domainId, siteGuid);

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
        public static PurchaseSettingsResponse GetPurchasePIN(int groupId, int domainId, string siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.GetPurchasePIN(groupId, domainId, siteGuid);
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
        public static PurchaseSettingsResponse UpdatePurchasePIN(int groupId, int domainId, string siteGuid, string pin)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            PurchaseSettingsResponse response = new PurchaseSettingsResponse();

            response.status = Core.Api.api.SetPurchasePIN(groupId, domainId, siteGuid, pin);
            if (response.status.Code == (int)eResponseStatus.OK)
                response = Core.Api.api.GetPurchasePIN(groupId, domainId, siteGuid);

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
        [Obsolete]
        public static Status SetPurchasePIN(int groupId, int domainId, string siteGuid, string pin)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.SetPurchasePIN(groupId, domainId, siteGuid, pin);
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
        public static Status ValidateParentalPIN(int groupId, string siteGuid, string pin, int domainId, int? ruleId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.ValidateParentalPIN(groupId, siteGuid, pin, domainId, ruleId);
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
        public static Status ValidatePurchasePIN(int groupId, string siteGuid, string pin, int domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.ValidatePurchasePIN(groupId, siteGuid, pin, domainId);
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
        public static ParentalRulesResponse GetParentalMediaRules(int groupId, string siteGuid, long mediaId, long domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.GetParentalMediaRules(groupId, siteGuid, mediaId, domainId);
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
        public static ParentalRulesResponse GetParentalEPGRules(int groupId, string siteGuid, long epgId, long domainId)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            return Core.Api.api.GetParentalEPGRules(groupId, siteGuid, epgId, domainId);
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
        public static Status DisableUserDefaultParentalRule(int groupId, string siteGuid, int domainId)
        {
            return Core.Api.api.SetUserParentalRules(groupId, siteGuid, -1, 1, domainId);
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
        public static Status DisableDomainDefaultParentalRule(int groupId, int domainId)
        {
            return Core.Api.api.SetDomainParentalRules(groupId, domainId, -1, 1);
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
        public static GenericRuleResponse GetMediaRules(int groupId, string siteGuid, long mediaId, long domainId, string ip, string udid, GenericRuleOrderBy orderBy)
        {
            return Core.Api.api.GetMediaRules(groupId, siteGuid, mediaId, domainId, ip, udid, orderBy);
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
        public static GenericRuleResponse GetEpgRules(int groupId, string siteGuid, long epgId, long channelMediaId, long domainId, string ip, GenericRuleOrderBy orderBy)
        {
            return Core.Api.api.GetEpgRules(groupId, siteGuid, epgId, channelMediaId, domainId, ip, orderBy);
        }

        public static ParentalRulesTagsResponse GetUserParentalRuleTags(int groupId, string siteGuid, long domainId)
        {

            if (HttpContext.Current != null && HttpContext.Current.Items != null)
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";
            }

            return Core.Api.api.GetUserParentalRuleTags(groupId, siteGuid, domainId);
        }

        #endregion

        public static bool BuildIPToCountryIndex(int groupId)
        {
            return Core.Api.api.BuildIPToCountryIndex(groupId);
        }

        public static StatusErrorCodesResponse GetErrorCodesDictionary()
        {
            StatusErrorCodesResponse response = new StatusErrorCodesResponse();

            response.ErrorsDictionary = Core.Api.api.GetErrorCodesDictionary();
            if (response.ErrorsDictionary != null)
                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            return response;
        }

        #region OSS Adpater
        public static OSSAdapterResponse InsertOSSAdapter(int groupId, OSSAdapter ossAdapter)
        {
            return Core.Api.api.InsertOSSAdapter(groupId, ossAdapter);
        }

        public static ApiObjects.Response.Status DeleteOSSAdapter(int groupId, int ossAdapterID)
        {
            return Core.Api.api.DeleteOSSAdapter(groupId, ossAdapterID);
        }

        public static OSSAdapterResponse SetOSSAdapter(int groupId, OSSAdapter ossAdapter)
        {
            return Core.Api.api.SetOSSAdapter(groupId, ossAdapter);
        }

        public static OSSAdapterResponseList GetOSSAdapter(int groupId)
        {
            return Core.Api.api.GetOSSAdapters(groupId);
        }

        public static OSSAdapterResponse GetOSSAdapterProfile(int groupId, int ossAdapterId)
        {
            return Core.Api.api.GetOSSAdapter(groupId, ossAdapterId);
        }

        public static ApiObjects.Response.Status InsertOSSAdapterSettings(int groupId, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            return Core.Api.api.InsertOSSAdapterSettings(groupId, ossAdapterId, settings);
        }

        public static ApiObjects.Response.Status SetOSSAdapterSettings(int groupId, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            return Core.Api.api.SetOSSAdapterSettings(groupId, ossAdapterId, settings);
        }

        public static ApiObjects.Response.Status DeleteOSSAdapterSettings(int groupId, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            return Core.Api.api.DeleteOSSAdapterSettings(groupId, ossAdapterId, settings);
        }

        public static OSSAdapterSettingsResponse GetOSSAdapterSettings(int groupId)
        {
            return Core.Api.api.GetOSSAdapterSettings(groupId);
        }

        public static OSSAdapterBillingDetailsResponse GetUserBillingDetails(int groupId, long householdId, int ossAdapterId, string userIP)
        {
            return Core.Api.api.GetUserBillingDetails(groupId, householdId, ossAdapterId, userIP);
        }

        public static ApiObjects.Response.Status SetOSSAdapterConfiguration(int groupId, int ossAdapterId)
        {
            return Core.Api.api.SetOSSAdapterConfiguration(groupId, ossAdapterId);
        }

        public static OSSAdapterResponse GenerateOSSSharedSecret(int groupId, int ossAdapterId)
        {
            return Core.Api.api.GenerateOSSSharedSecret(groupId, ossAdapterId);
        }


        #endregion

        public static bool UpdateCache(int groupId, string bucket, string[] keys)
        {
            bool result = false;

            result = TVinciShared.QueueUtils.UpdateCache(groupId, bucket, keys);

            return result;
        }

        public static bool UpdateGeoBlockRulesCache(int groupId)
        {
            bool result = false;

            result = Core.Api.api.UpdateGeoBlockRulesCache(groupId);

            return result;
        }

        #region Recommendation Engine
        public static RecommendationEngineResponse InsertRecommendationEngine(int groupId, RecommendationEngine recommendationEngine)
        {
            return Core.Api.api.InsertRecommendationEngine(groupId, recommendationEngine);
        }

        public static ApiObjects.Response.Status DeleteRecommendationEngine(int groupId, int recommendationEngineId)
        {
            return Core.Api.api.DeleteRecommendationEngine(groupId, recommendationEngineId);
        }

        public static RecommendationEngineResponse SetRecommendationEngine(int groupId, RecommendationEngine recommendationEngine)
        {
            return Core.Api.api.SetRecommendationEngine(groupId, recommendationEngine);
        }

        public static RecommendationEnginesResponseList GetRecommendationEngines(int groupId)
        {
            return Core.Api.api.GetRecommendationEngines(groupId);
        }

        public static RecommendationEnginesResponseList ListRecommendationEngines(int groupId)
        {
            return Core.Api.api.ListRecommendationEngines(groupId);
        }

        public static ApiObjects.Response.Status InsertRecommendationEngineSettings(int groupId, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            return Core.Api.api.InsertRecommendationEngineSettings(groupId, recommendationEngineId, settings);
        }

        public static ApiObjects.Response.Status SetRecommendationEngineSettings(int groupId, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            return Core.Api.api.SetRecommendationEngineSettings(groupId, recommendationEngineId, settings);
        }

        public static ApiObjects.Response.Status DeleteRecommendationEngineSettings(int groupId, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            return Core.Api.api.DeleteRecommendationEngineSettings(groupId, recommendationEngineId, settings);
        }

        public static RecommendationEngineSettinsResponse GetRecommendationEngineSettings(int groupId)
        {
            return Core.Api.api.GetRecommendationEngineSettings(groupId);
        }

        public static RecommendationEngineResponse UpdateRecommendationEngineConfiguration(int groupId, int recommendationEngineId)
        {
            return Core.Api.api.UpdateRecommendationEngineConfiguration(groupId, recommendationEngineId);
        }

        public static RecommendationEngineResponse GenerateRecommendationEngineSharedSecret(int groupId, int recommendationEngineId)
        {
            return Core.Api.api.GenerateRecommendationEngineSharedSecret(groupId, recommendationEngineId);
        }

        #endregion

        #region External Channel
        public static ExternalChannelResponse
            InsertExternalChannel(int groupId, ExternalChannel externalChannel)
        {
            return Core.Api.api.InsertExternalChannel(groupId, externalChannel);
        }

        public static ApiObjects.Response.Status DeleteExternalChannel(int groupId, int externalChannelId)
        {
            return Core.Api.api.DeleteExternalChannel(groupId, externalChannelId);
        }

        public static ExternalChannelResponse SetExternalChannel(int groupId, ExternalChannel externalChannel)
        {
            return Core.Api.api.SetExternalChannel(groupId, externalChannel);
        }

        public static ExternalChannelResponseList GetExternalChannels(int groupId)
        {
            return Core.Api.api.GetExternalChannels(groupId);
        }

        public static ExternalChannelResponseList ListExternalChannels(int groupId)
        {
            return Core.Api.api.ListExternalChannels(groupId);
        }
        #endregion

        public static BulkExportTaskResponse AddBulkExportTask(int groupId, string externalKey, string name, eBulkExportDataType dataType, string filter,
            eBulkExportExportType exportType, long frequency, string notificationUrl, List<int> vodTypes, bool isActive)
        {
            return Core.Api.api.AddBulkExportTask(groupId, externalKey, name, dataType, filter, exportType, frequency, notificationUrl, vodTypes, isActive);
        }

        public static BulkExportTaskResponse UpdateBulkExportTask(int groupId, long id, string externalKey, string name, eBulkExportDataType dataType,
            string filter, eBulkExportExportType exportType, long frequency, string notificationUrl, List<int> vodTypes, bool? isActive)
        {
            return Core.Api.api.UpdateBulkExportTask(groupId, id, externalKey, name, dataType, filter, exportType, frequency, notificationUrl, vodTypes, isActive);
        }

        public static Status DeleteBulkExportTask(int groupId, long id, string externalKey)
        {
            if (Core.Api.api.DeleteBulkExportTask(groupId, id, externalKey))
            {
                return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()); ;
        }

        public static BulkExportTasksResponse GetBulkExportTasks(int groupId, List<long> ids, List<string> externalKeys, BulkExportTaskOrderBy orderBy)
        {
            BulkExportTasksResponse response = new BulkExportTasksResponse() { Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            response.Tasks = Core.Api.api.GetBulkExportTasks(ids, externalKeys, groupId, orderBy);
            if (response.Tasks != null)
            {
                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        public static bool Export(int groupId, long taskId, string version)
        {
            return Core.Api.api.Export(groupId, taskId, version);
        }

        public static Status EnqueueExportTask(int groupId, long taskId)
        {
            return Core.Api.api.EnqueueExportTask(groupId, taskId);
        }

        public static Status MessageRecovery(int groupId, long baseDateSec, List<string> messageDataTypes)
        {
            return Core.Api.api.MessageRecovery(groupId, baseDateSec, messageDataTypes);
        }

        public static RolesResponse GetRoles(int groupId, List<long> roleIds)
        {
            return Core.Api.api.GetRoles(groupId, roleIds);
        }

        public static PermissionsResponse GetPermissions(int groupId, List<long> permissionIds)
        {
            return Core.Api.api.GetPermissions(groupId, permissionIds);
        }

        public static PermissionResponse AddPermission(int groupId, string name, List<long> permissionItemsIds, ePermissionType type, string usersGroup, long updaterId)
        {
            return Core.Api.api.AddPermission(groupId, name, permissionItemsIds, type, usersGroup, updaterId);
        }

        public static Status AddPermissionToRole(int groupId, long roleId, long permissionId)
        {
            return Core.Api.api.AddPermissionToRole(groupId, roleId, permissionId);
        }

        public static Status AddPermissionItemToPermission(int groupId, long permissionId, long permissionItemId)
        {
            return Core.Api.api.AddPermissionItemToPermission(groupId, permissionId, permissionItemId);
        }

        #region KSQL Channel
        public static KSQLChannelResponse
            InsertKSQLChannel(int groupId, KSQLChannel channel)
        {
            return APILogic.CRUD.KSQLChannelsManager.Insert(groupId, channel);
        }

        public static ApiObjects.Response.Status DeleteKSQLChannel(int groupId, int channelId)
        {
            return APILogic.CRUD.KSQLChannelsManager.Delete(groupId, channelId);
        }

        public static KSQLChannelResponse SetKSQLChannel(int groupId, KSQLChannel channel)
        {
            return APILogic.CRUD.KSQLChannelsManager.Set(groupId, channel);
        }

        public static KSQLChannelResponse GetKSQLChannel(int groupId, int channelId)
        {
            return APILogic.CRUD.KSQLChannelsManager.Get(groupId, channelId);
        }

        public static KSQLChannelResponseList GetKSQLChannels(int groupId)
        {
            return APILogic.CRUD.KSQLChannelsManager.List(groupId);
        }
        #endregion

        public static bool UpdateImageState(int groupId, long rowId, int version, eMediaType mediaType, eTableStatus status)
        {
            return Core.Api.api.UpdateImageState(groupId, rowId, version, mediaType, status);
        }

        public static OSSAdapterEntitlementsResponse GetExternalEntitlements(int groupId, string userId)
        {
            return Core.Api.api.GetExternalEntitlements(groupId, userId);
        }


        public static bool ModifyCB(int groupId, string bucket, string key, eDbActionType action, string data, long ttlMinutes)
        {
            return Core.Api.api.ModifyCB(bucket, key, action, data, ttlMinutes);
        }

        public static RegistryResponse GetAllRegistry(int groupId)
        {
            return Core.Api.api.GetAllRegistry(groupId);
        }

        public static int GetGroupIdByUsernamePassword(string username, string password)
        {
            int response = 0;
            int playerID = 0;
            try
            {
                response = TVinciShared.PageUtils.GetGroupByUNPass(username, password, ref playerID);
            }
            catch (Exception ex)
            {
                log.Error("Error while GetGroupIdByUsernamePassword", ex);
            }
            return response;
        }

        public static bool InitializeFreeItemsUpdate(int groupId)
        {
            return Core.Api.api.InitializeFreeItemsUpdate(groupId);
        }

        public static bool UpdateFreeFileTypeOfModule(int groupId, int moduleID)
        {
            bool result = false;

            if (groupId > 0)
            {
                result = Core.Api.api.UpdateFreeFileTypeOfModule(groupId, moduleID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return result;
        }

        public static TimeShiftedTvPartnerSettingsResponse GetTimeShiftedTvPartnerSettings(int groupId)
        {
            return Core.Api.api.GetTimeShiftedTvPartnerSettings(groupId);
        }

        public static Status UpdateTimeShiftedTvPartnerSettings(int groupId, TimeShiftedTvPartnerSettings settings)
        {
            return Core.Api.api.UpdateTimeShiftedTvPartnerSettings(groupId, settings);
        }


        public static Status UpdateTimeShiftedTvEpgChannelsSettings(int groupId, TimeShiftedTvPartnerSettings settings)
        {
            return Core.Api.api.UpdateTimeShiftedTvEpgChannelsSettings(groupId, settings);
        }

        public static ApiObjects.CDNAdapter.CDNAdapterListResponse GetCDNAdapters(int groupId)
        {
            return Core.Api.api.GetCDNAdapters(groupId);
        }

        public static Status DeleteCDNAdapter(int groupId, int adapterId)
        {
            return Core.Api.api.DeleteCDNAdapter(groupId, adapterId);
        }

        public static ApiObjects.CDNAdapter.CDNAdapterResponse InsertCDNAdapter(int groupId, ApiObjects.CDNAdapter.CDNAdapter adapter)
        {
            return Core.Api.api.InsertCDNAdapter(groupId, adapter);
        }

        public static ApiObjects.CDNAdapter.CDNAdapterResponse GenerateCDNSharedSecret(int groupId, int adapterId)
        {
            return Core.Api.api.GenerateCDNSharedSecret(groupId, adapterId);
        }

        public static ApiObjects.CDNAdapter.CDNAdapterResponse SetCDNAdapter(int groupId, ApiObjects.CDNAdapter.CDNAdapter adapter, int adapterID)
        {
            return Core.Api.api.SetCDNAdapter(groupId, adapter, adapterID);
        }

        public static ApiObjects.CDNAdapter.CDNAdapterResponse SendCDNAdapterConfiguration(int groupId, int adapterID)
        {
            return Core.Api.api.SendCDNConfigurationToAdapter(groupId, adapterID);
        }

        public static CDNPartnerSettingsResponse GetCDNPartnerSettings(int groupId)
        {
            return Core.Api.api.GetCDNPartnerSettings(groupId);
        }

        public static CDNPartnerSettingsResponse UpdateCDNPartnerSettings(int groupId, CDNPartnerSettings settings)
        {
            return Core.Api.api.UpdateCDNPartnerSettings(groupId, settings);
        }

        public static ApiObjects.CDNAdapter.CDNAdapterResponse GetCDNAdapter(int groupId, int adapterId)
        {
            return Core.Api.api.GetCdnAdapter(adapterId);
        }

        public static ApiObjects.CDNAdapter.CDNAdapterResponse GetGroupDefaultCDNAdapter(int groupId, eAssetTypes assetType)
        {
            return Core.Api.api.GetGroupDefaultCdnAdapter(groupId, assetType);
        }

        public static bool MigrateStatistics(int groupId, DateTime? startDate)
        {
            return Core.Api.api.MigrateStatistics(groupId, startDate);
        }

        public static ScheduledTaskLastRunDetails GetScheduledTaskLastRun(ApiObjects.ScheduledTaskType scheduledTaskType)
        {
            return Core.Api.api.GetScheduledTaskLastRun(scheduledTaskType);
        }

        public static bool UpdateScheduledTaskNextRunIntervalInSeconds(ApiObjects.ScheduledTaskType scheduledTaskType, double nextRunIntervalInSeconds)
        {
            return Core.Api.api.UpdateScheduledTaskNextRunIntervalInSeconds(scheduledTaskType, nextRunIntervalInSeconds);
        }

        public static DeviceFamilyResponse GetDeviceFamilyList(int groupId)
        {
            return Core.Api.api.GetDeviceFamilyList();
        }

        public static DeviceBrandResponse GetDeviceBrandList(int groupId)
        {
            return Core.Api.api.GetDeviceBrandList();
        }

        public static ApiObjects.CountryResponse GetCountryList(int groupId, List<int> countryIds)
        {
            return Core.Api.api.GetCountryList(countryIds);
        }

        public static MetaResponse GetGroupMetaList(int groupId, eAssetTypes assetType, MetaType metaType, MetaFieldName fieldNameEqual, MetaFieldName fieldNameNotEqual)
        {
            return Core.Api.api.GetGroupMetaList(groupId, assetType, metaType, fieldNameEqual, fieldNameNotEqual);
        }

        public static int GetIPCountryCode(string ip)
        {
            return Core.Api.api.GetIPCountryCode(ip);
        }
    }
}
