using ApiLogic.Api.Managers;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.BulkExport;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.Rules;
using ApiObjects.Segmentation;
using ApiObjects.TimeShiftedTv;
using Core.Api.Managers;
using Core.Api.Modules;
using Core.Catalog.Response;
using Core.Pricing;
using Phx.Lib.Log;
using Newtonsoft.Json.Linq;
using ScheduledTasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using ApiLogic.Modules.Services;
using EventBus.Kafka;
using OTT.Lib.Kafka;
using HouseholdSegment = ApiObjects.Segmentation.HouseholdSegment;
using UserSegment = ApiObjects.Segmentation.UserSegment;

namespace Core.Api
{
    public interface ISegmentsManager
    {
        GenericListResponse<SegmentationType> ListSegmentationTypes(int groupId, HashSet<long> ids, int pageIndex, int pageSize, AssetSearchDefinition assetSearchDefinition);
    }

    public class Module : ISegmentsManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<Module> lazy = new Lazy<Module>(() => new Module());
        private static ISegmentationTypeCrudMessageService  _segmentationTypeMessageService;
        private static IUserSegmentCrudMessageService  _userSegmentMessageService;

        public static Module Instance { get { return lazy.Value; } }

        private Module()
        {
        }

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

        public static RolesResponse GetUserRoles(int groupId, string userId)
        {
            return Core.Api.api.GetUserRoles(groupId, userId);
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
            catch (XmlException)
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

        public static RolesResponse UpdateRole(int groupId, Role role)
        {
            return Core.Api.api.UpdateRole(groupId, role);
        }

        public static ApiObjects.Response.Status DeleteRole(int groupId, long id)
        {
            return Core.Api.api.DeleteRole(groupId, id);
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

        public static string CheckGeoBlockMedia(int groupId, Int32 nMediaID, string sIP)
        {
            string ruleName;
            if (TvmRuleManager.CheckGeoBlockMedia(groupId, nMediaID, sIP, out ruleName))
            {
                return "Geo";
            }

            return "OK";
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
            return Core.Api.api.GetChannelAssets(channelId, groupId, pageIndex, pageSize);
        }

        public static UnifiedSearchResult[] SearchAssets(int groupId, string filter, int pageIndex, int pageSize, bool OnlyIsActive, int languageID, bool UseStartDate,
               string Udid, string UserIP, string SiteGuid, int DomainId, int ExectgroupId, bool IgnoreDeviceRule)
        {
            return Core.Api.api.SearchAssets(groupId, filter, pageIndex, pageSize, OnlyIsActive, languageID, UseStartDate,
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

            return TvmRuleManager.GetGroupMediaRules(nMediaID, sIP, siteGuid.ToString(), groupId, deviceUdid);
        }

        public static List<GroupRule> GetEPGProgramRules(int groupId, int nMediaId, int nProgramId, int siteGuid, string sIP, string deviceUdid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            return TvmRuleManager.GetEPGProgramRules(nProgramId, nMediaId, siteGuid.ToString(), sIP, groupId, deviceUdid);
        }

        public static List<GroupRule> GetNpvrRules(int groupId, RecordedEPGChannelProgrammeObject recordedProgram, int siteGuid, string sIP, string deviceUdid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid;

            return TvmRuleManager.GetNpvrRules(recordedProgram, siteGuid, sIP, groupId, deviceUdid);
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

        public static List<MediaConcurrencyRule> GetMediaConcurrencyRules(int groupId, int nMediaID, int bmID, eBusinessModule eType)
        {
            return Core.Api.api.GetMediaConcurrencyRules(nMediaID, groupId, bmID, eType);
        }

        public static RegionsResponse GetRegions(int groupId, List<string> externalRegionList, RegionOrderBy orderBy, int pageIndex = 0, int pageSize = 0)
        {
            RegionsResponse response = null;
            RegionFilter filter = new RegionFilter() { ExternalIds = externalRegionList };
            var regions = GetRegions(groupId, filter, pageIndex, pageSize);
            if (regions != null)
            {
                response = new RegionsResponse();
                response.Status = regions.Status;
                response.Regions = regions.Objects;
            }

            return response;
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
        public static ParentalRulesResponse GetParentalRules(int groupId, bool isAllowedToViewInactiveAssets = false)
        {
            return Core.Api.api.Instance.GetParentalRules(groupId, !isAllowedToViewInactiveAssets);
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
            return api.GetMediaRules(groupId, siteGuid, mediaId, domainId, ip, udid, orderBy);
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
            return api.GetEpgRules(groupId, siteGuid, epgId, channelMediaId, domainId, ip, orderBy);
        }

        public static GenericRuleResponse GetNPVRRules(int groupId, string siteGuid, long recordingId, long domainId, string ip, GenericRuleOrderBy orderBy)
        {
            return api.GetNPVRRules(groupId, siteGuid, recordingId, domainId, ip, orderBy);
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
            InsertExternalChannel(int groupId, ExternalChannel externalChannel, long userId)
        {
            return api.InsertExternalChannel(groupId, externalChannel, userId);
        }

        public static ApiObjects.Response.Status DeleteExternalChannel(int groupId, int externalChannelId, long userId)
        {
            return api.DeleteExternalChannel(groupId, externalChannelId, userId);
        }

        public static ExternalChannelResponse SetExternalChannel(int groupId, ExternalChannel externalChannel, long userId)
        {
            return api.SetExternalChannel(groupId, externalChannel, userId);
        }

        public static ExternalChannelResponseList GetExternalChannels(int groupId)
        {
            return Core.Api.api.GetExternalChannels(groupId);
        }

        public static ExternalChannelResponseList ListExternalChannels(int groupId, long userId)
        {
            return api.ListExternalChannels(groupId, userId);
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

        public static string GetCurrentUserPermissions(int groupId, string userId)
        {
            return Core.Api.api.GetCurrentUserPermissions(groupId, userId);
        }

        public static PermissionsResponse GetGroupPermissions(int groupId, long? roleIdIn)
        {
            return Core.Api.api.GetGroupPermissions(groupId, roleIdIn);
        }

        public static PermissionsResponse GetUserPermissions(int groupId, string userId)
        {
            return Core.Api.api.GetUserPermissions(groupId, userId);
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

        public static KSQLChannelResponse SetKSQLChannel(int groupId, KSQLChannel channel, long userId)
        {
            return APILogic.CRUD.KSQLChannelsManager.Set(groupId, channel, userId);
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
            return Core.Api.api.GetCdnAdapter(groupId, adapterId);
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

        public static ApiObjects.CountryLocaleResponse GetCountryList(int groupId, List<int> countryIds)
        {
            return Core.Api.api.GetCountryLocaleList(countryIds, groupId);
        }

        public static Country GetCountryByIp(int groupId, string ip)
        {
            return Core.Api.api.GetCountryByIp(groupId, ip);
        }

        public static Country GetCountryByCountryName(int groupId, string countryName)
        {
            return Core.Api.api.GetCountryByCountryName(groupId, countryName);
        }

        public static string GetLayeredCacheGroupConfig(int groupId)
        {
            return Core.Api.api.GetLayeredCacheGroupConfig(groupId);
        }

        public static long GetInvalidationKeyValue(int groupId, string layeredCacheConfigName, string invalidationKey)
        {
            return Core.Api.api.GetInvalidationKeyValue(groupId, layeredCacheConfigName, invalidationKey);
        }

        public static bool UpdateLayeredCacheGroupConfig(int groupId, int? version, bool? disableLayeredCache, List<string> layeredCacheSettingsToExclude, bool? shouldOverrideExistingExcludeSettings,
                                                            List<string> layeredCacheInvalidationKeySettingsToExclude, bool? shouldOverrideExistingInvalidationKeyExcludeSettings)
        {
            return Core.Api.api.UpdateLayeredCacheGroupConfig(groupId, version, disableLayeredCache, layeredCacheSettingsToExclude, shouldOverrideExistingExcludeSettings,
                                                                layeredCacheInvalidationKeySettingsToExclude, shouldOverrideExistingInvalidationKeyExcludeSettings);
        }

        public static bool UpdateLayeredCacheGroupConfigST(int groupId, int version, bool disableLayeredCache, string layeredCacheSettingsToExcludeCommaSeperated, bool shouldOverrideExistingExcludeSettings,
                                                            string layeredCacheInvalidationKeySettingsToExcludeCommaSeperated, bool shouldOverrideExistingInvalidationKeyExcludeSettings)
        {
            string[] layeredCacheSettingsToExclude = layeredCacheSettingsToExcludeCommaSeperated.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (layeredCacheSettingsToExclude == null)
            {
                layeredCacheSettingsToExclude = new string[0];
            }

            string[] layeredCacheInvalidationKeySettingsToExclude = layeredCacheInvalidationKeySettingsToExcludeCommaSeperated.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (layeredCacheInvalidationKeySettingsToExclude == null)
            {
                layeredCacheInvalidationKeySettingsToExclude = new string[0];
            }

            return Core.Api.api.UpdateLayeredCacheGroupConfig(groupId, version, disableLayeredCache, new List<string>(layeredCacheSettingsToExclude), shouldOverrideExistingExcludeSettings,
                                                                new List<string>(layeredCacheInvalidationKeySettingsToExclude), shouldOverrideExistingInvalidationKeyExcludeSettings);
        }

        public static bool IncrementLayeredCacheGroupConfigVersion(int groupId)
        {
            return Core.Api.api.IncrementLayeredCacheGroupConfigVersion(groupId);
        }

        public static Status ClearLocalServerCache(string action, string key)
        {
            return Core.Api.api.ClearLocalServerCache(action, key);
        }

        public static bool DoActionRules(bool isSingleRun = false)
        {
            bool result = false;

            try
            {
                result = Core.Api.api.DoActionRules(isSingleRun);
            }
            catch (Exception ex)
            {
                result = false;
                log.ErrorFormat("Error in DoActionRules", ex);
            }

            return result;
        }

        public static bool DoActionRules(int groupId, List<long> ruleIds)
        {
            bool result = false;

            try
            {
                result = Core.Api.api.DoActionRules(groupId, ruleIds);
            }
            catch (Exception ex)
            {
                result = false;
                log.Error($"Error in DoActionRules. ruleIds = {((ruleIds != null && ruleIds.Count > 0) ? string.Join(",", ruleIds) : string.Empty)}", ex);
            }

            return result;
        }

        public static FriendlyAssetLifeCycleRuleResponse GetFriendlyAssetLifeCycleRule(int groupId, long id)
        {
            FriendlyAssetLifeCycleRuleResponse result = new FriendlyAssetLifeCycleRuleResponse();

            try
            {
                result = Core.Api.api.GetFriendlyAssetLifeCycleRule(groupId, id);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetFriendlyAssetLifeCycleRule, groupId: {0}, id: {1}", groupId, id), ex);
            }

            return result;
        }

        public static FriendlyAssetLifeCycleRuleResponse InsertOrUpdateFriendlyAssetLifeCycleRule(int groupId, FriendlyAssetLifeCycleRule rule)
        {
            FriendlyAssetLifeCycleRuleResponse result = new FriendlyAssetLifeCycleRuleResponse();

            try
            {
                result = Core.Api.api.InsertOrUpdateFriendlyAssetLifeCycleRule(groupId, rule);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in InsertOrUpdateFriendlyAssetLifeCycleRule, groupId: {0}, id: {1}, name: {2}", groupId, rule.Id, rule.Name), ex);
            }

            return result;
        }

        public static bool InsertOrUpdateAssetLifeCycleRulePpvsAndFileTypes(int groupId, FriendlyAssetLifeCycleRule rule)
        {
            bool result = false;

            try
            {
                result = Core.Api.api.InsertOrUpdateAssetLifeCycleRulePpvsAndFileTypes(groupId, rule);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in InsertOrUpdateAssetLifeCycleRulePpvsAndFileTypes, groupId: {0}, id: {1}", groupId, rule.Id), ex);
            }

            return result;
        }

        public static ApiObjects.CountryLocaleResponse GetCountryLocaleByIp(int groupId, string ip)
        {
            return Core.Api.api.GetCountryLocaleByIp(groupId, ip);
        }

        public static LanguageResponse GetLanguageList(int groupId, List<string> languageCodes)
        {
            return Core.Api.api.GetLanguageList(groupId, languageCodes);
        }

        public static CurrencyResponse GetCurrencyList(int groupId, List<string> currencyCodes)
        {
            return Core.Api.api.GetCurrencyList(groupId, currencyCodes);
        }

        public static List<int> GetMediaFilesByMediaId(int groupId, int mediaId)
        {
            return Core.Api.api.GetMediaFilesByMediaId(groupId, mediaId);
        }

        public static Status SaveSearchHistory(string name, string service, string action, string language, int groupId, string userId, string deviceId, JObject persistedFilter)
        {
            return Core.Api.api.SaveSearchHistory(name, service, action, language, groupId, userId, deviceId, persistedFilter);
        }

        public static SearchHistoryResponse GetSearchHistory(int groupId, string userId, string udid, string language, int pageIndex, int? pageSize)
        {
            return Core.Api.api.GetSearchHistory(groupId, userId, udid, language, pageIndex, pageSize);
        }

        public static Status CleanSearchHistory(int groupId, string userId)
        {
            return Core.Api.api.CleanSearchHistory(groupId, userId);
        }

        public static Status DeleteSearchHistory(int groupId, string userId, string id)
        {
            return Core.Api.api.DeleteSearchHistory(groupId, userId, id);
        }

        public static Status CleanUserAssetHistory(int groupId, string userId, List<KeyValuePair<int, eAssetTypes>> assets)
        {
            return Core.Api.api.CleanUserAssetHistory(groupId, userId, assets);
        }

        public static RolesResponse AddRole(int groupId, Role role)
        {
            return Core.Api.api.AddRole(groupId, role);
        }

        public static DrmAdapterResponse SendDrmAdapterConfiguration(int groupId, int adapterID)
        {
            return Core.Api.api.SendDrmConfigurationToAdapter(groupId, adapterID);
        }

        public static StringResponse GetCustomDrmAssetLicenseData(int groupId, int drmAdapterId, string userId, string assetId, eAssetTypes eAssetTypes, int fileId,
            string externalFileId, string ip, string udid, PlayContextType contextType, string recordingId)
        {
            return Core.Api.api.GetCustomDrmAssetLicenseData(groupId, drmAdapterId, userId, assetId, eAssetTypes, fileId, externalFileId, ip, udid, contextType, recordingId);
        }

        public static StringResponse GetCustomDrmDeviceLicenseData(int groupId, int drmAdapterId, string userId, string udid, string deviceFamily, int deviceBrandId, string ip)
        {
            return Core.Api.api.GetCustomDrmDeviceLicenseData(groupId, drmAdapterId, userId, udid, deviceFamily, deviceBrandId, ip);
        }

        public static List<int> GetMediaConcurrencyRulesByDomainLimitionModule(int groupId, int dlmId)
        {
            return Core.Api.api.GetMediaConcurrencyRulesByDeviceLimitionModule(groupId, dlmId);
        }

        public static List<long> GetUserWatchedMediaIds(int groupId, int userId)
        {
            return Core.Api.api.GetUserWatchedMediaIds(groupId, userId);
        }

        public static GenericResponse<AssetRule> AddAssetRule(int groupId, AssetRule assetRule)
        {
            return Core.Api.api.AddAssetRule(groupId, assetRule);
        }

        public static Status DeleteAssetRule(int groupId, long id)
        {
            return Core.Api.api.DeleteAssetRule(groupId, id);
        }

        public static GenericListResponse<AssetRule> GetAssetRules(RuleConditionType assetRuleConditionType, int groupId, SlimAsset slimAsset = null, ApiObjects.RuleActionType? ruleActionType = null)
        {
            return Core.Api.api.GetAssetRules(assetRuleConditionType, groupId, slimAsset, ruleActionType);
        }

        public static GenericResponse<AssetRule> UpdateAssetRule(int groupId, AssetRule assetRuleRequest)
        {
            return Core.Api.api.UpdateAssetRule(groupId, assetRuleRequest);
        }

        public static bool DoActionAssetRules(bool isSingleRun = false)
        {
            bool result = false;

            try
            {
                result = Core.Api.api.DoActionAssetRules(isSingleRun);
            }
            catch (Exception ex)
            {
                result = false;
                log.ErrorFormat("Error in DoActionAssetRules", ex);
            }

            return result;
        }

        #region AssetUserRule

        public static GenericListResponse<AssetUserRule> GetAssetUserRuleList(int groupId, long? userId, RuleActionType? ruleActionType, RuleConditionType? ruleConditionType, bool returnConfigError)
        {
            return AssetUserRuleManager.GetAssetUserRuleList(groupId, userId, false, ruleActionType, ruleConditionType, returnConfigError);
        }

        public static GenericResponse<AssetUserRule> AddAssetUserRule(int groupId, AssetUserRule assetUserRuleToAdd)
        {
            return AssetUserRuleManager.AddAssetUserRule(groupId, assetUserRuleToAdd);
        }

        public static GenericResponse<AssetUserRule> UpdateAssetUserRule(int groupId, long assetUserRuleId, AssetUserRule assetUserRuleToUpdate, long userId)
        {
            return AssetUserRuleManager.UpdateAssetUserRule(groupId, assetUserRuleId, assetUserRuleToUpdate, userId);
        }

        public static Status DeleteAssetUserRule(int groupId, long assetUserRuleId, long userId)
        {
            return AssetUserRuleManager.DeleteAssetUserRule(groupId, assetUserRuleId, userId);
        }

        public static Status AddAssetUserRuleToUser(long userId, long ruleId, int groupId)
        {
            return AssetUserRuleManager.AddAssetUserRuleToUser(userId, ruleId, groupId);
        }

        public static Status DeleteAssetUserRuleFromUser(long userId, long ruleId, int groupId)
        {
            return AssetUserRuleManager.DeleteAssetUserRuleFromUser(userId, ruleId, groupId);
        }

        public static GenericResponse<AssetRule> GetAssetRule(int groupId, long assetRuleId)
        {
            return Core.Api.api.GetAssetRule(groupId, assetRuleId);
        }

        #endregion

        public static GenericListResponse<DeviceConcurrencyPriority> GetConcurrencyPartner(int groupId)
        {
            GenericListResponse<DeviceConcurrencyPriority> response = new GenericListResponse<DeviceConcurrencyPriority>();
            var deviceConcurrencyPriority = Core.Api.api.GetDeviceConcurrencyPriority(groupId);

            if (deviceConcurrencyPriority != null)
            {
                response.Objects.Add(deviceConcurrencyPriority);
            }

            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        public static Status UpdateConcurrencyPartner(int groupId, DeviceConcurrencyPriority deviceConcurrencyPriorityToUpdate)
        {
            return Core.Api.api.UpdateDeviceConcurrencyPriority(groupId, deviceConcurrencyPriorityToUpdate);
        }

        public static GenericListResponse<MediaConcurrencyRule> GetMediaConcurrencyRules(int groupId)
        {
            GenericListResponse<MediaConcurrencyRule> response = new GenericListResponse<MediaConcurrencyRule>();

            var mediaConcurrencyRules = Core.Api.api.GetGroupMediaConcurrencyRules(groupId);

            if (mediaConcurrencyRules != null)
            {
                if (mediaConcurrencyRules.Count > 0)
                {
                    response.Objects.AddRange(mediaConcurrencyRules.GroupBy(x => x.RuleID).Select(x => x.First()).ToList());
                }

                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        public static bool SetLayeredCacheInvalidationKey(string key)
        {
            return CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(key);
        }

        public static GenericResponse<SegmentationType> AddSegmentationType(int groupId, SegmentationType segmentationType, long userId)
        {
            GenericResponse<SegmentationType> response = segmentationType.ValidateForInsert();
            if (!response.IsOkStatusCode())
            {
                return response;
            }

            try
            {
                segmentationType.GroupId = groupId;

                if (!segmentationType.Insert())
                {
                    response.SetStatus(eResponseStatus.Error, "Failed inserting segmentation type.");
                }
                else
                {
                    if (segmentationType?.Actions?.Count > 0)
                    {
                        var virtualAssetInfo = new VirtualAssetInfo()
                        {
                            Type = ObjectVirtualAssetInfoType.Segment,
                            Id = segmentationType.Id,
                            Name = segmentationType.Name,
                            Description = segmentationType.Description,
                            UserId = userId,
                            IsActive = true
                        };

                        var res = api.Instance.AddVirtualAsset(groupId, virtualAssetInfo);

                        if (res.Status == VirtualAssetInfoStatus.Error)
                        {
                            log.Error($"Error while AddVirtualAsset - segmentationType: {segmentationType.Id} will delete ");
                            DeleteSegmentationType(groupId, segmentationType.Id, userId, true);
                            response.SetStatus(eResponseStatus.Error, "Failed inserting segmentation type.");
                            return response;
                        }
                    }

                    response.Object = segmentationType;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    _segmentationTypeMessageService?.PublishCreateEventAsync(groupId, segmentationType).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed inserting segmentation type. ex = {0}", ex);
                response.SetStatus(eResponseStatus.Error, "Error while inserting segmentation type.");
            }

            return response;
        }

        public static GenericResponse<SegmentationType> UpdateSegmentationType(int groupId, SegmentationType segmentationType, long userId)
        {
            segmentationType.GroupId = groupId;
            GenericResponse<SegmentationType> response = segmentationType.ValidateForUpdate();
            if (!response.IsOkStatusCode())
            {
                return response;
            }

            try
            {
                var assetSearchDefinition = new AssetSearchDefinition() { UserId = userId };
                var filter = api.Instance.GetObjectVirtualAssetObjectIds(groupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Segment, new HashSet<long>() { segmentationType.Id });
                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                {
                    response.SetStatus(filter.Status);
                    return response;
                }

                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                {
                    response.SetStatus((int)eResponseStatus.ObjectNotExist, "Given Id does not exist for group");
                    return response;
                }

                // Due to atomic action update virtual asset before segmentation update
                var virtualAssetInfo = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Segment,
                    Id = segmentationType.Id,
                    Name = segmentationType.Name,
                    Description = segmentationType.Description,
                    UserId = userId
                };

                var virtualAssetInfoResponse = api.Instance.UpdateVirtualAsset(groupId, virtualAssetInfo);

                if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
                {
                    log.Error($"Error while update segmentationType's virtualAsset. groupId: {groupId}, segmentationTypeId: {segmentationType.Id}, segmentationTypeName: {segmentationType.Name} ");
                    response.SetStatus(eResponseStatus.Error, "Error while updating segmentation type.");
                    return response;
                }

                if (!segmentationType.Update())
                {
                    response.SetStatus(segmentationType.ActionStatus);
                }
                else
                {

                    response.Object = segmentationType;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    _segmentationTypeMessageService?.PublishUpdateEventAsync(groupId, segmentationType).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed updating segmentation type. ex = {0}", ex);
                response.SetStatus(eResponseStatus.Error, "Error while updating segmentation type.");
            }

            return response;
        }

        public static Status DeleteSegmentationType(int groupId, long id, long userId, bool DBOnly = false)
        {
            Status result = new Status();
            bool deleteResult = false;
            try
            {
                SegmentationType segmentationType = new SegmentationType();
                result = segmentationType.ValidateForDelete(groupId, id);

                if (!result.IsOkStatusCode())
                {
                    return result;
                }

                if (!DBOnly)
                {
                    var assetSearchDefinition = new AssetSearchDefinition() { UserId = userId };
                    var filter = api.Instance.GetObjectVirtualAssetObjectIds(groupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Segment, new HashSet<long>() { id });
                    if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                    {
                        return filter.Status;
                    }

                    if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                    {
                        result.Set(eResponseStatus.ObjectNotExist, eResponseStatus.ObjectNotExist.ToString());
                        return result;
                    }
                }

                var segmentationTypeList = SegmentationType.ListFromCb(groupId, new List<long>() { id }, 0, 0, out int totalcount);
                if (segmentationTypeList == null || segmentationTypeList.Count != 1 || segmentationTypeList[0].Id != id)
                {
                    result.Set(eResponseStatus.ObjectNotExist, "Given Id does not exist for group");
                    return result;
                }

                segmentationType = segmentationTypeList[0];

                if (!DBOnly)
                {
                    // Due to atomic action delete virtual asset before SegmentationType delete
                    // Delete the virtual asset
                    var virtualAssetInfo = new VirtualAssetInfo()
                    {
                        Type = ObjectVirtualAssetInfoType.Segment,
                        Id = segmentationType.Id,
                        Name = segmentationType.Name,
                        Description = segmentationType.Description,
                        UserId = userId
                    };

                    var response = api.Instance.DeleteVirtualAsset(groupId, virtualAssetInfo);
                    if (response.Status == VirtualAssetInfoStatus.Error)
                    {
                        log.Error($"Error while delete segment virtual asset id {virtualAssetInfo.ToString()}");
                        result.Set(eResponseStatus.Error, $"Failed to delete segmentationType { id}");
                        return result;
                    }
                }

                deleteResult = segmentationType.Delete();

                if (!deleteResult)
                {
                    result = segmentationType.ActionStatus;
                }
                else
                {
                    result.Set(eResponseStatus.OK);
                    _segmentationTypeMessageService?.PublishDeleteEventAsync(groupId, id).GetAwaiter().GetResult();
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed deleting segmentation type. ex = {0}", ex);
                result = new Status((int)eResponseStatus.Error, "Error while deleting segmentation type.");
            }

            return result;
        }

        public GenericListResponse<SegmentationType> ListSegmentationTypes(int groupId, HashSet<long> ids, int pageIndex, int pageSize, AssetSearchDefinition assetSearchDefinition)
        {
            GenericListResponse<SegmentationType> result = new GenericListResponse<SegmentationType>();

            try
            {
                var filter = api.Instance.GetObjectVirtualAssetObjectIds(groupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Segment, ids, pageIndex, pageSize);
                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                {
                    result.SetStatus(filter.Status);
                    return result;
                }

                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                {
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    return result;
                }

                int totalCount;
                result.Objects = ApiLogic.Segmentation.SegmentationTypeLogic.List(groupId, filter.ObjectIds?.ToList(), pageIndex, pageSize, out totalCount);
                result.TotalItems = totalCount;
                result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed getting segmentation types of group id = {0}. ex = {1}", groupId, ex);
                result.SetStatus(eResponseStatus.Error, "Failed getting segmentation types");
            }

            return result;
        }

        public static GenericResponse<PersonalListItem> AddPersonalListItemForUser(int groupId, PersonalListItem personalListItem, long userId)
        {
            return api.AddPersonalListItemForUser(groupId, personalListItem, userId);
        }

        public static GenericListResponse<PersonalListItem> GetUserPersonalListItems(int groupId, long userId, int pageSize, int pageIndex, OrderDiretion order, HashSet<int> partnerListTypes)
        {
            return api.GetUserPersonalListItems(groupId, userId, pageSize, pageIndex, order, partnerListTypes);
        }

        public static Status DeletePersonalListItemForUser(int groupId, long personalListItemId, long userId)
        {
            return api.DeletePersonalListItemForUser(groupId, personalListItemId, userId);
        }

        public static GenericListResponse<UserSegment> GetUserSegments(int groupId, string userId, AssetSearchDefinition assetSearchDefinition, int pageIndex, int pageSize)
        {
            GenericListResponse<UserSegment> result = new GenericListResponse<UserSegment>();

            try
            {
                var userSegments = ApiLogic.Segmentation.UserSegmentLogic.List(groupId, userId, out int totalCount);
                if (totalCount > 0)
                {
                    var segmentTypeIds = SegmentBaseValue.GetSegmentationTypeOfSegmentIds(userSegments);
                    if (segmentTypeIds?.Count > 0)
                    {
                        var filtered = api.Instance.GetObjectVirtualAssetObjectIds(groupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Segment, new HashSet<long>(segmentTypeIds.Values.ToList()));
                        if (filtered.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                        {
                            result.SetStatus(filtered.Status);
                            return result;
                        }

                        if (filtered.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                        {
                            result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                            return result;
                        }

                        if (filtered.ObjectIds?.Count > 0)
                        {
                            result.Objects = new List<UserSegment>();

                            foreach (var segmentId in userSegments)
                            {
                                if (segmentTypeIds.ContainsKey(segmentId))
                                {
                                    if (filtered.ObjectIds.Contains(segmentTypeIds[segmentId]))
                                    {
                                        result.Objects.Add(new UserSegment{UserId = userId, SegmentId = segmentId});
                                    }
                                }
                            }

                            result.TotalItems = result.Objects.Count;

                            if (pageSize > 0)
                            {
                                result.Objects = result.Objects.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                            }
                        }
                    }
                }

                result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed getting user segments of user id {0} in group id = {1}. ex = {2}", userId, groupId, ex);
                result.SetStatus(eResponseStatus.Error, "Failed getting user segments");
            }

            return result;
        }

        public static GenericResponse<UserSegment> AddUserSegment(int groupId, UserSegment userSegment)
        {
            GenericResponse<UserSegment> response = new GenericResponse<UserSegment>();

            try
            {
                var userDataResponse = Core.Users.Module.GetUserData(groupId, userSegment.UserId, string.Empty);

                if (userDataResponse == null || userDataResponse.m_user == null || userDataResponse.m_RespStatus != ResponseStatus.OK)
                {
                    response.SetStatus((int)eResponseStatus.InvalidUser, "Invalid user");
                }
                else
                {
                    userSegment.GroupId = groupId;

                    long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(userSegment.SegmentId);
                    if (segmentationTypeId == 0)
                    {
                        response.SetStatus(eResponseStatus.ObjectNotExist, "Segment not exist");
                        return response;
                    }

                    var filter = api.Instance.GetObjectVirtualAssetObjectIds(groupId, new AssetSearchDefinition() { UserId = long.Parse(userSegment.UserId) },
                        ObjectVirtualAssetInfoType.Segment, new System.Collections.Generic.HashSet<long>() { segmentationTypeId });

                    if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                    {
                        response.SetStatus(filter.Status);
                        return response;
                    }

                    if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                    {
                        response.SetStatus(new Status((int)eResponseStatus.ObjectNotExist, "Object Not Exist"));
                        return response;
                    }

                    if (!userSegment.Insert())
                    {
                        response.SetStatus(eResponseStatus.Error, "Failed inserting user segment.");
                    }
                    else
                    {
                        response.Object = userSegment;
                        response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                            _userSegmentMessageService?.PublishCreateEventAsync(groupId, userSegment).GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed inserting user segment. ex = {0}", ex);
                response.SetStatus(eResponseStatus.Error, "Error while inserting user segment.");
            }

            return response;
        }

        public static Status DeleteUserSegment(int groupId, string userId, long segmentId)
        {
            Status result = null;
            bool deleteResult = false;
            try
            {
                long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(segmentId);
                if (segmentationTypeId == 0)
                {
                    return new Status(eResponseStatus.ObjectNotExist, "Segment not exist");
                }

                var filter = api.Instance.GetObjectVirtualAssetObjectIds(groupId, new AssetSearchDefinition() { UserId = long.Parse(userId) },
                    ObjectVirtualAssetInfoType.Segment, new System.Collections.Generic.HashSet<long>() { segmentationTypeId });

                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                {
                    return filter.Status;
                }

                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                {
                    return new Status(eResponseStatus.ObjectNotExist, "Object Not Exist");
                }

                UserSegment segmentationType = new UserSegment()
                {
                    GroupId = groupId,
                    UserId = userId,
                    SegmentId = segmentId
                };

                deleteResult = segmentationType.Delete();

                if (!deleteResult)
                {
                    result = segmentationType.ActionStatus;
                }
                else
                {
                    result = new Status();
                    _userSegmentMessageService?.PublishDeleteEventAsync(groupId, segmentationType).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed deleting user segment. ex = {0}", ex);
                result = new Status((int)eResponseStatus.Error, "Error while deleting user segment.");
            }

            return result;
        }

        public static GenericResponse<HouseholdSegment> AddHouseholdSegment(int groupId, HouseholdSegment householdSegmentToAdd)
        {
            throw new NotImplementedException();
        }

        public static Status DeleteHouseholdSegment(int groupId, long householdId, long segmentId)
        {
            throw new NotImplementedException();
        }

        public static GenericResponse<BusinessModuleRule> UpdateBusinessModuleRule(int groupId, BusinessModuleRule businessModuleRule)
        {
            return Core.Api.api.UpdateBusinessModuleRule(groupId, businessModuleRule);
        }

        public static Status DeleteBusinessModuleRule(int groupId, long id)
        {
            return Core.Api.api.DeleteBusinessModuleRule(groupId, id);
        }

        public static GenericResponse<BusinessModuleRule> GetBusinessModuleRule(int groupId, long businessModuleRuleId)
        {
            return Core.Api.api.GetBusinessModuleRule(groupId, businessModuleRuleId);
        }

        public static GenericResponse<BusinessModuleRule> AddBusinessModuleRule(int groupId, BusinessModuleRule businessModuleRuleToAdd)
        {
            return Core.Api.api.AddBusinessModuleRule(groupId, businessModuleRuleToAdd);
        }

        public static GenericListResponse<BusinessModuleRule> GetBusinessModuleRules(int groupId, BusinessModuleRuleConditionScope filter, RuleActionType? ruleActionType)
        {
            return Core.Api.api.GetBusinessModuleRules(groupId, filter, ruleActionType);
        }

        public static GenericListResponse<PlaybackProfile> GetPlaybackProfiles(int groupId)
        {
            return Core.Api.api.GetPlaybackAdapters(groupId);
        }

        public static GenericResponse<PlaybackProfile> AddPlaybackAdapter(int groupId, string userId, PlaybackProfile playbackAdapterToAdd)
        {
            return Core.Api.api.AddPlaybackAdapter(groupId, userId, playbackAdapterToAdd);
        }

        public static GenericResponse<PlaybackProfile> GeneratePlaybackAdapterSharedSecret(int groupId, long playbackAdapterId)
        {
            return Core.Api.api.GeneratePlaybackAdapterSharedSecret(groupId, playbackAdapterId);
        }

        public static GenericResponse<PlaybackProfile> UpdatePlaybackAdapter(int groupId, string userId, PlaybackProfile playbackAdapterToUpdate)
        {
            return Core.Api.api.UpdatePlaybackAdapter(groupId, userId, playbackAdapterToUpdate);
        }

        public static Status DeletePlaybackAdapter(int groupId, string userId, int id)
        {
            return Core.Api.api.DeletePlaybackAdapter(groupId, userId, id);
        }

        public static GenericListResponse<PlaybackProfile> GetPlaybackProfile(int groupId, long playbackProfileId, bool shouldGetOnlyActive)
        {
            return Core.Api.api.GetPlaybackProfile(groupId, playbackProfileId, shouldGetOnlyActive);

        }

        public static GenericResponse<ApiObjects.PlaybackAdapter.PlaybackContext> GetPlaybackContext(long adapterId, int groupId, string userId, string udid, string ip,
                                                                          ApiObjects.PlaybackAdapter.PlaybackContext playbackContext,
                                                                          ApiObjects.PlaybackAdapter.RequestPlaybackContextOptions requestPlaybackContextOptions)
        {
            return Core.Api.api.GetPlaybackAdapterContext(adapterId, groupId, userId, udid, ip, playbackContext, requestPlaybackContextOptions);
        }

        public static Status UpdateGeneralPartnerConfig(int groupId, GeneralPartnerConfig partnerConfigToUpdate)
        {
            if (partnerConfigToUpdate.EnableMultiLcns == false)
            {
                var getRegionsResponse = RegionManager.Instance.GetRegions(groupId, new RegionFilter { ExclusiveLcn = false });
                if (getRegionsResponse.IsOkStatusCode())
                {
                    var multiLcnsRegion = getRegionsResponse.Objects
                        .FirstOrDefault(x => x.linearChannels.Count != x.linearChannels.Select(_ => _.Key).Distinct().Count());
                    if (multiLcnsRegion != null)
                    {
                        return new Status(eResponseStatus.Error, $"Region with id={multiLcnsRegion.id} is configured with multi LCNs.");
                    }
                }
                else
                {
                    return getRegionsResponse.Status;
                }
            }

            return GeneralPartnerConfigManager.Instance.UpdateGeneralPartnerConfig(groupId, partnerConfigToUpdate);
        }

        public static Status UpdateOpcPartnerConfig(int groupId, OpcPartnerConfig partnerConfigToUpdate)
        {
            return PartnerConfigurationManager.Instance.UpdateOpcPartnerConfig(groupId, partnerConfigToUpdate);
        }

        public static GenericListResponse<OpcPartnerConfig> GetOpcPartnerConfiguration(int groupId)
        {
            return PartnerConfigurationManager.GetOpcPartnerConfiguration(groupId);
        }

        public static GenericListResponse<GeneralPartnerConfig> GetGeneralPartnerConfiguration(int groupId)
        {
            return GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfiguration(groupId);
        }

        public static LanguageResponse GetAllLanguageList(int groupId)
        {
            LanguageResponse result = new LanguageResponse();
            result.Languages = GeneralPartnerConfigManager.Instance.GetAllLanguages(groupId);
            if (result.Languages == null)
            {
                result.Status.Set((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            else
            {
                result.Status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return result;
        }

        public static CurrencyResponse GetCurrencyList(int groupId)
        {
            CurrencyResponse result = new CurrencyResponse();
            result.Currencies = GeneralPartnerConfigManager.Instance.GetCurrencyList(groupId);
            if (result.Currencies == null)
            {
                result.Status.Set((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            else
            {
                result.Status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return result;
        }

        public static GenericResponse<Permission> AddPermission(int groupId, Permission permission, long userId)
        {
            GenericResponse<Permission> result = new GenericResponse<Permission>();
            try
            {
                result = api.AddPermission(groupId, permission, userId);
            }
            catch (Exception ex)
            {
                log.Error("Exception in AddPermission", ex);
            }
            return result;
        }

        public static Status DeletePermission(int groupId, long id)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                result = api.DeletePermission(groupId, id);
            }
            catch (Exception ex)
            {
                log.Error("Exception in DeletePermission", ex);
            }
            return result;
        }

        public static List<string> GetGroupFeatures(int groupId)
        {
            Dictionary<string, Permission> groupfeatures = api.GetGroupFeatures(groupId);
            if (groupfeatures?.Count > 0)
            {
                return api.GetGroupFeatures(groupId).Keys.ToList();
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, List<string>> GetPermissionItemsToFeatures(int groupId)
        {
            return api.GetPermissionItemsToFeatures(groupId);
        }

        public static GenericResponse<Region> AddRegion(int groupId, Region region, long userId)
            => RegionManager.Instance.AddRegion(groupId, region, userId);

        public static GenericResponse<Region> UpdateRegion(int groupId, Region region, long userId)
            => RegionManager.Instance.UpdateRegion(groupId, region, userId);

        public static Status DeleteRegion(int groupId, int id, long userId)
            => RegionManager.Instance.DeleteRegion(groupId, id, userId);

        public static GenericListResponse<Region> GetRegions(int groupId, RegionFilter filter, int pageIndex, int pageSize)
            => RegionManager.Instance.GetRegions(groupId, filter, pageIndex, pageSize);

        public static Status BulkUpdateRegions(int groupId, long userId, long linearChannelId, IReadOnlyCollection<RegionChannelNumber> regionChannelNumbers)
            => RegionManager.Instance.BulkUpdateRegions(groupId, userId, linearChannelId, regionChannelNumbers);

        public static Status UpdateObjectVirtualAssetPartnerConfiguration(int groupId, ObjectVirtualAssetPartnerConfig partnerConfigToUpdate)
        {
            return VirtualAssetPartnerConfigManager.Instance.UpdateObjectVirtualAssetPartnerConfiguration(groupId, partnerConfigToUpdate);
        }

        public static GenericListResponse<ObjectVirtualAssetPartnerConfig> GetObjectVirtualAssetPartnerConfiguration(int groupId)
        {
            return VirtualAssetPartnerConfigManager.Instance.GetObjectVirtualAssetPartnerConfiguration(groupId);
        }

        public static List<long> GetUserAndHouseholdSegmentIds(int groupId, string userId, long householdId = -1)
        {
            List<long> result = new List<long>();

            try
            {
                result = ApiLogic.Segmentation.UserSegmentLogic.List(groupId, userId, out int totalCount);

                if (householdId == -1)
                {
                    var user = Users.Module.GetUserData(groupId, userId, string.Empty);
                    if (user != null && user.m_user != null && user.m_user.m_domianID > 0)
                    {
                        householdId = user.m_user.m_domianID;
                    }
                }

                if (householdId > 0)
                {
                    result.AddRange(ApiLogic.Segmentation.HouseholdSegmentLogic.List(groupId, householdId, out totalCount));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed getting User And Household SegmentIds of user id {0} in group id = {1}. ex = {2}", userId, groupId, ex);
            }

            return result;
        }

        public static GenericResponse<ApiObjects.PlaybackAdapter.PlaybackContext> GetPlaybackManifest(long adapterId, int groupId, ApiObjects.PlaybackAdapter.PlaybackContext playbackContext,
            ApiObjects.PlaybackAdapter.RequestPlaybackContextOptions requestPlaybackContextOptions, string userId, string udid, string ip)
        {
            return api.GetPlaybackAdapterManifest(adapterId, groupId, playbackContext, requestPlaybackContextOptions, userId, udid, ip);
        }
        public static GenericListResponse<Region> GetDefaultRegion(int groupId)
            => RegionManager.Instance.GetDefaultRegion(groupId);

        public static GenericListResponse<SegmentationType> GetSegmentationTypesBySegmentIds(int groupId, List<long> ids, int pageIndex, int pageSize,
            AssetSearchDefinition assetSearchDefinition)
        {
            GenericListResponse<SegmentationType> result = new GenericListResponse<SegmentationType>();

            try
            {
                var segmentTypeIds = SegmentBaseValue.GetSegmentationTypeOfSegmentIds(ids);

                if (segmentTypeIds?.Count > 0)
                {
                    result = Instance.ListSegmentationTypes(groupId, new HashSet<long>(segmentTypeIds.Values.ToList()), pageIndex, pageSize, assetSearchDefinition);
                }
                else
                {
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetSegmentationTypesBySegmentIds of group id = {0}. ex = {1}", groupId, ex);
                result.SetStatus(eResponseStatus.Error, "Failed getting segmentation types");
            }

            return result;
        }

        public static GenericListResponse<ExternalChannel> ListExternalChannels(int groupId, long userId, List<long> list)
        {
            return api.ListExternalChannels(groupId, userId, list);
        }

        public static GenericListResponse<Permission> GetGroupPermissionsByIds(int groupId, List<long> permissionIds)
        {
            return api.GetGroupPermissionsByIds(groupId, permissionIds);
        }

        public static GenericResponse<Permission> UpdatePermission(int groupId, long userId, long id, Permission permission)
        {
            GenericResponse<Permission> result = new GenericResponse<Permission>();
            try
            {
                result = api.UpdatePermission(groupId, userId, id, permission);
            }
            catch (Exception ex)
            {
                log.Error("Exception in UpdatePermission", ex);
            }
            return result;
        }
        
        public static void InitSegmentationTypeCrudMessageService(IKafkaContextProvider contextProvider)
        {
            _segmentationTypeMessageService = new SegmentationTypeCrudMessageService(
                KafkaProducerFactoryInstance.Get(),
                contextProvider,
                new KLogger(nameof(SegmentationTypeCrudMessageService)));
        }
        
        public static void InitUserSegmentCrudMessageService(IKafkaContextProvider contextProvider)
        {
            _userSegmentMessageService = new UserSegmentCrudMessageService(
                KafkaProducerFactoryInstance.Get(),
                contextProvider,
                new KLogger(nameof(UserSegmentCrudMessageService)));
        }
    }
}