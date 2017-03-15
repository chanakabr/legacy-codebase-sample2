using AdapterControllers;
using ApiLogic;
using APILogic;
using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.BulkExport;
using ApiObjects.Catalog;
using ApiObjects.CDNAdapter;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.Rules;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using ApiObjects.TimeShiftedTv;
using CachingHelpers;
using CachingProvider.LayeredCache;
using Core.Api.Managers;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using EpgBL;
using KLogMonitor;
using QueueWrapper;
using QueueWrapper.Queues.QueueObjects;
using ScheduledTasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Tvinci.Core.DAL;
using TvinciImporter;
using TVinciShared;

namespace Core.Api
{
    public class api
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts
        private const string CONFLICTED_PARAMS = "Conflicted params";
        private const string NO_PARAMS_TO_INSERT = "no params to insert";
        private const string NO_PARAMS_TO_DELETE = "no params to delete";
        private const string NO_OSS_ADAPTER_TO_INSERT = "No OSS adapter to insert";
        private const string NO_OSS_ADAPTER_TO_UPDATE = "No OSS adapter to update";
        private const string NAME_REQUIRED = "Name must have a value";
        private const string OSS_ADAPTER_SHARED_SECRET_REQUIRED = "Shared secret must have a value";
        private const string EXTERNAL_IDENTIFIER_REQUIRED = "External identifier must have a value";
        private const string ERROR_EXT_ID_ALREADY_IN_USE = "External identifier must be unique";
        private const string OSS_ADAPTER_ID_REQUIRED = "OSS adapter identifier is required";
        private const string OSS_ADAPTER_NOT_EXIST = "OSS adapter not exist";
        private const string ACTION_IS_NOT_ALLOWED = "Action is not allowed";
        private const string ADAPTER_URL_REQUIRED = "Adapter url must have a value";
        private const string ADAPTER_ID_REQUIRED = "Adapter identifier is required";
        private const string ADAPTER_NOT_EXIST = "Adapter not exist";
        private const string NO_ADAPTER_TO_INSERT = "No adapter to insert";
        private const string NO_ADAPTER_TO_UPDATE = "No adapter to update";

        private const string NO_RECOMMENDATION_ENGINE_TO_INSERT = "No recommendation engine to insert";
        private const string NO_RECOMMENDATION_ENGINE_TO_UPDATE = "No recommendation engine to update";
        private const string RECOMMENDATION_ENGINE_NOT_EXIST = "Recommendation engine not exist";
        private const string RECOMMENDATION_ENGINE_ID_REQUIRED = "Recommendation engine identifier is required";

        private const string NO_EXTERNAL_CHANNEL_TO_INSERT = "No external channel to insert";
        private const string NO_EXTERNAL_CHANNEL_TO_UPDTAE = "No external channel to update";
        private const string EXTERNAL_CHANNEL_NOT_EXIST = "External channel not exist";
        private const string EXTERNAL_CHANNEL_ID_REQUIRED = "External channelidentifier is required";

        private const string ALIAS_REQUIRED = "Alias is required";
        private const string ALIAS_ALREADY_EXISTS = "Alias must be unique";
        private const string SYSTEM_NAME_REQUIRED = "SystemName is required";
        private const string SYSTEM_NAME_ALREADY_EXISTS = "SystemName must be unique";
        private const string EXPORT_TASK_NOT_FOUND = "Export task not found";
        private const string EXPORT_NOTIFICATION_URL_REQUIRED = "Notification URL is required";
        private const string EXPORT_FREQUENCY_MIN_VALUE_FORMAT = "Frequency cannot be smaller than configured minimum value ({0} minutes)";
        private const string ROUTING_KEY_PROCESS_EXPORT = "PROCESS_EXPORT\\{0}";
        private const string PURCHASE_SETTINGS_TYPE_INVALID = "Purchase settings type invalid";

        protected const string ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION = "PROCESS_RENEW_SUBSCRIPTION\\{0}";
        protected const string ROUTING_KEY_CHECK_PENDING_TRANSACTION = "PROCESS_CHECK_PENDING_TRANSACTION\\{0}";        

        protected const string QUEUE_ASSEMBLY_NAME = "QueueWrapper";

        private const string GROUP_CDN_SETTINGS_LAYERED_CACHE_CONFIG_NAME = "groupCDNSettings";
        private const string CDN_ADAPTER_LAYERED_CACHE_CONFIG_NAME = "cdnAdapter";
        private const string GROUP_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "groupParentalRules";
        private const string USER_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "userParentalRules";
        private const string MEDIA_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "mediaParentalRules";

        private const string ACTION_RULE_TASK = "distributed_tasks.process_action_rule";
        private const double MAX_SERVER_TIME_DIF = 5;
        private const double HANDLE_ASSET_LIFE_CYCLE_RULE_SCHEDULED_TASKS_INTERVAL_SEC = 21600; // 6 hours
        private const string ROUTING_KEY_RECORDINGS_ASSET_LIFE_CYCLE_RULE = "PROCESS_ACTION_RULE";

        #endregion

        protected api() { }

        protected api(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        static public UserIMRequestObject GetTvinciGUID(InitializationObject oInitObj, Int32 nGroupID)
        {
            Int32 nWatcherID = 0;
            GetStartValues(oInitObj.m_oUserIMRequestObject.m_sSiteGuid, ref oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                ref nWatcherID, nGroupID, true, oInitObj.m_oUserIMRequestObject.m_sUserAgent, oInitObj.m_oUserIMRequestObject.m_sUserIP);
            return oInitObj.m_oUserIMRequestObject;

        }

        static public List<GroupOperator> GetOperators(int nGroupID, List<int> operatorIds)
        {
            try
            {
                // Return all group operators by ID
                List<GroupOperator> ret = new List<GroupOperator>();


                DataSet ds = DAL.ApiDAL.Get_Operators_Info(nGroupID, operatorIds);
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        GroupOperator go;

                        foreach (DataRow dataRow in ds.Tables[0].Rows)
                        {
                            go = new GroupOperator();
                            go.ID = ODBCWrapper.Utils.GetIntSafeVal(dataRow["ID"]);
                            go.Name = ODBCWrapper.Utils.GetSafeStr(dataRow["Name"]);
                            go.SubGroupID = ODBCWrapper.Utils.GetIntSafeVal(dataRow["Sub_Group_ID"]);
                            go.Type = (eOperatorType)ODBCWrapper.Utils.GetIntSafeVal(dataRow["type"]);
                            go.CoGuid = ODBCWrapper.Utils.GetSafeStr(dataRow["Client_Id"]);

                            go.UIData = new UIData();
                            go.UIData.picURL = ODBCWrapper.Utils.GetSafeStr(dataRow["Pic_URL"]);
                            go.UIData.ColorCode = ODBCWrapper.Utils.GetSafeStr(dataRow["Color_Code"]);
                            go.GroupUserName = ODBCWrapper.Utils.GetSafeStr(dataRow["USERNAME"]);
                            go.GroupPassword = ODBCWrapper.Utils.GetSafeStr(dataRow["PASSWORD"]);
                            go.AboutUs = ODBCWrapper.Utils.GetSafeStr(dataRow["About_Us"]);
                            go.ContactUs = ODBCWrapper.Utils.GetSafeStr(dataRow["Contact_Us"]);

                            go.Groups_operators_menus = new List<ApiObjects.KeyValuePair>();
                            foreach (DataRow groupsOperatorsMenusDRdataRow in ds.Tables[1].Rows)
                            {
                                string PID = ODBCWrapper.Utils.GetSafeStr(groupsOperatorsMenusDRdataRow["PlatformID"]);
                                string MID = ODBCWrapper.Utils.GetSafeStr(groupsOperatorsMenusDRdataRow["TVPMenuID"]);
                                go.Groups_operators_menus.Add(new ApiObjects.KeyValuePair(PID, MID));
                            }

                            ret.Add(go);
                        }
                    }
                }
                return ret;
            }
            catch
            {
                // TODO print to logger
                return null;
            }
        }

        static public bool InitializeGroupNPlayer(ref ApiObjects.InitializationObject initObj)
        {
            try
            {
                if (initObj.m_oPlayerIMRequestObject != null)
                    initObj.m_nGroupID = PageUtils.GetGroupByUNPass(initObj.m_oPlayerIMRequestObject.m_sPalyerID, initObj.m_oPlayerIMRequestObject.m_sPlayerKey, ref initObj.m_nPlayerID);
                else
                    return false;

                if (initObj.m_nGroupID == 0)
                    return false;

                api.GetStartValues(initObj.m_oUserIMRequestObject.m_sSiteGuid, ref initObj.m_oUserIMRequestObject.m_sTvinciGuid,
                    ref initObj.m_nWatcherID, initObj.m_nGroupID, false, initObj.m_oUserIMRequestObject.m_sUserAgent, initObj.m_oUserIMRequestObject.m_sUserIP);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static public ApiObjects.MediaInfoObject TVAPI_GetMediaInfo(InitializationObject oInitObj, Int32 nGroupID, Int32 nMediaID)
        {
            XmlDocument doc = null;
            ApiObjects.MediaInfoObject theInfo = new MediaInfoObject();
            ApiObjects.MediaStatistics theStat = null;
            ApiObjects.MediaPersonalStatistics thePersonalStat = null;
            ApiObjects.PicObject[] thePics = null;

            TVinciShared.ProtocolsFuncs.MediaInfoProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                !oInitObj.m_oExtraRequestObject.m_bNoCache, false, ref thePics, ref theInfo, ref thePersonalStat, ref theStat, nMediaID);
            return theInfo;
        }

        static public ApiObjects.MediaInfoStructObject GetMediaStructure(InitializationObject oInitObj, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ApiObjects.MediaInfoStructObject theInfoStruct = new MediaInfoStructObject();
            TVinciShared.ProtocolsFuncs.MediaStructureProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                ref theInfoStruct);
            return theInfoStruct;
        }

        static public ApiObjects.ChannelObject SearchMedia(InitializationObject oInitObj, SearchDefinitionObject theSearchCriteria, MediaInfoStructObject theInfoStruct, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = 0;
            if (oInitObj == null)
            {
                nDeviceID = 0;
            }
            else
            {
                nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            }
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.SearchMediaProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                !oInitObj.m_oExtraRequestObject.m_bNoCache, false, false, nCountryID, ref oInitObj,
                ref theSearchCriteria, ref theInfoStruct, ref ret, nDeviceID);

            return ret;
        }

        static public ApiObjects.RateResponseObject RateMedia(InitializationObject oInitObj, Int32 nMediaID, Int32 nRateVal, Int32 nGroupID)
        {
            XmlDocument doc = null;
            RateResponseObject ret = new RateResponseObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.RatingProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                ref oInitObj, ref ret, nMediaID, nRateVal);

            return ret;
        }

        static public ApiObjects.ChannelObject SearchRelated(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nMediaID, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.SearchRelatedProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, nCountryID, ref oInitObj, ref theInfoStruct, ref ret, ref thePageDef, nMediaID, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject NowPlaying(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.NowPlayingProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, nCountryID, ref oInitObj, ref theInfoStruct, ref ret, ref thePageDef, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject PersonalRateList(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nGroupID, Int32 nMinRate, Int32 nMaxRate)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.PersonalRatedProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, nCountryID, ref oInitObj, ref theInfoStruct, ref ret, ref thePageDef, nMinRate, nMaxRate, nDeviceID);

            return ret;
        }

        static public ApiObjects.TagResponseObject[] TagValues(InitializationObject oInitObj, ApiObjects.TagRequestObject[] sTagTypes, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ApiObjects.TagResponseObject[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.TagValuesProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                ref oInitObj, ref sTagTypes, ref ret, nCountryID, nDeviceID);
            return ret;
        }

        static public ApiObjects.OneTimeObject OneTimeLink(InitializationObject oInitObj, Int32 nMediaID, Int32 nMediaFileID,
            string sBaseLink, string sCDNImpleType, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ApiObjects.OneTimeObject ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.MediaOneTimeLinkProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                sCDNImpleType, nMediaID, nMediaFileID, sBaseLink, oInitObj.m_oPlayerIMRequestObject.m_sPalyerID, ref ret);
            return ret;
        }

        /*Get the Lucene URL from DB , else from config file*/
        private static string GetLuceneUrl(int nGroupID)
        {
            string sLuceneURL = GetWSURL("LUCENE_WCF");
            return sLuceneURL;
        }

        static public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        static public List<string> GetAutoCompleteList(RequestObj request, int groupID)
        {
            log.Debug("AutoComplete - Start AutoComplete");
            List<string> retVal = new List<string>();

            MediaAutoCompleteRequest autoCompleteRequest = new MediaAutoCompleteRequest();
            autoCompleteRequest.m_nGroupID = groupID;

            if (!string.IsNullOrEmpty(request.m_sLanguage))
            {
                try
                {
                    autoCompleteRequest.m_oFilter = new Filter();
                    autoCompleteRequest.m_oFilter.m_nLanguage = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("lu_languages", "ID", "CODE3", "=", request.m_sLanguage).ToString());
                }
                catch
                {

                }
            }

            autoCompleteRequest.m_sPrefix = request.m_InfoStruct.m_sPrefix;
            autoCompleteRequest.m_lMetas = request.m_InfoStruct.m_Metas;
            autoCompleteRequest.m_lTags = request.m_InfoStruct.m_Tags;

            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = GetWSURL("CatalogSignatureKey");
            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            autoCompleteRequest.m_sSignature = sSignature;
            autoCompleteRequest.m_sSignString = sSignString;

            MediaAutoCompleteResponse response = autoCompleteRequest.GetResponse(autoCompleteRequest) as MediaAutoCompleteResponse;

            if (response != null && response.lResults != null)
            {
                retVal = response.lResults.ToList();
            }

            return retVal;
        }

        static public ApiObjects.UserComment[] MediaComments(InitializationObject oInitObj, Int32 nMediaID, Int32 nGroupID, string sCommentType)
        {
            XmlDocument doc = null;
            ApiObjects.UserComment[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.CommentsListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                ref oInitObj, nMediaID, sCommentType, ref ret, oInitObj.m_oLanguageRequestObject.m_sFullName);
            return ret;
        }

        static public ApiObjects.UserComment[] UserSaveComment(InitializationObject oInitObj, ApiObjects.UserComment theUserComment,
            bool bAutoPublish, Int32 nGroupID, string sCommentType)
        {
            XmlDocument doc = null;
            ApiObjects.UserComment[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.SaveCommentsProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                ref oInitObj, theUserComment, bAutoPublish, sCommentType, ref ret, oInitObj.m_oLanguageRequestObject.m_sFullName);
            return ret;
        }


        static public bool SetMediaDuration(InitializationObject oInitObj, Int32 nMediaFileID, Int32 nDurationInSec, Int32 nGroupID)
        {
            XmlDocument doc = null;
            bool ret = false;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.SetDurationProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID,
                oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                ref oInitObj, nMediaFileID, nDurationInSec, ref ret);
            return ret;
        }

        static public bool MediaMark(InitializationObject oInitObj, string sAction, Int32 nLocationSec,
            ref MediaFileObject theMediaFile, Int32 nGroupID)
        {
            XmlDocument doc = null;
            bool ret = true;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.MediaMark(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, nCountryID,
                oInitObj.m_nPlayerID, ref oInitObj, sAction, nLocationSec, ref theMediaFile);
            return ret;
        }

        static public bool Hit(InitializationObject oInitObj, Int32 nLocationInVideoSec,
            ref MediaFileObject theMediaFile, Int32 nGroupID)
        {
            XmlDocument doc = null;
            bool ret = true;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.HitProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, nCountryID,
                oInitObj.m_nPlayerID, ref oInitObj, nLocationInVideoSec, ref theMediaFile);
            return ret;
        }

        static public ApiObjects.ChannelObject UserLastWatched(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.PersonalLastWatchedProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, nCountryID, ref oInitObj, ref thePageDef, ref theInfoStruct, ref ret, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject PeopleWhoWatchedAlsoWatched(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nGroupID, Int32 nMediaID, Int32 nMediaFileID)
        {
            XmlDocument doc = null;
            ChannelObject ret = new ChannelObject();
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.PWWAWProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, nCountryID, ref oInitObj, ref theInfoStruct, ref ret, nMediaID, nMediaFileID, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject[] ChannelsMedia(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, ChannelRequestObject[] theChannelsRequestObj, Int32 nGroupID)
        {
            XmlDocument doc = null;
            ChannelObject[] ret = new ChannelObject[theChannelsRequestObj.Length];
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.ChannelMediaProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                !oInitObj.m_oExtraRequestObject.m_bNoCache, false, nCountryID,
                ref oInitObj, ref theInfoStruct, ref theChannelsRequestObj,
                ref ret, nDeviceID);

            return ret;
        }

        static public ApiObjects.CategoryObject[] CategoriesTree(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nGroupID, Int32 nCategoryID, bool bWithChannels)
        {
            XmlDocument doc = null;
            CategoryObject[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.CategoriesListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, !oInitObj.m_oExtraRequestObject.m_bNoCache, ref oInitObj, ref theInfoStruct, nCategoryID, bWithChannels,
                ref ret, nCountryID, nDeviceID, true);


            return ret;
        }

        static public ApiObjects.ChannelObject[] CategoryChannels(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, Int32 nGroupID, Int32 nCategoryID)
        {
            XmlDocument doc = null;
            ChannelObject[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.ChannelsListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName, oInitObj.m_nPlayerID,
                false, !oInitObj.m_oExtraRequestObject.m_bNoCache, ref oInitObj, ref theInfoStruct, nCategoryID, ref ret, nCountryID, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject[] UserChannels(InitializationObject oInitObj,
            MediaInfoStructObject theInfoStruct, Int32 nGroupID, string sLang)
        {
            XmlDocument doc = null;
            ChannelObject[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.WatcherChannelsListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                false, ref oInitObj, ref theInfoStruct, ref ret, sLang, nCountryID, nDeviceID);

            return ret;
        }

        static public ApiObjects.ChannelObject[] DeleteUserChannel(InitializationObject oInitObj, MediaInfoStructObject theInfoStruct,
            Int32 nGroupID, Int32 nChannelID)
        {
            XmlDocument doc = null;
            ChannelObject[] ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.DeletePlayListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID, oInitObj.m_oLanguageRequestObject.m_sFullName,
                false, ref oInitObj, ref theInfoStruct, ref ret, nChannelID, nCountryID, nDeviceID);

            return ret;
        }

        static public ApiObjects.GenericWriteResponse saveUserPlaylist(InitializationObject oInitObj, Int32 nGroupID, Int32[] nMediaIDs, string sChannelTitle, bool bRewrite)
        {
            XmlDocument doc = null;
            GenericWriteResponse ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.SavePlayListProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID, oInitObj.m_oLanguageRequestObject.m_sFullName,
                ref oInitObj, nMediaIDs, sChannelTitle, bRewrite, ref ret);
            return ret;
        }

        static public ApiObjects.GenericWriteResponse SendMediaByEmail(InitializationObject oInitObj, Int32 nGroupID, Int32 nMediaID,
            string sFromEmail, string sToEmail, string sRecieverName, string sSenderName, string sContent)
        {
            XmlDocument doc = null;
            GenericWriteResponse ret = null;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            TVinciShared.ProtocolsFuncs.SentToFriendProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid,
                "", "", oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_nPlayerID,
                ref oInitObj, nMediaID, sFromEmail, sToEmail, sRecieverName, sSenderName, sContent, ref ret);
            return ret;
        }

        static public bool SetTokenData(ref InitializationObject oInitObj, ref Int32 nCountryID, ref Int32 nDeviceID, Int32 nGroupID)
        {
            string sIP = PageUtils.GetCallerIP();
            Int32 nCountryTokenID = 0;
            string sLang = "";
            Int32 nDeviceTokenID = 0;
            bool bAdmin = false;
            bool bWithCache = false;
            bool bAdminToken = false;
            if (String.IsNullOrEmpty(oInitObj.m_sAdminToken) == false)
                bAdminToken = TVinciShared.ProtocolsFuncs.GetAdminTokenValues(oInitObj.m_sAdminToken, sIP,
                    ref nCountryTokenID, ref sLang, ref nDeviceTokenID, nGroupID, ref bAdmin, ref bWithCache);
            if (bAdminToken == true)
            {
                nCountryID = nCountryTokenID;
                oInitObj.m_oLanguageRequestObject = new LanguageRequestObject();
                oInitObj.m_oLanguageRequestObject.Initialize(sLang);
                nDeviceID = nDeviceTokenID;
            }
            return bAdminToken;
        }

        static public ChannelObject SingleMedia(InitializationObject oInitObj, Int32 nGroupID, Int32[] nMediaIDs, MediaInfoStructObject theInfoStruct)
        {
            ChannelObject ret = new ChannelObject();
            XmlDocument doc = null;
            ApiObjects.MediaObject[] theMediaObjs = null;
            ApiObjects.PlayListSchema thePlayListSchema = new PlayListSchema(); ;
            Int32 nCountryID = PageUtils.GetIPCountry2(oInitObj.m_oUserIMRequestObject.m_sUserIP);
            Int32 nDeviceID = ProtocolsFuncs.GetDeviceIdFromName(oInitObj.m_sDevice, nGroupID);
            SetTokenData(ref oInitObj, ref nCountryID, ref nDeviceID, nGroupID);
            TVinciShared.ProtocolsFuncs.SingleMediaProtocol(ref doc, nGroupID, oInitObj.m_oUserIMRequestObject.m_sTvinciGuid, "", "",
                oInitObj.m_oUserIMRequestObject.m_sSiteGuid, oInitObj.m_nWatcherID, oInitObj.m_oLanguageRequestObject.m_sFullName,
                oInitObj.m_nPlayerID, !oInitObj.m_oExtraRequestObject.m_bNoCache, false, ref oInitObj, nMediaIDs,
                ref theInfoStruct, ref theMediaObjs, ref thePlayListSchema, nCountryID, nDeviceID);
            ret.Initialize(thePlayListSchema, theMediaObjs, nMediaIDs.Length, null, "Container", "", "", 0);
            return ret;
        }

        static public void GetStartValues(string sSiteGUID, ref string sTVinciGUID, ref Int32 nWatcherID, Int32 nGroupID, bool bCreate, string sUserAgent, string sCallerIP)
        {
            if (sSiteGUID.Trim() != "")
            {
                DataTable dt = DAL.ApiDAL.Get_StartValues(nGroupID, sSiteGUID, 1);// 1 = by sSiteGUID
                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        nWatcherID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "WATCHER_ID");
                        sTVinciGUID = APILogic.Utils.GetSafeStr(dt.Rows[0], "TVINCI_GUID");
                    }
                    else
                    {
                        if (sTVinciGUID == "")
                            sTVinciGUID = System.Guid.NewGuid().ToString();
                        if (bCreate == true)
                            nWatcherID = ProtocolsFuncs.AddWatcherField(sTVinciGUID, sUserAgent, sSiteGUID, nGroupID, sCallerIP, false);
                    }
                }
            }
            else
            {
                if (sTVinciGUID != "")
                {
                    DataTable dt = DAL.ApiDAL.Get_StartValues(0, sTVinciGUID, 2);// 1 = by sTVinciGUID
                    if (dt != null)
                    {
                        if (dt.Rows != null && dt.Rows.Count > 0)
                        {
                            nWatcherID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "ID");
                        }
                    }

                    if (nWatcherID == 0)
                    {
                        if (bCreate == true)
                            nWatcherID = ProtocolsFuncs.AddWatcherField(sTVinciGUID, sUserAgent, sSiteGUID, nGroupID, sCallerIP, false);
                    }
                }
                else
                {
                    if (bCreate == true)
                    {
                        sTVinciGUID = System.Guid.NewGuid().ToString();
                        nWatcherID = ProtocolsFuncs.AddWatcherField(sTVinciGUID, sUserAgent, sSiteGUID, nGroupID, sCallerIP, false);
                    }
                }
            }
        }

        static public Int32 GetMediaFileTypeID(Int32 nMediaFileID, Int32 nGroupID)
        {
            Int32 nMediaFileType = 0;

            //try to get from cache 
            string key = string.Format("{0}_MediaFileTypeID_{1}", eWSModules.API.ToString(), nMediaFileID);
            bool bInCache = ApiCache.GetItem<int>(key, out nMediaFileType);

            if (!bInCache || nMediaFileType == 0)
            {
                DataTable dt = DAL.ApiDAL.Get_MediaFileTypeID(nMediaFileID, nGroupID);
                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        nMediaFileType = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "virtual_name");

                        ApiCache.AddItem(key, nMediaFileType);
                    }
                }
            }

            return nMediaFileType;
        }

        static public bool GetAdminTokenValues(string sIP, string sToken, Int32 nGroupID, ref string sCountryCd2, ref string sLanguageFullName, ref string sDeviceName, ref ApiObjects.UserStatus theUserStatus)
        {
            bool bRet = false;
            DataTable dt = DAL.ApiDAL.Get_AdminTokenValues(sIP, nGroupID);
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {

                    sDeviceName = APILogic.Utils.GetSafeStr(dt.Rows[0], "DEVICE_NAME");
                    sLanguageFullName = APILogic.Utils.GetSafeStr(dt.Rows[0], "LANGUAGE_NAME");
                    sCountryCd2 = APILogic.Utils.GetSafeStr(dt.Rows[0], "COUNTRY_NAME");
                    Int32 nUserStatus = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "USER_STATUS_ID");
                    theUserStatus = (UserStatus)nUserStatus;
                    bRet = true;
                }
            }
            return bRet;
        }


        static public List<int> GetChannelsMediaIDs(Int32[] nChannels, Int32[] nFileTypeIDs, bool bWithCache, Int32 nGroupID, string sDevice, bool activeAssets, bool useStartDate)
        {
            List<int> nMedias = new List<int>();

            try
            {
                string sSignString = Guid.NewGuid().ToString();
                string sSignatureString = GetWSURL("CatalogSignatureKey");

                string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);
                for (int i = 0; i < nChannels.Length; i++)
                {
                    try
                    {
                        ChannelRequestMultiFiltering channelRequest = new ChannelRequestMultiFiltering();
                        channelRequest.m_nChannelID = nChannels[i];
                        channelRequest.m_nGroupID = nGroupID;
                        channelRequest.m_oFilter = new Filter();
                        channelRequest.m_oFilter.m_bOnlyActiveMedia = activeAssets;
                        channelRequest.m_oFilter.m_bUseStartDate = useStartDate;
                        channelRequest.m_nPageSize = 0;
                        channelRequest.m_nPageIndex = 0;
                        channelRequest.m_oFilter.m_sDeviceId = sDevice;
                        channelRequest.m_oFilter.m_bOnlyActiveMedia = false;
                        channelRequest.m_bIgnoreDeviceRuleID = true;
                        channelRequest.m_sSignString = sSignString;
                        channelRequest.m_sSignature = sSignature;
                        BaseResponse response = channelRequest.GetResponse(channelRequest);
                        SearchResult[] medias = ((ChannelResponse)response).m_nMedias.ToArray();
                        int[] mediaIdsReturned = null;

                        List<int> lIdsReturned = APILogic.Utils.ConvertMediaResultObjectIDsToIntArray(medias);
                        if (lIdsReturned != null)
                        {
                            mediaIdsReturned = lIdsReturned.ToArray();
                        }

                        if (mediaIdsReturned != null && mediaIdsReturned.Length > 0)
                        {
                            nMedias.AddRange(mediaIdsReturned);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error - " + string.Format("channel:{0}, msg:{1}", nChannels[i], ex.Message), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Configuration Reading - Couldn't read values from configuration ", ex);
            }


            return nMedias.Distinct().ToList();
        }
        static public Int32[] GetChannelsMediaIDs(Int32[] nChannels, Int32[] nFileTypeIDs, bool bWithCache, Int32 nGroupID, string sDevice)
        {

            List<int> nMedias = new List<int>();
            nMedias = GetChannelsMediaIDs(nChannels, nFileTypeIDs, bWithCache, nGroupID, sDevice, true, true);
            return nMedias.Distinct().ToArray();
        }

        static public Int32[] GetChannelsMediaIDs(Int32[] nChannels, Int32[] nFileTypeIDs, bool bWithCache, Int32 nGroupID, Int32 nDeviceID)
        {

            // TO DO add call to catalog !!!!! SubsContainMedia

            List<int> nMedias = new List<int>();

            //APILogic.Lucene.Service client = new APILogic.Lucene.Service();

            //string sWSURL = GetLuceneUrl(nGroupID);
            //if (!String.IsNullOrEmpty(sWSURL))
            //{
            //    client.Url = sWSURL; // add parent group id to url
            //}

            //for (int i = 0; i < nChannels.Length; i++)
            //{
            //    APILogic.Lucene.SearchResultsObj medias = client.GetChannelMedias(nGroupID, nChannels[i], 0, null, true, true, null, 0, 0, 0);

            //    if (medias == null || medias.m_resultIDs == null || medias.m_resultIDs.Length == 0)
            //        continue;

            //    nMedias.AddRange(medias.m_resultIDs);
            //}

            //if (nMedias == null || nMedias.Count == 0)
            //    return null;

            return nMedias.Distinct().ToArray();
        }

        static public FileTypeContainer[] GetAvailableFileTypes(Int32 nGroupID)
        {
            FileTypeContainer[] ret = null;

            List<FileTypeContainer> lFileTypeContainer;
            string key = string.Format("{0}_GetAvailableFileTypes_{1}", eWSModules.API.ToString(), nGroupID);
            bool bInCache = ApiCache.GetItem<List<FileTypeContainer>>(key, out lFileTypeContainer);

            if (!bInCache || lFileTypeContainer == null)
            {
                DataTable dt = DAL.ApiDAL.Get_AvailableFileTypes(nGroupID);
                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        lFileTypeContainer = new List<FileTypeContainer>();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            int nID = APILogic.Utils.GetIntSafeVal(dt.Rows[i], "MEDIA_TYPE_ID");
                            if (nID > 0)
                            {
                                FileTypeContainer f = new FileTypeContainer();

                                f.Initialize(dt.Rows[i]["description"].ToString(), APILogic.Utils.GetIntSafeVal(dt.Rows[i], "ID"));
                                lFileTypeContainer.Add(f);
                            }
                        }
                        if (lFileTypeContainer != null && lFileTypeContainer.Count > 0)
                        {
                            ApiCache.AddItem(key, lFileTypeContainer);
                        }
                    }
                }
            }

            if (lFileTypeContainer != null)
            {
                ret = lFileTypeContainer.ToArray();
            }

            return ret;
        }

        static public Int32[] GetChannelMediaIDs(Int32 nChannelID, Int32[] nFileTypeIDs, bool bWithCache, Int32 nGroupID,
            Int32 nLangID, bool bIsLangMain, Int32 nCountryID, Int32 nDeviceID)
        {
            Int32 nCount = 0;
            TVinciShared.Channel c = new TVinciShared.Channel(nChannelID, bWithCache, nLangID, bIsLangMain, nCountryID, nDeviceID);
            c.SetGroupID(nGroupID);
            System.Data.DataTable d = c.GetChannelMediaDT(nFileTypeIDs);
            if (d == null)
                return null;
            nCount += d.DefaultView.Count;
            if (nCount > 0)
            {
                Int32[] ret = new Int32[nCount];
                Int32 nIndex = 0;
                if (d == null)
                    return null;
                for (int j = 0; j < nCount; j++)
                {
                    Int32 nMediaID = int.Parse(d.DefaultView[j].Row["id"].ToString());
                    if (Array.IndexOf(ret, nMediaID) == -1)
                    {
                        ret[nIndex] = nMediaID;
                        nIndex++;
                    }
                }
                if (nIndex < nCount)
                {
                    Int32[] tmp1 = new Int32[nIndex];
                    Array.Copy(ret, tmp1, nIndex);
                    ret = tmp1;
                }
                return ret;
            }
            return null;
        }

        static public bool DoesMediaBelongToChannels(Int32[] nChannels, Int32[] nFileTypeIDs, Int32 nMediaID, bool bWithCache, Int32 nGroupID, Int32 nDeviceID)
        {
            try
            {
                ChannelsContainingMediaRequest ccm = new ChannelsContainingMediaRequest();
                ccm.m_nGroupID = nGroupID;
                ccm.m_nMediaID = nMediaID;
                ccm.m_lChannles = nChannels.ToList();
                string sSignString = Guid.NewGuid().ToString();
                string sSignatureString = GetWSURL("CatalogSignatureKey");
                string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);
                ccm.m_sSignString = sSignString;
                ccm.m_sSignature = sSignature;
                ccm.m_oFilter.m_sDeviceId = nDeviceID.ToString();

                ChannelsContainingMediaResponse response = (ChannelsContainingMediaResponse)ccm.GetResponse(ccm);

                if (response == null || response.m_lChannellList == null || response.m_lChannellList.Count() == 0)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("group:{0}, msg:{1}", nGroupID, ex.Message), ex);
                return false;
            }
        }

        static public bool DoesMediaBelongToChannels(Int32[] nChannels, Int32[] nFileTypeIDs, Int32 nMediaID, bool bWithCache, Int32 nGroupID, string sDevice)
        {
            try
            {
                ChannelsContainingMediaRequest ccm = new ChannelsContainingMediaRequest();
                ccm.m_nGroupID = nGroupID;
                ccm.m_nMediaID = nMediaID;
                ccm.m_lChannles = nChannels.ToList();
                string sSignString = Guid.NewGuid().ToString();
                string sSignatureString = GetWSURL("CatalogSignatureKey");
                string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);
                ccm.m_sSignString = sSignString;
                ccm.m_sSignature = sSignature;

                ccm.m_oFilter = new Filter();
                ccm.m_oFilter.m_sDeviceId = sDevice;

                ChannelsContainingMediaResponse response = (ChannelsContainingMediaResponse)ccm.GetResponse(ccm);

                if (response == null)
                    return false;
                else if (response.m_lChannellList == null || response.m_lChannellList.Count() == 0)
                    return false;
                else
                    return true;

            }
            catch
            {
                return false;
            }

        }

        static public bool ValidateBaseLink(Int32 nMediaFileID, string sBaseLink, Int32 nGroupID)
        {
            bool bRet = false;
            string sDBBaseLink = "";

            string key = string.Format("{0}_ValidateBaseLink_{1}_{2}", eWSModules.API.ToString(), nMediaFileID, sBaseLink);
            string sCacheBaseLink = string.Empty;
            bool bCache = ApiCache.GetItem<string>(key, out sCacheBaseLink);
            if (bCache)
            {
                bool.TryParse(sCacheBaseLink, out bRet);
            }
            else
            {
                DataTable dt = DAL.ApiDAL.Get_DataByTableID(nMediaFileID.ToString(), "media_files", "STREAMING_CODE");
                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        sDBBaseLink = APILogic.Utils.GetSafeStr(dt.Rows[0], "STREAMING_CODE");
                        if (sDBBaseLink != string.Empty)
                            sDBBaseLink = sDBBaseLink.ToLower().Trim();

                        if (sBaseLink.Trim().ToLower().EndsWith(sDBBaseLink) == true && sDBBaseLink != "")
                        {
                            bRet = true;
                        }
                        else
                        {
                            if (System.Web.HttpUtility.HtmlDecode(sBaseLink.Trim().ToLower()).EndsWith(System.Web.HttpUtility.HtmlDecode(sDBBaseLink)) == true && sDBBaseLink != "")
                                bRet = true;
                            else
                            {
                                if (System.Web.HttpUtility.UrlDecode(sBaseLink.Trim().ToLower()).Trim().EndsWith(System.Web.HttpUtility.UrlDecode(sDBBaseLink).Trim()) == true && sDBBaseLink != "")
                                    bRet = true;
                            }
                        }
                        ApiCache.AddItem(key, bRet.ToString());
                    }
                }
            }
            return bRet;
        }

        static public MeidaMaper[] MapMediaFiles(Int32[] mediaFileIds, Int32 groupID)
        {
            List<MeidaMaper> result = new List<MeidaMaper>();

            if (mediaFileIds == null || mediaFileIds.Length == 0)
            {
                return null;
            }

            //string.Format("api_MapMediaFiles_{1}", string.Empty);
            string key = string.Empty;
            List<string> cacheKeys = new List<string>();
            Dictionary<string, int> keyToFileId = new Dictionary<string, int>();
            List<int> mediaFilesToGet = new List<int>();

            #region create list of keys

            foreach (int fileId in mediaFileIds)
            {
                key = string.Format("{0}_MapMediaFiles_{1}", eWSModules.API.ToString(), fileId);

                if (!keyToFileId.ContainsKey(key))
                {
                    cacheKeys.Add(key);
                    keyToFileId.Add(key, fileId);
                }
            }

            #endregion

            // get values from cache
            IDictionary<string, object> cacheMapper = ApiCache.GetValues(cacheKeys);

            #region go over the results return from cache call

            if (cacheMapper != null)
            {
                foreach (KeyValuePair<string, object> kvp in cacheMapper)
                {
                    // build list of keys that not exists in cache
                    if (kvp.Value == null)
                    {
                        if (keyToFileId.ContainsKey(kvp.Key))
                        {
                            mediaFilesToGet.Add(keyToFileId[kvp.Key]);
                        }
                    }
                    else
                    {
                        MeidaMaper mediaMapper = (MeidaMaper)kvp.Value;
                        if (mediaMapper != null)
                            result.Add(mediaMapper);
                    }
                }
            }
            #endregion

            #region  Get all values from DB , after that fill the MeidaMaper List and insert the values into cache !!

            if (mediaFilesToGet != null && mediaFilesToGet.Count > 0)
            {
                DataTable dt = DAL.ApiDAL.Get_MapMediaFiles(mediaFilesToGet);

                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            Int32 mediaFileId = APILogic.Utils.GetIntSafeVal(dt.Rows[i], "id");
                            Int32 mediaId = APILogic.Utils.GetIntSafeVal(dt.Rows[i], "media_id");
                            string productCode = APILogic.Utils.GetSafeStr(dt.Rows[i], "product_code");
                            MeidaMaper mediaMapper = new MeidaMaper();
                            mediaMapper.Initialize(mediaFileId, mediaId, productCode);
                            result.Add(mediaMapper);

                            // build key
                            key = string.Format("{0}_MapMediaFiles_{1}", eWSModules.API.ToString(), mediaFileId);

                            // add value to cache 
                            ApiCache.AddItem(key, mediaMapper);
                        }
                    }
                }
            }
            #endregion

            return result.ToArray();
        }

        static public MeidaMaper[] MapMediaFilesST(string sSeperatedMediaFileIDs, Int32 nGroupID)
        {

            string[] sToSep = { ";" };
            string[] sSep = sSeperatedMediaFileIDs.Split(sToSep, StringSplitOptions.RemoveEmptyEntries);
            if (sSep.Length == 0)
                return null;

            List<int> nMediaFileIDs = new List<int>(sSeperatedMediaFileIDs.Split(';').Select(int.Parse));
            return MapMediaFiles(nMediaFileIDs.ToArray<int>(), nGroupID);
        }

        static public GroupInfo[] GetSubGroupsTree(string sGroupName, Int32 nGroupID)
        {
            List<GroupInfo> lGroup = null;
            string key = string.Format("{0}_GetSubGroupTree_{1}", eWSModules.API.ToString(), sGroupName);
            if (ApiCache.GetList<GroupInfo>(sGroupName, out lGroup))
            {
                return lGroup.ToArray();
            }

            // Get groups table from database
            DataTable dt = DAL.ApiDAL.Get_SubGroupsTree();
            lGroup = new List<GroupInfo>();
            if (dt != null)
            {
                if (dt.Rows != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        GroupInfo info = new GroupInfo();

                        info.Name = APILogic.Utils.GetSafeStr(row, "Group_Name");
                        info.ID = (long)row["ID"];
                        info.ParentID = (long)row["PARENT_GROUP_ID"];

                        lGroup.Add(info);
                    }
                }
            }
            else
            {
                throw new Exception("Failed getting groups table from database");
            }

            ApiCache.AddList<GroupInfo>(sGroupName, lGroup);

            return lGroup.ToArray();
        }

        static public string[] GetGroupPlayers(string sGroupName, bool sIncludeChildGroups, Int32 nGroupID)
        {
            List<string> ret = null;
            string key = string.Format("{0}_GetGroupPlayers_{1}", eWSModules.API.ToString(), sGroupName);
            if (ApiCache.GetList<string>(key, out ret))
            {
                return ret.ToArray();
            }

            DataTable dt = DAL.ApiDAL.Get_GroupPlayers();
            ret = new List<string>();
            if (dt != null)
            {
                EnumerableRowCollection<DataRow> source = dt.AsEnumerable();

                EnumerableRowCollection<DataRow> mainGroupRows = source.Where(item => item["GROUP_NAME"].ToString() == sGroupName);

                if (mainGroupRows.Count() == 0)
                {
                    // Main group not found
                    throw new Exception("Main group given was not found in groups table");
                }

                // Add group's players
                ret.AddRange(mainGroupRows.Select<DataRow, string>(item => item["USERNAME"].ToString()));

                long mainGroupID = (long)mainGroupRows.ElementAt(0)["ID"];

                if (sIncludeChildGroups)
                {
                    // Add child group's players
                    GetChildGroupPlayers(mainGroupID, ret, source);
                }
            }
            else
            {
                throw new Exception("Failed getting groups table from database");
            }

            // Sort return array
            ret.Sort();

            ApiCache.AddList<string>(key, ret);

            return ret.ToArray();
        }

        static private void GetChildGroupPlayers(long lParentGroupID, List<string> addToList, EnumerableRowCollection<DataRow> eSourceData)
        {
            EnumerableRowCollection<DataRow> children = eSourceData.Where(item => (long)item["PARENT_GROUP_ID"] == lParentGroupID);

            foreach (DataRow child in children)
            {
                addToList.Add(child["USERNAME"].ToString());

                GetChildGroupPlayers((long)child["ID"], addToList, eSourceData);
            }
        }

        static public string[] GetGroupMediaNames(string sGroupName, Int32 nGroupID)
        {
            List<string> lGroupMediaNames = null;
            string key = string.Format("{0}_GetGroupMediaNames_{1}", eWSModules.API.ToString(), sGroupName);
            if (ApiCache.GetList<string>(key, out lGroupMediaNames))
            {
                return lGroupMediaNames.ToArray();
            }

            DataSet ds = DAL.ApiDAL.Get_GroupMediaNames(sGroupName);
            string[] ret = null;
            int groupID = 0;
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    groupID = APILogic.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");

                    if (groupID != 0 && ds.Tables[1] != null)
                    {
                        EnumerableRowCollection<DataRow> source = ds.Tables[1].AsEnumerable();
                        ret = source.Select<DataRow, string>(item => item["NAME"].ToString()).ToArray();
                    }
                    else
                    {
                        throw new Exception("Failed getting group's media names from database");
                    }
                }
                else
                {
                    throw new Exception("Failed getting group's id from database");
                }
            }
            else
            {
                throw new Exception("Failed getting group's id from database");
            }

            ApiCache.AddList<string>(key, ret.ToList<string>());

            return ret;
        }


        static public ApiObjects.MediaMarkObject GetMediaMark(Int32 nGroupID, Int32 nMediaID, string sSiteGUID)
        {
            MediaMarkObject mmo = new MediaMarkObject();
            mmo.nGroupID = nGroupID;
            mmo.nMediaID = nMediaID;

            if (string.IsNullOrEmpty(sSiteGUID))
            {
                mmo.eStatus = MediaMarkObject.MediaMarkObjectStatus.MISSING_USER_SITE_GUID;
                return mmo;
            }

            mmo.sSiteGUID = sSiteGUID;

            if (nMediaID == 0)
            {
                mmo.eStatus = MediaMarkObject.MediaMarkObjectStatus.MISIING_MEDIA_ID;
                return mmo;
            }

            try
            {
                mmo = DAL.ApiDAL.Get_MediaMark(nMediaID, sSiteGUID, nGroupID);
            }
            catch
            {

                mmo.eStatus = MediaMarkObject.MediaMarkObjectStatus.FAILED;
            }

            return mmo;
        }

        static public ApiObjects.RateMediaObject RateMedia(Int32 nGroupID, Int32 nMediaID, string sSiteGUID, Int32 nRateVal)
        {
            RateMediaObject rmo = new RateMediaObject();
            rmo.oStatus.Initialize("OK", 0);

            Int32 nWatcherID = 0;
            int siteGuid = 0;
            if (!string.IsNullOrEmpty(sSiteGUID))
            {
                siteGuid = int.Parse(sSiteGUID);
                object oWatcherID = ODBCWrapper.Utils.GetTableSingleVal("watchers_groups_data", "watcher_id", "group_guid", "=", sSiteGUID);
                if (oWatcherID != null && oWatcherID != DBNull.Value)
                    nWatcherID = int.Parse(oWatcherID.ToString());
            }

            string sVotesCountField = "";
            if (nRateVal > 0 && nRateVal < 6)
                sVotesCountField = "VOTES_" + nRateVal.ToString() + "_COUNT";
            if (nRateVal < 1)
                sVotesCountField = "VOTES_LO_COUNT";
            if (nRateVal > 5)
                sVotesCountField = "VOTES_UP_COUNT";


            Int32 nVoted = TVinciShared.ProtocolsFuncs.UserVoteValueForMedia(nWatcherID, nMediaID, false, true, siteGuid);
            if (nVoted == -1)
            {
                ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                directQuery += "update media set ";
                directQuery += sVotesCountField + "=" + sVotesCountField + "+1";
                directQuery += ",VOTES_SUM=VOTES_SUM+" + nRateVal.ToString() + ",VOTES_COUNT=VOTES_COUNT+1 where";
                directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
                directQuery.Execute();
                directQuery.Finish();
                directQuery = null;

                if (nWatcherID > 0 || siteGuid > 0)
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("watchers_media_rating");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCHER_ID", "=", nWatcherID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RATE_VAL", "=", nRateVal);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", siteGuid);
                    insertQuery.Execute();
                    insertQuery.Finish();
                    insertQuery = null;
                }
            }
            else
            {
                rmo.oStatus.Initialize("User already voted", -1);
                return rmo;
            }

            Int32 nVotesSum = 0;
            Int32 nViews = 0;
            Int32 nVotesCnt = 0;
            double dAvg = 0.0;

            Int32 nVotesUpCnt = 0;
            Int32 nVotesLoCnt = 0;
            Int32 nVotes1Cnt = 0;
            Int32 nVotes2Cnt = 0;
            Int32 nVotes3Cnt = 0;
            Int32 nVotes4Cnt = 0;
            Int32 nVotes5Cnt = 0;

            TVinciShared.ProtocolsFuncs.GetCountersFromMedia(nMediaID, ref nViews, ref nVotesSum, ref nVotesCnt, ref dAvg
                , ref nVotesLoCnt
                , ref nVotesUpCnt
                , ref nVotes1Cnt
                , ref nVotes2Cnt
                , ref nVotes3Cnt
                , ref nVotes4Cnt
                , ref nVotes5Cnt, true);

            rmo.Initialize(nVotesSum, nVotesCnt, dAvg);

            int mediaType = Tvinci.Core.DAL.CatalogDAL.Get_MediaTypeIdByMediaId(nMediaID);

            // Insert statistic record to ElasticSearch
            ElasticSearch.Utilities.ESStatisticsUtilities.InsertSocialActionStatistics(
                nGroupID, nMediaID, mediaType, eUserAction.RATES, nRateVal);
            //InsertStatisticsToES(nGroupID, nMediaID, eUserAction.RATES, nRateVal);

            return rmo;
        }

        private static void AddSubAccountsToAccount(ref AdminAccountObj accountObj)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select id, group_name from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_group_id", "=", accountObj.m_id);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_archived", "=", 0);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string name = selectQuery.Table("query").DefaultView[i].Row["group_name"].ToString();
                        int id = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                        AdminAccountObj subAccount = new AdminAccountObj(id, name, string.Empty, accountObj.m_id);
                        accountObj.m_subGroups.Add(subAccount);
                        AddSubAccountsToAccount(ref subAccount);
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        static public ApiObjects.AdminAccountUserResponse GetAdminUserAccount(string user, string pass)
        {
            ApiObjects.AdminAccountUserResponse retVal = new AdminAccountUserResponse();

            DataTable dt = DAL.ApiDAL.Get_AdminUserAccount(user, pass);
            if (dt != null)
            {
                int count = dt.DefaultView.Count;
                if (count > 0)
                {
                    int id = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "id");
                    string name = APILogic.Utils.GetSafeStr(dt.Rows[0], "username");
                    string email = APILogic.Utils.GetSafeStr(dt.Rows[0], "email_add");
                    int groupID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "group_id");
                    int parentGroupID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "parent_group_id");
                    string groupName = APILogic.Utils.GetSafeStr(dt.Rows[0], "group_name");
                    AdminAccountUserObj userObj = new AdminAccountUserObj();
                    AdminAccountObj accountObj = new AdminAccountObj(groupID, groupName, string.Empty, parentGroupID);
                    AddSubAccountsToAccount(ref accountObj);
                    userObj.Initialize(id, name, email, groupID, accountObj);
                    retVal.Initialize(AdminUserStatus.OK, userObj);
                }
                else
                {
                    retVal.Initialize(AdminUserStatus.UserDoesNotExist, null);
                }
            }
            return retVal;
        }


        static public int InserEPGScheduleToChannel(int groupID, int channelID, string xml, bool deleteOld)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNodeList programList = xmlDoc.SelectNodes("/tv/programme");
            int count = programList.Count;
            if (deleteOld)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "0");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "2");
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", channelID);
                updateQuery.Execute();
                updateQuery = null;
            }
            for (int i = 0; i < count; i++)
            {
                XmlNode programNode = programList[i];
                string startDateStr = XmlUtils.GetItemParameterVal(ref programNode, "start");
                string endDateStr = XmlUtils.GetItemParameterVal(ref programNode, "stop");
                string name = XmlUtils.GetSafeValueFromXML(ref programNode, "title");
                string desc = XmlUtils.GetSafeValueFromXML(ref programNode, "desc");
                DateTime startDate = ParseEPGStrToDate(startDateStr);
                DateTime endDate = ParseEPGStrToDate(endDateStr);
                string epgIdentifier = string.Format("{0}_{1}", channelID.ToString(), i.ToString());

                ODBCWrapper.DataSetInsertQuery insertQuery = new ODBCWrapper.DataSetInsertQuery("epg_channels_schedule");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", channelID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", epgIdentifier);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", name);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Description", desc);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Start_Date", startDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("End_Date", endDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", groupID);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            return count;
        }

        static public List<EPGChannelObject> GetEPGChannel(int groupID, string sPicSize)
        {
            List<EPGChannelObject> res = new List<EPGChannelObject>();
            DataTable dt = DAL.ApiDAL.Get_EPGChannel(groupID);
            if (dt != null)
            {
                int count = dt.DefaultView.Count;
                if (count > 0)
                {
                    int i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        EPGChannelObject item = new EPGChannelObject();
                        string EPG_CHANNEL_ID = APILogic.Utils.GetSafeStr(dr, "ID");
                        string NAME = APILogic.Utils.GetSafeStr(dr, "NAME");
                        string DESCRIPTION = APILogic.Utils.GetSafeStr(dr, "DESCRIPTION");
                        string ORDER_NUM = APILogic.Utils.GetSafeStr(dr, "ORDER_NUM");
                        string IS_ACTIVE = APILogic.Utils.GetSafeStr(dr, "IS_ACTIVE");
                        string PIC_ID = PageUtils.GetPicURL(APILogic.Utils.GetIntSafeVal(dr, "PIC_ID"), sPicSize);
                        string GROUP_ID = APILogic.Utils.GetSafeStr(dr, "GROUP_ID");
                        string EDITOR_REMARKS = APILogic.Utils.GetSafeStr(dr, "EDITOR_REMARKS");
                        string STATUS = APILogic.Utils.GetSafeStr(dr, "STATUS");
                        string UPDATER_ID = APILogic.Utils.GetSafeStr(dr, "UPDATER_ID");
                        string CREATE_DATE = APILogic.Utils.GetSafeStr(dr, "CREATE_DATE");
                        string PUBLISH_DATE = APILogic.Utils.GetSafeStr(dr, "PUBLISH_DATE");
                        string CHANNEL_ID = APILogic.Utils.GetSafeStr(dr, "CHANNEL_ID");
                        string MEDIA_ID = APILogic.Utils.GetSafeStr(dr, "MEDIA_ID");
                        item.Initialize(EPG_CHANNEL_ID, NAME, DESCRIPTION, ORDER_NUM, IS_ACTIVE, PIC_ID, GROUP_ID, EDITOR_REMARKS, STATUS, UPDATER_ID, CREATE_DATE, PUBLISH_DATE, CHANNEL_ID, MEDIA_ID);
                        res.Add(item);
                        i++;
                    }
                }
            }
            return res;
        }

        static private Dictionary<string, List<EPGDictionary>> GetAllEPGMetaProgram(int nGroupID, DataTable ProgramID)
        {
            Dictionary<string, List<EPGDictionary>> EPG_ResponseMeta = new Dictionary<string, List<EPGDictionary>>();

            if (ProgramID != null && ProgramID.Rows.Count > 0)
            {
                string programIDSQL = ODBCWrapper.Utils.GetDelimitedStringFromDataTable(ProgramID, ",", "id", "in (", ")");  //convert program id to SQL statment

                ODBCWrapper.DataSetSelectQuery selectMetaQuery = new ODBCWrapper.DataSetSelectQuery();
                selectMetaQuery += " select mt.name as Name, mp.value as Value, mp.program_id as program_id  from epg_program_metas as mp inner join epg_metas_types as mt";
                selectMetaQuery += "on mt.id = mp.epg_meta_id ";
                selectMetaQuery += "where mp.group_id ";
                selectMetaQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
                selectMetaQuery += " and ";
                selectMetaQuery += string.Format("mp.program_id {0}", programIDSQL);
                selectMetaQuery += " and ";
                selectMetaQuery += "mp.status=1 and mt.status=1 and mt.is_active=1";

                if (selectMetaQuery.Execute("query", true) != null)
                {
                    int countMeta = selectMetaQuery.Table("query").DefaultView.Count;
                    if (countMeta > 0)
                    {
                        foreach (DataRowView drMeta in selectMetaQuery.Table("query").DefaultView)
                        {
                            EPGDictionary metaItem = new EPGDictionary();
                            metaItem.Key = ODBCWrapper.Utils.GetSafeStr(drMeta["Name"]);
                            metaItem.Value = ODBCWrapper.Utils.GetSafeStr(drMeta["Value"]);

                            if (EPG_ResponseMeta.ContainsKey(drMeta["program_id"].ToString()))
                            {
                                List<EPGDictionary> temp = EPG_ResponseMeta[drMeta["program_id"].ToString()];
                                temp.Add(metaItem);
                                EPG_ResponseMeta[drMeta["program_id"].ToString()] = temp;
                            }
                            else
                            {
                                List<EPGDictionary> temp = new List<EPGDictionary>();
                                temp.Add(metaItem);
                                EPG_ResponseMeta.Add(drMeta["program_id"].ToString(), temp);
                            }
                        }
                    }
                }
                selectMetaQuery.Finish();
                selectMetaQuery = null;
            }
            return EPG_ResponseMeta;
        }

        static private List<EPGDictionary> GetEPGMetasData(int nGroupID, string ProgramID)
        {
            List<EPGDictionary> EPG_ResponseMeta = new List<EPGDictionary>();

            ODBCWrapper.DataSetSelectQuery selectMetaQuery = new ODBCWrapper.DataSetSelectQuery();
            selectMetaQuery += " select mt.name as Name , mp.value as Value  from epg_program_metas as mp inner join epg_metas_types as mt";
            selectMetaQuery += "on mt.id = mp.epg_meta_id ";
            selectMetaQuery += "where mp.group_id ";
            selectMetaQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
            selectMetaQuery += " and ";
            selectMetaQuery += ODBCWrapper.Parameter.NEW_PARAM("mp.program_id", "=", ProgramID);
            selectMetaQuery += " and ";
            selectMetaQuery += "mp.status=1 and mt.status=1 and mt.is_active=1";

            if (selectMetaQuery.Execute("query", true) != null)
            {
                int countMeta = selectMetaQuery.Table("query").DefaultView.Count;
                if (countMeta > 0)
                {
                    foreach (DataRowView drMeta in selectMetaQuery.Table("query").DefaultView)
                    {
                        EPGDictionary metaItem = new EPGDictionary();
                        metaItem.Key = ODBCWrapper.Utils.GetSafeStr(drMeta["Name"]);
                        metaItem.Value = ODBCWrapper.Utils.GetSafeStr(drMeta["Value"]);
                        EPG_ResponseMeta.Add(metaItem);
                    }
                }
            }
            selectMetaQuery.Finish();
            selectMetaQuery = null;
            return EPG_ResponseMeta;
        }

        static private Dictionary<string, List<EPGDictionary>> GetAllEPGTagProgram(int nGroupID, DataTable ProgramID)
        {
            Dictionary<string, List<EPGDictionary>> EPG_ResponseTag = new Dictionary<string, List<EPGDictionary>>();

            if (ProgramID != null && ProgramID.Rows.Count > 0)
            {
                string programIDSQL = ODBCWrapper.Utils.GetDelimitedStringFromDataTable(ProgramID, ",", "id", "in (", ")");  //convert program id to SQL statment               

                ODBCWrapper.DataSetSelectQuery selectTagsQuery = new ODBCWrapper.DataSetSelectQuery();
                selectTagsQuery += " select tt.name as Name, tv.value as Value, tp.program_id as program_id from epg_program_tags as tp inner join epg_tags as tv on tp.epg_tag_id = tv.id";
                selectTagsQuery += "inner join epg_tags_types as tt on tv.epg_tag_type_id = tt.id ";
                selectTagsQuery += "where tp.group_id ";
                selectTagsQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
                selectTagsQuery += " and ";
                selectTagsQuery += string.Format("tp.program_id {0}", programIDSQL);
                selectTagsQuery += " and ";
                selectTagsQuery += "tp.status=1 and tv.status=1 and tt.status=1 and tt.is_active=1";

                if (selectTagsQuery.Execute("query", true) != null)
                {
                    int countMeta = selectTagsQuery.Table("query").DefaultView.Count;
                    if (countMeta > 0)
                    {
                        foreach (DataRowView drTag in selectTagsQuery.Table("query").DefaultView)
                        {
                            EPGDictionary tagItem = new EPGDictionary();
                            tagItem.Key = ODBCWrapper.Utils.GetSafeStr(drTag["Name"]);
                            tagItem.Value = ODBCWrapper.Utils.GetSafeStr(drTag["Value"]);
                            if (EPG_ResponseTag.ContainsKey(drTag["program_id"].ToString()))
                            {
                                List<EPGDictionary> temp = EPG_ResponseTag[drTag["program_id"].ToString()];
                                temp.Add(tagItem);
                                EPG_ResponseTag[drTag["program_id"].ToString()] = temp;
                            }
                            else
                            {
                                List<EPGDictionary> temp = new List<EPGDictionary>();
                                temp.Add(tagItem);
                                EPG_ResponseTag.Add(drTag["program_id"].ToString(), temp);
                            }
                        }
                    }
                }
                selectTagsQuery.Finish();
                selectTagsQuery = null;
            }
            return EPG_ResponseTag;
        }

        static private List<EPGDictionary> GetEPGTagsData(int nGroupID, string ProgramID)
        {
            List<EPGDictionary> EPG_ResponseTag = new List<EPGDictionary>();

            ODBCWrapper.DataSetSelectQuery selectTagsQuery = new ODBCWrapper.DataSetSelectQuery();
            selectTagsQuery += " select tt.name as Name, tv.value as Value from epg_program_tags as tp inner join epg_tags as tv on tp.epg_tag_id = tv.id";
            selectTagsQuery += "inner join epg_tags_types as tt on tv.epg_tag_type_id = tt.id ";
            selectTagsQuery += "where tp.group_id ";
            selectTagsQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
            selectTagsQuery += " and ";
            selectTagsQuery += ODBCWrapper.Parameter.NEW_PARAM("tp.program_id", "=", ProgramID);
            selectTagsQuery += " and ";
            selectTagsQuery += "tp.status=1 and tv.status=1 and tt.status=1 and tt.is_active=1";

            if (selectTagsQuery.Execute("query", true) != null)
            {
                int countMeta = selectTagsQuery.Table("query").DefaultView.Count;
                if (countMeta > 0)
                {
                    foreach (DataRowView drTag in selectTagsQuery.Table("query").DefaultView)
                    {
                        EPGDictionary tagItem = new EPGDictionary();
                        tagItem.Key = ODBCWrapper.Utils.GetSafeStr(drTag["Name"]);
                        tagItem.Value = ODBCWrapper.Utils.GetSafeStr(drTag["Value"]);
                        EPG_ResponseTag.Add(tagItem);
                    }
                }
            }
            selectTagsQuery.Finish();
            selectTagsQuery = null;
            return EPG_ResponseTag;
        }

        static public List<EPGChannelProgrammeObject> GetEPGChannelPrograms_Old(Int32 groupID, string sEPGChannelID, string sPicSize, EPGUnit unit, int nFromOffsetUnit, int nToOffsetUnit, int nUTCOffset)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();

            DateTime fromOffsetDay;
            DateTime toOffsetDay;

            DateTime UTCOffsetFromTimeZone = DateTimeOffset.UtcNow.AddHours(nUTCOffset).DateTime;
            DateTime UTCOffsetToTimeZone = DateTimeOffset.UtcNow.AddHours(nUTCOffset).DateTime;
            switch (unit)
            {
                case EPGUnit.Days:
                    DateTime fromDay = UTCOffsetFromTimeZone.AddDays(nFromOffsetUnit);
                    DateTime toDay = UTCOffsetToTimeZone.AddDays(nToOffsetUnit);
                    fromOffsetDay = new DateTime(fromDay.Year, fromDay.Month, fromDay.Day, 00, 00, 00);
                    toOffsetDay = new DateTime(toDay.Year, toDay.Month, toDay.Day, 23, 59, 59);

                    #region Get EPG Program Schedule
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += " select * from epg_channels_schedule where group_id ";
                    selectQuery += PageUtils.GetAllGroupTreeStr(groupID);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", ">", fromOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "<", toOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
                    selectQuery += " and status = 1 and is_active = 1 ";
                    selectQuery += " order by START_DATE asc";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            int i = 0;
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectQuery.Table("query"));
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectQuery.Table("query"));
                            foreach (DataRowView dr in selectQuery.Table("query").DefaultView)
                            {

                                long program_id = ODBCWrapper.Utils.GetLongSafeVal(dr["id"]);
                                string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                                string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                                string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                                string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                                DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "START_DATE", i);
                                string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                                DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "END_DATE", i);
                                string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
                                string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "PIC_ID", i), sPicSize, "epg_pics");
                                string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                                string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                                string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                                string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                                string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                                string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                                string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                                string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                                int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);

                                List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                                       where t.Key == program_id.ToString()
                                                                       select t.Value).FirstOrDefault<List<EPGDictionary>>();

                                List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                        where t.Key == program_id.ToString()
                                                                        select t.Value).FirstOrDefault<List<EPGDictionary>>();



                                EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                                item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                                res.Add(item);
                                i++;
                            }
                        }

                    }
                    selectQuery.Finish();
                    selectQuery = null;
                    #endregion

                    break;
                case EPGUnit.Hours:

                    fromOffsetDay = UTCOffsetFromTimeZone.AddHours(nFromOffsetUnit);
                    toOffsetDay = UTCOffsetToTimeZone.AddHours(nToOffsetUnit);

                    #region Get EPG Program Schedule
                    ODBCWrapper.DataSetSelectQuery selectHourQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectHourQuery += " select * from epg_channels_schedule where group_id ";
                    selectHourQuery += PageUtils.GetAllGroupTreeStr(groupID);
                    selectHourQuery += " and ";
                    selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", ">", fromOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectHourQuery += " and ";
                    selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "<", toOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectHourQuery += " and ";
                    selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
                    selectHourQuery += " and status = 1 and is_active = 1 ";
                    selectHourQuery += " order by START_DATE asc";
                    if (selectHourQuery.Execute("query", true) != null)
                    {
                        int count = selectHourQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectHourQuery.Table("query"));
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectHourQuery.Table("query"));

                            int i = 0;
                            foreach (DataRowView dr in selectHourQuery.Table("query").DefaultView)
                            {
                                string program_id = ODBCWrapper.Utils.GetSafeStr(dr["id"]);
                                string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                                string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                                string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                                string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                                DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectHourQuery, "START_DATE", i);
                                string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                                DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectHourQuery, "END_DATE", i);
                                string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
                                string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectHourQuery, "PIC_ID", i), sPicSize);
                                string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                                string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                                string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                                string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                                string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                                string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                                string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                                string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                                int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);
                                List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                                       where t.Key == program_id
                                                                       select t.Value).FirstOrDefault<List<EPGDictionary>>();

                                List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                        where t.Key == program_id
                                                                        select t.Value).FirstOrDefault<List<EPGDictionary>>();


                                EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                                item.Initialize(0, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                                res.Add(item);
                                i++;
                            }
                        }

                    }
                    selectHourQuery.Finish();
                    selectHourQuery = null;
                    #endregion
                    break;

                case EPGUnit.Current:
                    fromOffsetDay = UTCOffsetFromTimeZone;
                    toOffsetDay = UTCOffsetToTimeZone;

                    #region Get EPG Program Schedule
                    if (nFromOffsetUnit > 0)
                    {
                        #region Get Befor Programmes
                        ODBCWrapper.DataSetSelectQuery selectBeforQuery = new ODBCWrapper.DataSetSelectQuery();
                        selectBeforQuery += string.Format(" select TOP({0}) * from epg_channels_schedule where group_id ", nFromOffsetUnit);
                        selectBeforQuery += PageUtils.GetAllGroupTreeStr(groupID);
                        selectBeforQuery += " and ";
                        selectBeforQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "<", fromOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                        selectBeforQuery += " and ";
                        selectBeforQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
                        selectBeforQuery += " and status = 1 and is_active = 1 ";
                        selectBeforQuery += " order by END_DATE desc";
                        if (selectBeforQuery.Execute("query", true) != null)
                        {
                            int count = selectBeforQuery.Table("query").DefaultView.Count;
                            if (count > 0)
                            {
                                int i = 0;

                                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectBeforQuery.Table("query"));
                                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectBeforQuery.Table("query"));

                                foreach (DataRowView dr in selectBeforQuery.Table("query").DefaultView)
                                {
                                    long program_id = ODBCWrapper.Utils.GetLongSafeVal(dr["id"]);
                                    string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                                    string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                                    string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                                    string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                                    DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectBeforQuery, "START_DATE", i);
                                    string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                                    DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectBeforQuery, "END_DATE", i);
                                    string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
                                    string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectBeforQuery, "PIC_ID", i), sPicSize);
                                    string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                                    string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                                    string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                                    string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                                    string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                                    string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                                    string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                                    string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                                    int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);
                                    List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                                           where t.Key == program_id.ToString()
                                                                           select t.Value).FirstOrDefault<List<EPGDictionary>>();

                                    List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                            where t.Key == program_id.ToString()
                                                                            select t.Value).FirstOrDefault<List<EPGDictionary>>();


                                    EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                                    item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                                    res.Add(item);
                                    i++;
                                }
                            }

                        }
                        selectBeforQuery.Finish();
                        selectBeforQuery = null;
                        #endregion
                    }
                    #region Get Current Program
                    ODBCWrapper.DataSetSelectQuery selectUnitQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectUnitQuery += " select * from epg_channels_schedule where group_id ";
                    selectUnitQuery += PageUtils.GetAllGroupTreeStr(groupID);
                    selectUnitQuery += " and ";
                    selectUnitQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", ">", fromOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectUnitQuery += " and ";
                    selectUnitQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "<", toOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                    selectUnitQuery += " and ";
                    selectUnitQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
                    selectUnitQuery += " and status = 1 and is_active = 1 ";
                    selectUnitQuery += " order by START_DATE asc";
                    if (selectUnitQuery.Execute("query", true) != null)
                    {
                        int count = selectUnitQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            int i = 0;
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectUnitQuery.Table("query"));
                            Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectUnitQuery.Table("query"));

                            foreach (DataRowView dr in selectUnitQuery.Table("query").DefaultView)
                            {
                                long program_id = ODBCWrapper.Utils.GetLongSafeVal(dr["id"]);
                                string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                                string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                                string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                                string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                                DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectUnitQuery, "START_DATE", i);
                                string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                                DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectUnitQuery, "END_DATE", i);
                                string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");


                                string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectUnitQuery, "PIC_ID", i), sPicSize);
                                string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                                string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                                string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                                string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                                string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                                string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                                string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                                string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                                int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);

                                List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                                       where t.Key == program_id.ToString()
                                                                       select t.Value).FirstOrDefault<List<EPGDictionary>>();

                                List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                        where t.Key == program_id.ToString()
                                                                        select t.Value).FirstOrDefault<List<EPGDictionary>>();


                                EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                                item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                                res.Add(item);
                                i++;
                            }
                        }

                    }
                    selectUnitQuery.Finish();
                    selectUnitQuery = null;
                    #endregion

                    if (nToOffsetUnit > 0)
                    {
                        #region Get after Programmes
                        ODBCWrapper.DataSetSelectQuery selectBeforQuery = new ODBCWrapper.DataSetSelectQuery();
                        selectBeforQuery += string.Format(" select TOP({0}) * from epg_channels_schedule where group_id ", nToOffsetUnit);
                        selectBeforQuery += PageUtils.GetAllGroupTreeStr(groupID);
                        selectBeforQuery += " and ";
                        selectBeforQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", ">", toOffsetDay.ToString("yyyy-MM-dd HH:mm:ss"));
                        selectBeforQuery += " and ";
                        selectBeforQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
                        selectBeforQuery += " and status = 1 and is_active = 1 ";
                        selectBeforQuery += " order by START_DATE asc";

                        if (selectBeforQuery.Execute("query", true) != null)
                        {
                            int count = selectBeforQuery.Table("query").DefaultView.Count;
                            if (count > 0)
                            {
                                int i = 0;
                                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectBeforQuery.Table("query"));
                                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectBeforQuery.Table("query"));
                                foreach (DataRowView dr in selectBeforQuery.Table("query").DefaultView)
                                {
                                    long program_id = ODBCWrapper.Utils.GetLongSafeVal(dr["id"]);
                                    string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                                    string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                                    string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                                    string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                                    DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectBeforQuery, "START_DATE", i);
                                    string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                                    DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectBeforQuery, "END_DATE", i);
                                    string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
                                    string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectBeforQuery, "PIC_ID", i), sPicSize);
                                    string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                                    string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                                    string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                                    string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                                    string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                                    string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                                    string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                                    string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                                    int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);

                                    List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                                           where t.Key == program_id.ToString()
                                                                           select t.Value).FirstOrDefault<List<EPGDictionary>>();

                                    List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                            where t.Key == program_id.ToString()
                                                                            select t.Value).FirstOrDefault<List<EPGDictionary>>();


                                    EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                                    item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                                    res.Add(item);
                                    i++;
                                }
                            }

                        }
                        selectBeforQuery.Finish();
                        selectBeforQuery = null;
                        #endregion
                    }
                    #endregion
                    break;

            }
            return res;
        }

        static public List<EPGChannelProgrammeObject> GetEPGChannelProgramsByDates_Old(Int32 groupID, string sEPGChannelID, string sPicSize, DateTime fromDay, DateTime toDay, double nUTCOffset)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();

            DateTime fromUTCDay = fromDay.AddHours(nUTCOffset);
            DateTime toUTCDay = toDay.AddHours(nUTCOffset);

            ODBCWrapper.DataSetSelectQuery selectHourQuery = new ODBCWrapper.DataSetSelectQuery();
            selectHourQuery += " select * from epg_channels_schedule where group_id ";
            selectHourQuery += PageUtils.GetAllGroupTreeStr(groupID);
            selectHourQuery += " and ";
            selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", ">", fromUTCDay.ToString("yyyy-MM-dd HH:mm:ss"));
            selectHourQuery += " and ";
            selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "<", toUTCDay.ToString("yyyy-MM-dd HH:mm:ss"));
            selectHourQuery += " and ";
            selectHourQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", sEPGChannelID);
            selectHourQuery += " and status = 1 and is_active = 1 ";
            selectHourQuery += " order by START_DATE asc";
            if (selectHourQuery.Execute("query", true) != null)
            {
                int count = selectHourQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, selectHourQuery.Table("query"));
                    Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, selectHourQuery.Table("query"));

                    int i = 0;
                    foreach (DataRowView dr in selectHourQuery.Table("query").DefaultView)
                    {
                        long program_id = ODBCWrapper.Utils.GetLongSafeVal(dr["id"]);
                        string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(dr["EPG_CHANNEL_ID"]);
                        string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(dr["EPG_IDENTIFIER"]);
                        string NAME = ODBCWrapper.Utils.GetSafeStr(dr["NAME"]);
                        string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(dr["DESCRIPTION"]);
                        DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(selectHourQuery, "START_DATE", i);
                        string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
                        DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(selectHourQuery, "END_DATE", i);
                        string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
                        string PIC_ID = PageUtils.GetPicURL(ODBCWrapper.Utils.GetIntSafeVal(selectHourQuery, "PIC_ID", i), sPicSize);
                        string STATUS = ODBCWrapper.Utils.GetSafeStr(dr["STATUS"]);
                        string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(dr["IS_ACTIVE"]);
                        string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(dr["GROUP_ID"]);
                        string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(dr["UPDATER_ID"]);
                        string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["UPDATE_DATE"]);
                        string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(dr["PUBLISH_DATE"]);
                        string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(dr["CREATE_DATE"]);
                        string media_id = ODBCWrapper.Utils.GetSafeStr(dr["MEDIA_ID"]);
                        int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dr["like_counter"]);
                        List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                               where t.Key == program_id.ToString()
                                                               select t.Value).FirstOrDefault<List<EPGDictionary>>();

                        List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                                where t.Key == program_id.ToString()
                                                                select t.Value).FirstOrDefault<List<EPGDictionary>>();


                        EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
                        item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_ID, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);

                        res.Add(item);
                        i++;
                    }
                }
            }
            selectHourQuery.Finish();
            selectHourQuery = null;
            return res;
        }

        static private DateTime ParseEPGStrToDate(string dateStr)
        {
            DateTime dt = new DateTime();
            int year = int.Parse(dateStr.Substring(0, 4));
            int month = int.Parse(dateStr.Substring(4, 2));
            int day = int.Parse(dateStr.Substring(6, 2));
            int hour = int.Parse(dateStr.Substring(8, 2));
            int min = int.Parse(dateStr.Substring(10, 2));
            int sec = int.Parse(dateStr.Substring(12, 2));
            dt = new DateTime(year, month, day, hour, min, sec);
            return dt;
        }

        static public bool AddUserSocialAction(Int32 nGroupID, Int32 nMediaID, string sSiteGUID, Int32 nSocialAction, Int32 nSocialPlatform)
        {
            bool res = true;

            SocialAction eSocialAction = (SocialAction)nSocialAction;
            SocialPlatform eSocialPlatform = (SocialPlatform)nSocialPlatform;

            switch (eSocialAction)
            {
                case SocialAction.LIKE:
                    {
                        Int32 nID = GetUserSocialActionID(nGroupID, nMediaID, sSiteGUID, eSocialAction, eSocialPlatform);
                        if (nID > 0)
                        {
                            //Update Like - set is_active=1
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_social_actions");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;

                            //Update UnLike - set is_active=0
                            updateQuery = new ODBCWrapper.UpdateQuery("users_social_actions");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGUID);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("social_action", "=", (int)SocialAction.UNLIKE);
                            updateQuery += " and ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("social_platform", "=", (int)eSocialPlatform);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                        else
                        {
                            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_social_actions");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGUID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("social_platform", "=", (int)eSocialPlatform);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("social_action", "=", (int)eSocialAction);
                            insertQuery.Execute();
                            insertQuery.Finish();
                            insertQuery = null;
                        }

                        UpdateMediaLikeCounter(nMediaID, 1);

                        break;
                    }
                case SocialAction.UNLIKE:
                    {
                        //Update Like - set is_active=0
                        Int32 nID = GetUserSocialActionID(nGroupID, nMediaID, sSiteGUID, SocialAction.LIKE, eSocialPlatform);
                        if (nID > 0)
                        {
                            //Update Like - set is_active=1
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_social_actions");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 0);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }

                        //Update UnLike
                        nID = GetUserSocialActionID(nGroupID, nMediaID, sSiteGUID, eSocialAction, eSocialPlatform);
                        if (nID > 0)
                        {
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_social_actions");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);

                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                        else
                        {
                            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_social_actions");
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGUID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("social_platform", "=", (int)eSocialPlatform);
                            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("social_action", "=", (int)eSocialAction);
                            insertQuery.Execute();
                            insertQuery.Finish();
                            insertQuery = null;
                        }

                        UpdateMediaLikeCounter(nMediaID, -1);

                        break;
                    }
                default:
                    {
                        res = false;
                        break;
                    }
            }

            return res;
        }

        static private Int32 GetUserSocialActionID(Int32 nGroupID, Int32 nMediaID, string sSiteGUID, SocialAction eSocialAction, SocialPlatform eSocialPlatform)
        {
            Int32 nID = 0;

            DataTable dt = DAL.ApiDAL.Get_UserSocialActionID(nMediaID, sSiteGUID, nGroupID, (int)eSocialPlatform, (int)eSocialAction);
            if (dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    nID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "id");
                }
            }
            return nID;
        }

        static private Int32 UpdateMediaLikeCounter(Int32 nMediaID, int nVal)
        {
            Int32 nlikeCounter = 0;

            object oLikeCounter = ODBCWrapper.Utils.GetTableSingleVal("media", "like_counter", nMediaID);
            if (oLikeCounter != null && oLikeCounter != DBNull.Value)
                nlikeCounter = int.Parse(oLikeCounter.ToString());

            nlikeCounter += nVal;

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("like_counter", "=", nlikeCounter);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            return nlikeCounter;
        }

        static public bool RunImporter(Int32 nGroupID, string extraParams)
        {
            return false;
        }

        protected Int32 m_nGroupID;

        public static Country GetCountryByIp(int groupId, string ip)
        {
            Country country = null;
            try
            {
                string key = LayeredCacheKeys.GetKeyForIp(ip);
                if (!LayeredCache.Instance.Get<Country>(key, ref country, APILogic.Utils.GetCountryByIpFromES, new Dictionary<string, object>() { { "ip", ip } },
                                                        groupId, LayeredCacheConfigNames.COUNTRY_BY_IP_LAYERED_CACHE_CONFIG_NAME,
                                                        new List<string>() { LayeredCacheConfigNames.GET_COUNTRY_BY_IP_INVALIDATION_KEY }))
                {
                    log.ErrorFormat("Failed getting country by ip from LayeredCache, ip: {0}, key: {1}", ip, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetCountryByIp for ip: {0}", ip), ex);
            }

            return country;
        }

        static public bool CheckGeoBlockMedia(Int32 groupId, Int32 mediaId, string ip, out string ruleName)
        {
            bool isBlocked = false;
            Int32 nGeoBlockID = 0;
            int nProxyRule = 0;
            double dProxyLevel = 0.0;

            ruleName = string.Empty;
            string key = LayeredCacheKeys.GetCheckGeoBlockMediaKey(groupId, mediaId);
            DataTable dt = null;
            // try to get from cache            
            bool cacheResult = LayeredCache.Instance.Get<DataTable>(key, ref dt, APILogic.Utils.Get_GeoBlockPerMedia, new Dictionary<string, object>() { { "groupId", groupId },
                                                                    { "mediaId", mediaId } }, groupId, LayeredCacheConfigNames.CHECK_GEO_BLOCK_MEDIA_LAYERED_CACHE_CONFIG_NAME,
                                                                    new List<string>() { LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaId) });
            if (cacheResult && dt != null)
            {
                if (dt.Rows != null && dt.Rows.Count > 0)
                {
                    Country country = GetCountryByIp(groupId, ip);
                    Int32 nCountryID = country != null ? country.Id : 0;
                    ruleName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "NAME");
                    nGeoBlockID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "ID");
                    int nONLY_OR_BUT = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "ONLY_OR_BUT");

                    DataRow[] existingRows = dt.Select(string.Format("COUNTRY_ID={0}", nCountryID));
                    bool bExsitInRuleM2M = existingRows != null && existingRows.Length == 1 ? true : false;

                    log.Debug("Geo Blocks - Geo Block ID " + nGeoBlockID + " Country ID " + nCountryID);

                    if (nGeoBlockID > 0)
                    {
                        //No one except
                        if (nONLY_OR_BUT == 0)
                            isBlocked = !bExsitInRuleM2M;
                        //All except
                        if (nONLY_OR_BUT == 1)
                            isBlocked = bExsitInRuleM2M;

                        if (!isBlocked) // then check what about the proxy - is it reliable 
                        {
                            nProxyRule = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "PROXY_RULE");
                            dProxyLevel = APILogic.Utils.GetDoubleSafeVal(dt.Rows[0], "PROXY_LEVEL");
                            isBlocked = !(MaxMind.IsProxyAllowed(nProxyRule, dProxyLevel, ip, groupId));
                        }
                    }
                }
            }

            return isBlocked;
        }


        static public bool CheckMediaUserType(Int32 nMediaID, int nSiteGuid, int groupId)
        {
            bool result = true;
            int nUserTypeID = 0;

            try
            {
                Users.UserResponseObject response = Core.Users.Module.GetUserData(groupId, nSiteGuid.ToString(), string.Empty);
                // Make sure response is OK
                if (response != null && response.m_RespStatus == ResponseStatus.OK && response.m_user != null && response.m_user.m_eSuspendState == DAL.DomainSuspentionStatus.OK
                    && response.m_user.m_oBasicData != null && response.m_user.m_oBasicData.m_UserType.ID.HasValue)
                {
                    nUserTypeID = response.m_user.m_oBasicData.m_UserType.ID.Value;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(string.Format("CheckMediaUserType - Error when calling GetUserData, user {0} in group {1}. ex = {2}, ST = {3}", nSiteGuid, groupId, ex.Message, ex.StackTrace), ex);
            }

            string key = LayeredCacheKeys.GetIsMediaExistsToUserTypeKey(nMediaID, nUserTypeID);
            bool? isMediaExistsToUserType = null;
            // try go get from cache
            bool cacheResult = LayeredCache.Instance.Get<bool?>(key, ref isMediaExistsToUserType, APILogic.Utils.GetIsMediaExistsToUserType,
                                                                new Dictionary<string, object>() { { "mediaId", nMediaID }, { "userTypeId", nUserTypeID } },
                                                                groupId, LayeredCacheConfigNames.MEDIA_USER_TYPE_LAYERED_CACHE_CONFIG_NAME);

            if (cacheResult && isMediaExistsToUserType.HasValue)
            {
                result = isMediaExistsToUserType.Value;
            }

            return result;
        }

        public static bool SendToFriend(int nGroupID, string sSenderName, string sSendaerMail, string sMailTo, string sNameTo, int nMediaID)
        {
            bool retVal = false;
            string sMediaName = string.Empty;
            string sMediaType = string.Empty;
            string sMailSubject = string.Empty;
            string sMailTemplate = string.Empty;
            string sender_mail = string.Empty;

            DataSet ds = DAL.ApiDAL.Get_MediaDetailsForEmail(nMediaID, nGroupID);
            if (ds != null)
            {
                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        sMediaName = ds.Tables[0].Rows[0]["Name"].ToString();
                        sMediaType = ds.Tables[0].Rows[0]["MediaType"].ToString();
                    }
                    if (ds.Tables.Count > 1)
                    {
                        if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                        {
                            sMailTemplate = ds.Tables[1].Rows[0]["mail_template"].ToString();
                            sMailSubject = ds.Tables[1].Rows[0]["mail_subject"].ToString();
                            sender_mail = ds.Tables[1].Rows[0]["sender_mail"].ToString();
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(sender_mail))
            {
                log.ErrorFormat("send mail for meida id ={0}, nGroupID ={1}, sSenderName ={2}, sMailTo ={3}, sNameTo ={4} fail due default sender_mail per group is null or empty",
                    nMediaID, nGroupID, sSenderName, sMailTo, sNameTo);
                return false;
            }

            SendToFriendMailRequest request = new SendToFriendMailRequest();
            request.m_eMailType = eMailTemplateType.SendToFriend;
            request.m_sContentName = sMediaName;
            request.m_sFirstName = sNameTo;
            request.m_sMediaID = nMediaID.ToString();
            request.m_sMediaType = HttpUtility.UrlEncode(sMediaType);

            request.m_sSenderFrom = sender_mail;
            request.m_sSenderName = sSenderName;
            request.m_sSenderTo = sMailTo;
            request.m_sSubject = sMailSubject;
            request.m_sTemplateName = sMailTemplate;
            retVal = SendMailTemplate(request);
            return retVal;
        }

        static public List<GroupRule> GetGroupMediaRules(int mediaId, string ip, string siteGuid, int groupId, string deviceUdid)
        {
            List<GroupRule> groupRules = new List<GroupRule>();

            //Check if geo-block applies
            string ruleName;
            if (CheckGeoBlockMedia(groupId, (int)mediaId, ip, out ruleName))
            {
                groupRules.Add(new GroupRule()
                {
                    Name = "GeoBlock",
                    IsActive = true,
                    BlockType = eBlockType.Geo,

                });
            }

            //Check if user type match media user types
            if (!string.IsNullOrEmpty(siteGuid) && CheckMediaUserType((int)mediaId, int.Parse(siteGuid), groupId) == false)
            {
                groupRules.Add(new GroupRule()
                {
                    RuleID = 0,
                    Name = "UserTypeBlock",
                    BlockType = eBlockType.UserType,
                    IsActive = true
                });
            }

            var response = GetParentalMediaRules(groupId, siteGuid, mediaId, 0);

            if (response != null && response.status != null && response.status.Code == 0)
            {
                groupRules.AddRange(ConvertParentalToGroupRule(response.rules));
            }

            return groupRules;

            #region old code
            //List<GroupRule> tempRules = getMediaUserRules(nMediaID, nSiteGuid);
            //List<GroupRule> resultRules = GetRules(eGroupRuleType.Parental, nSiteGuid, nMediaID, nGroupID, sIP, tempRules);
            //return resultRules; 
            #endregion
        }

        private static List<GroupRule> getMediaUserRules(int nMediaID, int nSiteGuid)
        {
            DataTable rulesDt = DAL.ApiDAL.Get_GroupMediaRules(nMediaID, nSiteGuid.ToString());
            List<GroupRule> userRules = new List<GroupRule>();
            GroupRule rule = new GroupRule();
            if (rulesDt != null)
            {
                if (rulesDt.Rows != null && rulesDt.Rows.Count > 0)
                {
                    for (int i = 0; i < rulesDt.Rows.Count; i++)
                    {
                        int nRuleID = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "rule_id");
                        int nTagTypeID = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "TAG_TYPE_ID");
                        string sValue = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "VALUE");
                        string sKey = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "Key");
                        string sName = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "Name");
                        object sAgeRestriction = rulesDt.Rows[i]["age_restriction"];
                        int nIsActive = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "is_active");
                        eGroupRuleType eRuleType = (eGroupRuleType)APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "group_rule_type_id");
                        int nBlockAnonymous = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "block_anonymous");
                        bool bBlockAnonymous = (nBlockAnonymous == 1);
                        rule = new GroupRule(nRuleID, nTagTypeID, sValue, sKey, sName, sAgeRestriction, nIsActive, eRuleType, bBlockAnonymous);
                        userRules.Add(rule);
                    }
                }
            }
            return userRules;
        }

        private static bool CheckAgeValidation(int ageLimit, int userID)
        {
            bool retVal = false;
            DataTable dt = DAL.ApiDAL.Get_DetailsUsersDynamicData(userID);
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    int count = dt.DefaultView.Count;
                    int birthDay = 0;
                    int birthMonth = 0;
                    int birthYear = 0;
                    DateTime birthDate = DateTime.MaxValue;
                    for (int i = 0; i < count; i++)
                    {
                        string dataType = dt.Rows[i]["data_type"].ToString();
                        string dataValue = dt.Rows[i]["data_value"].ToString();
                        if (dataType.ToLower().Equals("birthday"))
                        {
                            if (dataValue.ToLower().Contains("/"))
                            {
                                if (DateTime.TryParse(dataValue.ToLower(), out birthDate))
                                {
                                    break;
                                }
                            }
                            else
                            {
                                birthDay = int.Parse(dataValue);
                            }
                        }
                        else if (dataType.ToLower().Equals("birthmonth"))
                        {
                            birthMonth = int.Parse(dataValue);
                        }
                        else if (dataType.ToLower().Equals("birthyear"))
                        {
                            birthYear = int.Parse(dataValue);
                        }

                    }
                    if (birthDate == DateTime.MaxValue)
                    {
                        if (birthYear != 0 && birthYear != 0)
                        {
                            birthDate = new DateTime(birthYear, birthMonth, birthDay);
                        }
                    }
                    if (birthDate != DateTime.MaxValue)
                    {
                        if (birthDate.CompareTo(DateTime.Now.AddYears(-ageLimit)) < 0)
                        {
                            retVal = true;
                        }
                    }
                }
            }

            return retVal;
        }

        static public bool SendMailTemplate(MailRequestObj request)
        {
            bool retVal = false;
            Mailer.IMailer mailer = Mailer.MailFactory.GetMailer(Mailer.MailImplementors.MCMailer);
            retVal = mailer.SendMailTemplate(request);
            return retVal;
        }

        /// <summary>
        /// Return all of the rules for group. Parental media, parental EPG,& purchase
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        static public List<GroupRule> GetGroupRules(int groupId)
        {
            List<GroupRule> groupRules = new List<GroupRule>();

            var response = GetParentalRules(groupId);

            if (response != null && response.status != null && response.status.Code == 0)
            {
                groupRules = ConvertParentalToGroupRule(response.rules);
            }

            ePurchaeSettingsType purchaseSetting = DAL.ApiDAL.Get_Group_PurchaseSetting(groupId);

            // Only relevant to ask/block setting (block is treated as ask, although it is wrong, because of backward compatibility issue). 
            // allow is not supported as purchase rule.
            if (purchaseSetting == ePurchaeSettingsType.Ask || purchaseSetting == ePurchaeSettingsType.Block)
            {
                GroupRule settingsRule = CreateSettingsGroupRule(purchaseSetting);

                groupRules.Add(settingsRule);
            }

            return groupRules;

            #region old code
            //List<GroupRule> retRules = new List<GroupRule>();

            //DataTable dt = DAL.ApiDAL.Get_GroupRules(nGroupID);
            //if (dt != null)
            //{
            //    if (dt.Rows != null && dt.Rows.Count > 0)
            //    {
            //        DataTable ruleTypeDt = dt.DefaultView.ToTable(true, "ID", "Name", "TAG_TYPE_ID", "dynamic_data_key", "Order_Num", "group_rule_type_id");
            //        GroupRule gr;
            //        foreach (DataRow dr in ruleTypeDt.Rows)
            //        {
            //            gr = new GroupRule();
            //            int nGroupRuleType = APILogic.Utils.GetIntSafeVal(dr, "group_rule_type_id");
            //            gr.RuleID = APILogic.Utils.GetIntSafeVal(dr, "ID");
            //            gr.Name = APILogic.Utils.GetSafeStr(dr, "Name");
            //            gr.TagTypeID = APILogic.Utils.GetIntSafeVal(dr, "TAG_TYPE_ID");
            //            gr.DynamicDataKey = APILogic.Utils.GetSafeStr(dr, "dynamic_data_key");
            //            gr.OrderNum = APILogic.Utils.GetIntSafeVal(dr, "ORDER_NUM");
            //            gr.GroupRuleType = nGroupRuleType > 0 ? (eGroupRuleType)(nGroupRuleType) : eGroupRuleType.Parental;
            //            gr.AllTagValues = new List<string>();
            //            gr.IsActive = true;
            //            DataRow[] drArr = dt.Select("ID=" + gr.RuleID);
            //            foreach (DataRow tagIdDr in drArr)
            //            {
            //                gr.AllTagValues.Add(tagIdDr["Value"].ToString());
            //            }
            //            retRules.Add(gr);
            //        }
            //    }
            //}

            //return retRules.OrderBy(gr => gr.OrderNum).ToList(); 
            #endregion
        }

        #region User Group Rules

        static public List<GroupRule> GetUserGroupRules(int groupID, string siteGuid)
        {
            return GetUserDomainGroupRules(groupID, siteGuid, 0);

            #region Old code

            //Dictionary<int, GroupRule> groupRulesDict = new Dictionary<int, GroupRule>();

            //DataTable dtGroupRules = DAL.ApiDAL.Get_UserGroupRules(sSiteGuid);

            //if (dtGroupRules != null)
            //{
            //    foreach (DataRow drGroupRule in dtGroupRules.Rows)
            //    {
            //        GroupRule gr = new GroupRule();
            //        int nGroupRuleType = APILogic.Utils.GetIntSafeVal(drGroupRule, "group_rule_type_id");
            //        gr.RuleID = APILogic.Utils.GetIntSafeVal(drGroupRule, "rule_id");
            //        gr.IsActive = (APILogic.Utils.GetIntSafeVal(drGroupRule, "is_active") == 1) ? true : false;
            //        gr.Name = APILogic.Utils.GetSafeStr(drGroupRule, "name");
            //        gr.TagTypeID = APILogic.Utils.GetIntSafeVal(drGroupRule, "tag_type_id");
            //        gr.DynamicDataKey = APILogic.Utils.GetSafeStr(drGroupRule, "dynamic_data_key");
            //        gr.OrderNum = APILogic.Utils.GetIntSafeVal(drGroupRule, "order_num");
            //        gr.GroupRuleType = nGroupRuleType > 0 ? (eGroupRuleType)(nGroupRuleType) : eGroupRuleType.Parental;
            //        gr.AllTagValues = new List<string>();
            //        groupRulesDict.Add(gr.RuleID, gr);
            //    }
            //}

            //if (groupRulesDict.Count > 0)
            //{
            //    List<GroupRule> parentalGroupRules = groupRulesDict.Values.ToList().Where(x => x.GroupRuleType == eGroupRuleType.Parental).ToList();
            //    List<int> ruleIDsList = parentalGroupRules.Select(x => x.RuleID).ToList();
            //    DataTable dtGroupRulesTagsValues = DAL.ApiDAL.GetGroupRulesTagsValues(ruleIDsList);
            //    if (dtGroupRulesTagsValues != null)
            //    {
            //        foreach (DataRow drTagValue in dtGroupRulesTagsValues.Rows)
            //        {
            //            int ruleID = APILogic.Utils.GetIntSafeVal(drTagValue, "ruleID");
            //            string tagValue = APILogic.Utils.GetSafeStr(drTagValue, "Value");

            //            if (groupRulesDict.ContainsKey(ruleID) == true && groupRulesDict[ruleID] != null)
            //            {
            //                groupRulesDict[ruleID].AllTagValues.Add(tagValue);
            //            }
            //        }
            //    }
            //}
            //return groupRulesDict.Values.ToList();

            #endregion

        }

        /// <summary>
        /// Set parental and purchase rule for a user. Given Rule ID
        /// </summary>
        /// <param name="siteGuid"></param>
        /// <param name="ruleId"></param>
        /// <param name="ruleStatus"></param>
        /// <param name="pin"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        static public bool SetUserGroupRule(string siteGuid, int ruleId, int ruleStatus, string pin, int groupId)
        {
            bool result = false;
            bool setRuleResult = false;
            bool setPinResult = false;
            int isActive = 0;

            if (ruleStatus == 1)
            {
                isActive = 1;
            }

            // ruleID 0 means it is purchase rule
            if (ruleId == 0)
            {
                ePurchaeSettingsType type = ePurchaeSettingsType.Allow;

                // activating - setting "ask". Disabling - setting "allow"
                if (isActive == 1)
                {
                    type = ePurchaeSettingsType.Ask;
                }

                var setStatus = SetPurchaseSettings(groupId, 0, siteGuid, type);

                if (setStatus != null && setStatus.Code == 0)
                {
                    setRuleResult = true;
                }
            }
            // otherwise it is parental rule
            else
            {
                ApiObjects.Response.Status setStatus = null;

                setStatus = SetUserParentalRules(groupId, siteGuid, ruleId, isActive, 0);

                // If user wants to activate a rule - normal behavior
                if (setStatus != null && setStatus.Code == 0)
                {
                    setRuleResult = true;
                }

                // if user wants to deactivate, we need to reactivate other rules
                if (isActive == 0)
                {
                    setRuleResult = false;

                    List<ParentalRule> parentalRules = DAL.ApiDAL.Get_User_ParentalRules(groupId, siteGuid);

                    // Check if the rule is defined for this user from the domain or the group level
                    ParentalRule defaultRule = parentalRules.FirstOrDefault(rule => (rule.id == (long)ruleId && rule.level != eRuleLevel.User));

                    if (parentalRules != null && defaultRule != null)
                    {
                        // Disbale = is active = 0, rule id = -1
                        setStatus = SetUserParentalRules(groupId, siteGuid, -1, 1, 0);

                        if (setStatus != null && setStatus.Code == 0)
                        {
                            setRuleResult = true;
                        }

                        // Now let's enable all other default rules - so they don't get away from us...
                        // There should be just one more additional - movies/tv series
                        if (parentalRules.Remove(defaultRule))
                        {
                            foreach (var rule in parentalRules)
                            {
                                // rule id = other default rule id, is active = 1
                                setStatus = SetUserParentalRules(groupId, siteGuid, rule.id, 1, 0);

                                if (setStatus != null && setStatus.Code == 0)
                                {
                                    setRuleResult &= true;
                                }
                            }
                        }
                    }
                    else
                    {
                        // if there is no other rule, we are fine and good to go
                        setRuleResult = (setStatus != null && setStatus.Code == 0);
                    }
                }
            }

            if (string.IsNullOrEmpty(pin))
            {
                setPinResult = true;
            }
            else
            {
                if (ruleId > 0)
                {
                    var pinStatus = SetParentalPIN(groupId, 0, siteGuid, pin, ruleId);

                    if (pinStatus != null && pinStatus.Code == 0)
                    {
                        setPinResult = true;
                    }
                }
                // ruleID 0 means it is purchase rule
                else
                {
                    var pinStatus = SetPurchasePIN(groupId, 0, siteGuid, pin);

                    if (pinStatus != null && pinStatus.Code == 0)
                    {
                        setPinResult = true;
                    }
                }
            }

            // Result is if both are successful 
            result = setRuleResult && setPinResult;

            return result;

            #region old code
            //DataTable dt = DAL.ApiDAL.Get_UserGroupRule(nGroupID, sSiteGUID, nRuleID);
            //if (dt != null)
            //{
            //    if (dt.DefaultView.Count > 0)
            //    {
            //        int m_nUserRuleID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "rule_id");
            //        int m_nIsActive = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "is_active");
            //        string m_sPIN = dt.Rows[0]["code"].ToString();

            //        return UpdateUserGroupRule(sSiteGUID, m_nUserRuleID, nStatus, true, sPIN);
            //    }
            //    else
            //    {
            //        return InsertNewUserGroupRule(sSiteGUID, nRuleID, nStatus, sPIN, nGroupID);
            //    }

            //}
            //return false; 
            #endregion
        }

        private static bool UpdateUserGroupRule(string sSiteGuid, int nUserRuleID, int nIsActive, bool bWithPIN, string sPIN)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_group_rules");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
            if (bWithPIN)
            {
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("code", "=", sPIN);
            }
            updateQuery += "WHERE";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGuid);
            updateQuery += "AND";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("rule_id", "=", nUserRuleID);
            updateQuery += "AND";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);

            bool ret = updateQuery.Execute();
            updateQuery.Finish();
            return ret;
        }

        private static bool InsertNewUserGroupRule(string sSiteGuid, int nRuleID, int nStatus, string sPIN, int nGroupID)
        {
            bool ret = false;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "SELECT id FROM groups_rules WHERE status=1 AND is_active=1 AND";
            selectQuery += "group_id " + PageUtils.GetAllGroupTreeStr(nGroupID);
            selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nRuleID);

            if (selectQuery.Execute("query", true) != null)
            {
                if (selectQuery.Table("query").DefaultView.Count > 0)
                {
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_group_rules");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "=", sSiteGuid);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rule_id", "=", nRuleID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("code", "=", sPIN);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nStatus);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                    ret = insertQuery.Execute();
                    insertQuery.Finish();
                }
            }
            selectQuery.Finish();
            return ret;
        }

        #endregion

        #region Domain Group Rules

        public static List<GroupRule> GetUserDomainGroupRules(int groupId, string siteGuid, int domainId)
        {
            List<GroupRule> groupRules = new List<GroupRule>();

            if (!string.IsNullOrEmpty(siteGuid))
            {
                // Get parental rule from new DAL method
                var parentalRules = DAL.ApiDAL.Get_User_ParentalRules(groupId, siteGuid);

                groupRules.AddRange(ConvertParentalToGroupRule(parentalRules));

                eRuleLevel ruleLevel = eRuleLevel.User;
                ePurchaeSettingsType type = ePurchaeSettingsType.Block;

                bool hasPurchaseSetting = DAL.ApiDAL.Get_PurchaseSettings(groupId, domainId, siteGuid, out ruleLevel, out type);

                // Create purchase rule if setting is ask or block (block = known backward compatibility issue)
                if (hasPurchaseSetting && (type == ePurchaeSettingsType.Ask || type == ePurchaeSettingsType.Block))
                {
                    GroupRule settingsRule = CreateSettingsGroupRule(type);

                    groupRules.Add(settingsRule);
                }
            }
            else
            {
                // Get parental rule from new DAL method
                var parentalRules = DAL.ApiDAL.Get_Domain_ParentalRules(groupId, domainId);

                groupRules.AddRange(ConvertParentalToGroupRule(parentalRules));

                eRuleLevel ruleLevel = eRuleLevel.User;
                ePurchaeSettingsType type = ePurchaeSettingsType.Block;

                bool hasPurchaseSetting = DAL.ApiDAL.Get_PurchaseSettings(groupId, domainId, "0", out ruleLevel, out type);

                // Create purchase rule if setting is ask or block (block = known backward compatibility issue)
                if (hasPurchaseSetting && (type == ePurchaeSettingsType.Ask || type == ePurchaeSettingsType.Block))
                {
                    GroupRule settingsRule = CreateSettingsGroupRule(type);

                    groupRules.Add(settingsRule);
                }
            }

            return groupRules;

            #region old code
            //Dictionary<int, GroupRule> groupRulesDict = new Dictionary<int, GroupRule>();

            //DataTable dtGroupRules = string.IsNullOrEmpty(sSiteGuid) ? DAL.ApiDAL.GetDomainGroupRules(nDomainID) : DAL.ApiDAL.Get_UserGroupRules(sSiteGuid);

            //if (dtGroupRules != null && dtGroupRules.Rows.Count > 0)
            //{
            //    foreach (DataRow drGroupRule in dtGroupRules.Rows)
            //    {
            //        GroupRule gr = new GroupRule();

            //        int nGroupRuleType = APILogic.Utils.GetIntSafeVal(drGroupRule, "group_rule_type_id");
            //        gr.RuleID = APILogic.Utils.GetIntSafeVal(drGroupRule, "rule_id");
            //        gr.IsActive = (APILogic.Utils.GetIntSafeVal(drGroupRule, "is_active") == 1);
            //        gr.Name = APILogic.Utils.GetSafeStr(drGroupRule, "name");
            //        gr.TagTypeID = APILogic.Utils.GetIntSafeVal(drGroupRule, "tag_type_id");
            //        gr.DynamicDataKey = APILogic.Utils.GetSafeStr(drGroupRule, "dynamic_data_key");
            //        gr.OrderNum = APILogic.Utils.GetIntSafeVal(drGroupRule, "order_num");
            //        gr.GroupRuleType = nGroupRuleType > 0 ? (eGroupRuleType)(nGroupRuleType) : eGroupRuleType.Parental;
            //        gr.AllTagValues = new List<string>();

            //        groupRulesDict.Add(gr.RuleID, gr);
            //    }
            //}

            //if (groupRulesDict.Count > 0)
            //{
            //    List<GroupRule> parentalGroupRules = groupRulesDict.Values.ToList().Where(x => x.GroupRuleType == eGroupRuleType.Parental).ToList();
            //    List<int> ruleIDsList = parentalGroupRules.Select(x => x.RuleID).ToList();
            //    DataTable dtGroupRulesTagsValues = DAL.ApiDAL.GetGroupRulesTagsValues(ruleIDsList);

            //    if (dtGroupRulesTagsValues != null)
            //    {
            //        foreach (DataRow drTagValue in dtGroupRulesTagsValues.Rows)
            //        {
            //            int ruleID = APILogic.Utils.GetIntSafeVal(drTagValue, "ruleID");
            //            string tagValue = APILogic.Utils.GetSafeStr(drTagValue, "Value");

            //            if (groupRulesDict.ContainsKey(ruleID) == true && groupRulesDict[ruleID] != null)
            //            {
            //                groupRulesDict[ruleID].AllTagValues.Add(tagValue);
            //            }
            //        }
            //    }
            //}

            //return groupRulesDict.Values.ToList(); 
            #endregion
        }

        private static GroupRule CreateSettingsGroupRule(ePurchaeSettingsType type)
        {
            GroupRule settingsRule = new GroupRule()
            {
                RuleID = (int)0,
                IsActive = true,
                Name = "Purchase",
                OrderNum = 0,
                GroupRuleType = eGroupRuleType.Purchase,
                BlockType = eBlockType.Validation
            };

            return settingsRule;
        }

        private static List<GroupRule> ConvertParentalToGroupRule(List<ParentalRule> parentalRules)
        {
            List<GroupRule> groupRules = new List<GroupRule>();

            foreach (var parentalRule in parentalRules)
            {
                if (parentalRule.mediaTagTypeId > 0)
                {
                    // Convert parental rule into group rule
                    GroupRule groupRule = new GroupRule()
                    {
                        RuleID = (int)parentalRule.id,
                        IsActive = true,
                        Name = parentalRule.name,
                        TagTypeID = parentalRule.mediaTagTypeId,
                        OrderNum = parentalRule.order,
                        GroupRuleType = eGroupRuleType.Parental,
                        AllTagValues = parentalRule.mediaTagValues,
                        BlockAnonymous = parentalRule.blockAnonymousAccess,
                        BlockType = eBlockType.Validation
                    };

                    groupRules.Add(groupRule);
                }

                if (parentalRule.epgTagTypeId > 0)
                {
                    // Convert parental rule into group rule
                    GroupRule groupRule = new GroupRule()
                    {
                        RuleID = (int)parentalRule.id,
                        IsActive = true,
                        Name = parentalRule.name,
                        TagTypeID = parentalRule.epgTagTypeId,
                        OrderNum = parentalRule.order,
                        GroupRuleType = eGroupRuleType.EPG,
                        AllTagValues = parentalRule.epgTagValues,
                        BlockAnonymous = parentalRule.blockAnonymousAccess,
                        BlockType = eBlockType.Validation
                    };

                    groupRules.Add(groupRule);
                }
            }

            return groupRules;
        }

        public static bool SetDomainGroupRule(int domainId, int ruleId, int ruleStatus, string pin, int groupId)
        {
            bool result = false;
            bool setRuleResult = false;
            bool setPinResult = false;
            int isActive = 0;

            if (ruleStatus == 1)
            {
                isActive = 1;
            }

            // ruleID 0 means it is purchase rule
            if (ruleId == 0)
            {
                ePurchaeSettingsType type = ePurchaeSettingsType.Allow;

                // activating - setting "ask". Disabling - setting "allow"
                if (isActive == 1)
                {
                    type = ePurchaeSettingsType.Ask;
                }

                var setStatus = SetPurchaseSettings(groupId, domainId, string.Empty, type);

                if (setStatus != null && setStatus.Code == 0)
                {
                    setRuleResult = true;
                }
            }
            // otherwise it is parental rule
            else
            {
                ApiObjects.Response.Status setStatus = null;

                setStatus = SetDomainParentalRules(groupId, domainId, ruleId, isActive);

                if (setStatus != null && setStatus.Code == 0)
                {
                    setRuleResult = true;
                }

                // If user wants to activate a rule - normal behavior
                if (isActive == 0)
                {
                    setRuleResult = false;

                    List<ParentalRule> parentalRules = DAL.ApiDAL.Get_Domain_ParentalRules(groupId, domainId);

                    // Check if the rule is defined for this user from the domain or the group level
                    if (parentalRules != null && parentalRules.FirstOrDefault(rule => (rule.id == (long)ruleId && rule.level == eRuleLevel.Group)) != null)
                    {
                        // Disbale = is active = 0, rule id = -1
                        setStatus = SetDomainParentalRules(groupId, domainId, -1, 1);

                        if (setStatus != null && setStatus.Code == 0)
                        {
                            setRuleResult = true;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(pin))
            {
                setPinResult = true;
            }
            else
            {
                if (ruleId > 0)
                {
                    var pinStatus = SetParentalPIN(groupId, domainId, string.Empty, pin, ruleId);

                    if (pinStatus != null && pinStatus.Code == 0)
                    {
                        setPinResult = true;
                    }
                }
                // ruleID 0 means it is purchase rule
                else
                {
                    var pinStatus = SetPurchasePIN(groupId, domainId, string.Empty, pin);

                    if (pinStatus != null && pinStatus.Code == 0)
                    {
                        setPinResult = true;
                    }
                }
            }

            // Result is if both are successful 
            result = setRuleResult && setPinResult;

            return result;

            #region old code
            //bool ret = false;
            //string[] dbRule = DAL.ApiDAL.GetDomainGroupRule(nGroupID, nDomainID, nRuleID);

            //if (dbRule != null && dbRule.Length > 0)
            //{
            //    int ruleID = APILogic.Utils.GetIntSafeVal(dbRule[0]);
            //    int isActive = APILogic.Utils.GetIntSafeVal(dbRule[1]);
            //    string pin = dbRule[2];

            //    ret = UpdateDomainGroupRule(nDomainID, ruleID, nStatus, true, sPIN);
            //}
            //else
            //{
            //    ret = InsertNewDomainGroupRule(nDomainID, nRuleID, nStatus, sPIN, nGroupID);
            //}

            //return ret; 
            #endregion
        }

        private static bool InsertNewDomainGroupRule(int nDomainID, int nRuleID, int nIsActive, string sPIN, int nGroupID)
        {
            string sInGroups = PageUtils.GetAllGroupTreeStr(nGroupID);
            bool ret = DAL.ApiDAL.InsertNewDomainGroupRule(sInGroups, nDomainID, nRuleID, sPIN, nIsActive, 1);
            return ret;
        }

        private static bool UpdateDomainGroupRule(int nDomainID, int nRuleID, int nIsActive, bool bWithPIN, string sPIN)
        {
            bool bUpdated = DAL.ApiDAL.UpdateDomainGroupRule(nDomainID, nRuleID, nIsActive, 1, bWithPIN, sPIN);
            return bUpdated;
        }

        #endregion

        static public GroupOperator[] GetGroupOperators(int nGroupID, string sScope = "")
        {
            DataTable dt = DAL.ApiDAL.Get_GroupOperatorsDetails(nGroupID);

            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {

                    return dt.Rows.OfType<DataRow>()
                        // GroupBy to select only operators     
                        .GroupBy(
                                 dr =>
                                 new
                                     {
                                         ID = dr.Field<int>("ID"),
                                         Name = dr.Field<string>("Name"),
                                         Type = dr.Field<int>("Type"),
                                         CoGuid = dr.Field<string>("Client_Id"),
                                         LoginUrl = dr.Field<string>("Operator_Login"),
                                         LogoutUrl = dr.Field<string>("Operator_Login")
                                     })

                             .Select(dr => new GroupOperator()
                             {
                                 ID = dr.Key.ID,
                                 Name = dr.Key.Name,
                                 Type = (eOperatorType)dr.Key.Type,
                                 CoGuid = dr.Key.CoGuid,
                                 LoginUrl =

                                     string.IsNullOrEmpty(sScope) ?

                                     dr.Key.LoginUrl
                                     :

                                     // if provider has scopes:
                                     dt.Select("ID=" + dr.Key.ID + " AND SCOPE='" + sScope + "'").Any() ?

                                     // True: Set the LoginUrl to the scope's LoginUrl
                                         dt.Select("ID=" + dr.Key.ID + " AND SCOPE='" + sScope + "'").Select(r => r["Scope_Login"].ToString()).FirstOrDefault()
                                         :

                                     // False: set the LoginUrl to the provider's LoginUrl
                                         dr.Key.LoginUrl,
                                 LogoutURL = string.IsNullOrEmpty(sScope) ?

                                     dr.Key.LogoutUrl
                                     :

                                     // if provider has scopes:
                                     dt.Select("ID=" + dr.Key.ID + " AND SCOPE='" + sScope + "'").Any() ?

                                     // True: Set the LoginUrl to the scope's LoginUrl
                                         dt.Select("ID=" + dr.Key.ID + " AND SCOPE='" + sScope + "'").Select(r => r["Scope_Logout"].ToString()).FirstOrDefault()
                                         :

                                     // False: set the LoginUrl to the provider's LoginUrl
                                         dr.Key.LogoutUrl,
                                 Scopes =
                                           string.IsNullOrEmpty(sScope) ?

                                               dt.Select("ID=" + dr.Key.ID).Select(r => new Scope
                                               {
                                                   Name = r["Scope"].ToString(),
                                                   LoginUrl = r["Scope_Login"].ToString()
                                               }
                                                                                   ).ToArray()
                                               :

                                               null

                             })
                        .ToArray();

                }
                else return new GroupOperator[0];
            }
            else
            {
                return null;
            }
        }

        public static bool CheckParentalPIN(string siteGuid, int ruleId, string parentalPIN, int groupId)
        {
            bool result = false;

            ApiObjects.Response.Status status = null;

            // rule id 0 is purchase rule
            if (ruleId == 0)
            {
                //eRuleLevel level = eRuleLevel.User;
                //ePurchaeSettingsType type;
                //string pin;

                //bool success = DAL.ApiDAL.Get_PurchasePin(groupId, 0, siteGuid, out level, out pin, true);

                //if (success)
                //{
                //    // If user has no pin defined - compare the given pin to the group's default PARENTAL pin
                //    if (level == eRuleLevel.Group)
                //    {
                //        string defaultPin = DAL.ApiDAL.Get_Group_DefaultPIN(groupId, eGroupRuleType.Purchase);

                //        if (defaultPin == parentalPIN)
                //        {
                //            result = true;
                //        }
                //    }
                //    // If user has pin defined - compare given pin to the defined pin
                //    else if (!string.IsNullOrEmpty(parentalPIN) && parentalPIN == pin)
                //    {
                //        result = true;
                //    }
                //}

                status = ValidatePurchasePIN(groupId, siteGuid, parentalPIN, 0);
            }
            else
            {
                status = ValidateParentalPIN(groupId, siteGuid, parentalPIN, 0, ruleId);
            }

            if (status != null && status.Code == 0)
            {
                result = true;
            }

            return result;

            #region old code
            //bool retVal = false;

            //DataTable dt = DAL.ApiDAL.Get_CodeForParentalPIN(sSiteGUID, nRuleID);

            //if (dt != null)
            //{
            //    if (dt.DefaultView.Count > 0)
            //    {
            //        if (dt.Rows[0]["code"].ToString() == sParentalPIN)
            //        {
            //            retVal = true;
            //        }
            //    }
            //}
            //return retVal; 
            #endregion
        }

        public static bool CheckDomainParentalPIN(int domainID, int ruleID, string parentalPIN, int groupId)
        {
            return CheckParentalPIN(string.Empty, domainID, ruleID, parentalPIN, groupId);
        }

        public static bool CheckParentalPIN(string siteGUID, int domainID, int ruleID, string parentalPIN, int groupId)
        {
            bool result = false;

            ApiObjects.Response.Status status = null;

            // rule id 0 is purchase rule
            if (ruleID == 0)
            {
                //eRuleLevel level = eRuleLevel.User;
                //ePurchaeSettingsType type;
                //string pin;

                //bool success = DAL.ApiDAL.Get_PurchasePin(groupId, domainID, siteGUID, out level, out pin, false);

                //if (success)
                //{
                //    // If user has no pin defined - compare the given pin to the group's default PARENTAL pin
                //    if (level == eRuleLevel.Group)
                //    {
                //        string defaultPin = DAL.ApiDAL.Get_Group_DefaultPIN(groupId);

                //        if (defaultPin == parentalPIN)
                //        {
                //            result = true;
                //        }
                //    }
                //    // If user has pin defined - compare given pin to the defined pin
                //    else if (!string.IsNullOrEmpty(parentalPIN) && parentalPIN == pin)
                //    {
                //        result = true;
                //    }
                //}

                status = ValidatePurchasePIN(groupId, siteGUID, parentalPIN, domainID);
            }
            else
            {
                status = ValidateParentalPIN(groupId, siteGUID, parentalPIN, domainID, ruleID);
            }

            if (status != null && status.Code == 0)
            {
                result = true;
            }

            return result;
            #region old code
            //bool retVal = false;

            //if (!string.IsNullOrEmpty(sSiteGUID))
            //{
            //    retVal = CheckParentalPIN(sSiteGUID, nRuleID, sParentalPIN);
            //}
            //else
            //{
            //    string dbPIN = DAL.ApiDAL.GetDomainCodeForParentalPIN(nDomainID, nRuleID);
            //    retVal = (string.Compare(dbPIN, sParentalPIN, false) == 0);
            //}

            //return retVal; 
            #endregion
        }


        public static bool SetDefaultRules(string sSiteGuid, int nGroupID)
        {
            // Because of new parental rules, this logic is no longer needed.
            return true;

            #region old code
            //DataTable dt = DAL.ApiDAL.Get_DefaultRules(nGroupID);
            //if (dt != null)
            //{
            //    if (dt.DefaultView.Count > 0)
            //    {
            //        foreach (DataRow dr in dt.Rows)
            //        {
            //            bool bSuccsess = SetUserGroupRule(sSiteGuid, APILogic.Utils.GetIntSafeVal(dr, "ID"), APILogic.Utils.GetIntSafeVal(dr, "default_enabled"), dr["default_val"].ToString(), nGroupID);
            //            if (!bSuccsess)
            //            {
            //                return false;
            //            }
            //        }
            //        return true;
            //    }
            //    else return true;
            //}
            //else return false; 
            #endregion
        }

        public static bool SetRuleState(string siteGuid, int domainId, int ruleId, int status, int groupId)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(siteGuid))
            {
                result = SetUserGroupRule(siteGuid, ruleId, status, string.Empty, groupId);
            }
            else
            {
                result = SetDomainGroupRule(domainId, ruleId, status, string.Empty, groupId);
            }

            return result;

            #region old code
            //if (!string.IsNullOrEmpty(sSiteGuid))
            //{
            //    return UpdateUserGroupRule(sSiteGuid, nRuleID, nStatus, false, string.Empty);
            //}
            //else
            //{
            //    return UpdateDomainGroupRule(nDomainID, nRuleID, nStatus, false, string.Empty);
            //} 
            #endregion
        }

        public static DeviceAvailabiltyRule GetAvailableDevices(int nMediaID, int nGroupID)
        {
            DeviceAvailabiltyRule retVal = new DeviceAvailabiltyRule();

            DataTable dt = DAL.ApiDAL.Get_AvailableDevices(nMediaID, nGroupID);
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    retVal.ID = APILogic.Utils.GetIntSafeVal(dt.Rows[0], "Rule_ID");
                    retVal.Name = APILogic.Utils.GetSafeStr(dt.Rows[0], "Rule_Name");

                    retVal.AvailableDevices = dt.AsEnumerable()
                             .Select(dr => new DeviceObject()
                             {
                                 ID = APILogic.Utils.GetIntSafeVal(dr, "ID"),
                                 Name = APILogic.Utils.GetSafeStr(dr, "Name"),
                                 FamilyID = APILogic.Utils.GetIntSafeVal(dr, "Family_ID"),
                                 FamilyName = APILogic.Utils.GetSafeStr(dr, "Family_Name")
                             }
                                     ).ToList();
                }
            }
            return retVal;
        }
        /// <summary>
        /// Check Geo Commerce subscription
        /// </summary>
        /// <param name="nGroupID">set group ID</param>
        /// <param name="SubscriptionRuleID">set subscriiption geo commerce rule ID</param>
        /// <param name="sIP">set client IP</param>
        /// <returns>return true if the enable purchase subscription in spasfic country by IP</returns>
        public static bool IsGeoCommerceBlock(int nGroupID, int SubscriptionGeoCommerceID, string sIP)
        {
            bool GeoCommersRes = false;
            if (SubscriptionGeoCommerceID != 0)
            {
                //convert IP to country ID
                Country country = GetCountryByIp(nGroupID, sIP);
                Int32 nCountryID = country != null ? country.Id : 0;
                string sGroupID = PageUtils.GetAllGroupTreeStr(nGroupID);
                log.Debug("Geo Commerce - Geo Commerce ID " + SubscriptionGeoCommerceID + " Country ID " + nCountryID);
                //define the logic roul only spasfic country or no one except.
                // true : No one except the below selections
                // false : Every body except the below selection
                bool rule_ONLY_OR_BUT = true;

                //get the ONLY_OR_BUT value from geo_block_types table by Subscription Geo Commerce ID
                //GEO_RULE_TYPE = 3 only for geo commerce.

                #region Get geo commerce ONLY_OR_BUT value
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "Select ONLY_OR_BUT from geo_block_types where IS_ACTIVE=1 and STATUS=1 and GEO_RULE_TYPE=3 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", SubscriptionGeoCommerceID);
                selectQuery += " and group_id " + sGroupID;

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        object oBTID = selectQuery.Table("query").DefaultView[0].Row["ONLY_OR_BUT"];
                        if (oBTID != null && !string.IsNullOrEmpty(oBTID.ToString()))
                        {
                            rule_ONLY_OR_BUT = oBTID.ToString() == "1" ? true : false;
                        }
                    }
                }
                #endregion

                //declare member exist country in rule
                bool bExsitInRuleM2M = false;
                //is geo block type include country
                bExsitInRuleM2M = PageUtils.DoesGeoBlockTypeIncludeCountry(SubscriptionGeoCommerceID, nCountryID);

                //retun true if: 
                //No one except except this country (true && true)
                //every body except this country (false && false)
                if ((rule_ONLY_OR_BUT && bExsitInRuleM2M) || (!rule_ONLY_OR_BUT && !bExsitInRuleM2M))
                {
                    GeoCommersRes = true;
                }
                else
                {
                    GeoCommersRes = false;
                }
            }

            return GeoCommersRes;
        }

        public static ApiObjects.Response.Status CleanUserHistory(int nGroupID, string siteGuid, List<int> lMediaIDs)
        {
            try
            {
                if (string.IsNullOrEmpty(siteGuid))
                {
                    log.Debug("CleanUserHistory - siteGuid = " + siteGuid);
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                }

                if (lMediaIDs == null || lMediaIDs.Count == 0)
                {
                    lMediaIDs = new List<int>();
                }

                var succeeded = DAL.ApiDAL.CleanUserHistory(siteGuid, lMediaIDs);
                if (succeeded)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.OK, "Ok");
                }
                else
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                log.Error("CleanUserHistory - Error = " + ex.Message, ex);
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
            }
        }

        public static Scheduling GetProgramSchedule(int nProgramId, int nGroupID)
        {
            //call catalog service for details 
            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = GetWSURL("CatalogSignatureKey");

            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            EpgProgramDetailsRequest request = new EpgProgramDetailsRequest();
            request.m_lProgramsIds = new List<int> { nProgramId };
            request.m_nGroupID = nGroupID;
            request.m_nPageIndex = 0;
            request.m_nPageSize = 0;
            request.m_sSignString = sSignString;
            request.m_sSignature = sSignature;

            EpgProgramResponse response = (EpgProgramResponse)request.GetProgramsByIDs(request);
            Scheduling scheduling = null;
            if (response != null && response.m_nTotalItems > 0)
            {
                ProgramObj programObj = response.m_lObj[0] as ProgramObj;
                DateTime startTime = ODBCWrapper.Utils.GetDateSafeVal(programObj.m_oProgram.START_DATE);
                DateTime endTime = ODBCWrapper.Utils.GetDateSafeVal(programObj.m_oProgram.END_DATE);
                try
                {
                    startTime = DateTime.ParseExact(programObj.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    endTime = DateTime.ParseExact(programObj.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    log.Error("GetProgramSchedule - " + string.Format("fail to convert datetime string to DateTime ex={0}", ex.Message), ex);
                }
                scheduling = new Scheduling(startTime, endTime);
            }

            return scheduling;
        }

        public static string GetCoGuidByMediaFileId(int nMediaFileID)
        {
            string sCoGuid = null;

            DataTable dt = DAL.ApiDAL.GetCoGuidByMediaFileId(nMediaFileID);

            if (dt != null && dt.Rows.Count > 0)
            {
                if (dt.Rows[0] != null)
                {
                    sCoGuid = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["CO_GUID"]);
                }
            }

            return sCoGuid;
        }

        public static List<string> GetUserStartedWatchingMedias(string siteGuid, int numOfItems, int groupId)
        {
            List<string> medias = new List<string>();

            // build request
            WatchHistoryRequest request = new WatchHistoryRequest()
            {
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupId,
                m_nPageIndex = 0,
                m_nPageSize = numOfItems,
                AssetTypes = null,
                FilterStatus = eWatchStatus.Progress,
                NumOfDays = 30,
                OrderDir = ApiObjects.SearchObjects.OrderDir.DESC
            };

            WatchHistoryResponse response = request.GetResponse(request) as WatchHistoryResponse;

            if (response != null && response.result != null)
                medias = response.result.Select(x => x.AssetId).ToList<string>();

            return medias;
        }

        public static bool DoesMediaBelongToBundle(int nBundleCode, int[] nFileTypeIDs, int nMediaID, string sDevice, int nGroupID, Btype bType)
        {
            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = GetWSURL("CatalogSignatureKey");

            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            try
            {
                BundleContainingMediaRequest bundleMediaRequest = new BundleContainingMediaRequest();
                switch (bType)
                {
                    case Btype.SUBSCRIPTION:
                        {
                            bundleMediaRequest.m_eBundleType = eBundleType.SUBSCRIPTION;
                            break;
                        }
                    case Btype.COLLECTION:
                        {
                            bundleMediaRequest.m_eBundleType = eBundleType.COLLECTION;
                            break;
                        }
                }


                bundleMediaRequest.m_nPageIndex = 0;
                bundleMediaRequest.m_nPageSize = 0;
                bundleMediaRequest.m_nGroupID = nGroupID;
                bundleMediaRequest.m_nBundleID = nBundleCode;
                bundleMediaRequest.m_nMediaID = nMediaID;
                bundleMediaRequest.m_sSignString = sSignString;
                bundleMediaRequest.m_sSignature = sSignature;
                bundleMediaRequest.m_oFilter = new Filter();
                bundleMediaRequest.m_oFilter.m_bOnlyActiveMedia = true;
                bundleMediaRequest.m_oFilter.m_bUseStartDate = true;
                bundleMediaRequest.m_oFilter.m_sDeviceId = sDevice;
                bundleMediaRequest.m_sMediaType = "0";

                ContainingMediaResponse response = (ContainingMediaResponse)bundleMediaRequest.GetResponse(bundleMediaRequest);

                bool isMediaInBundle = false;

                if (response != null)
                {
                    isMediaInBundle = response.m_bContainsMedia;
                }
                return isMediaInBundle;
            }
            catch (Exception ex)
            {
                log.Error("Get Bundle Media Ids - Failed to get bundle " + nBundleCode + " from group " + nGroupID, ex);
                return false;
            }
        }

        public static List<int> GetBundleMediaIds(int nBundleCode, int[] nFileTypeIDs, string sDevice, int nGroupID, Btype btype)
        {
            List<int> nMedias = new List<int>();

            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = GetWSURL("CatalogSignatureKey");

            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            try
            {
                BundleMediaRequest bundleMediaRequest = new BundleMediaRequest();
                switch (btype)
                {
                    case Btype.SUBSCRIPTION:
                        {
                            bundleMediaRequest.m_eBundleType = eBundleType.SUBSCRIPTION;
                            break;
                        }
                    case Btype.COLLECTION:
                        {
                            bundleMediaRequest.m_eBundleType = eBundleType.COLLECTION;
                            break;
                        }
                }

                bundleMediaRequest.m_nPageIndex = 0;
                bundleMediaRequest.m_nPageSize = 0;
                bundleMediaRequest.m_nGroupID = nGroupID;
                bundleMediaRequest.m_nBundleID = nBundleCode;
                bundleMediaRequest.m_sSignString = sSignString;
                bundleMediaRequest.m_sSignature = sSignature;
                bundleMediaRequest.m_oFilter = new Filter();
                bundleMediaRequest.m_oFilter.m_bOnlyActiveMedia = true;
                bundleMediaRequest.m_oFilter.m_bUseStartDate = true;
                bundleMediaRequest.m_oFilter.m_sDeviceId = sDevice;
                bundleMediaRequest.m_sMediaType = "0";

                MediaIdsResponse response = (MediaIdsResponse)bundleMediaRequest.GetResponse(bundleMediaRequest);
                SearchResult[] medias = response.m_nMediaIds.ToArray();


                nMedias = APILogic.Utils.ConvertMediaResultObjectIDsToIntArray(medias);
                if (nMedias != null && nMedias.Count > 0)
                {
                    nMedias = nMedias.Distinct().ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error("Get Bundle Media Ids - Failed to get bundle " + nBundleCode + " from group " + nGroupID, ex);
            }

            return nMedias;
        }

        public static List<GroupRule> GetEPGProgramRules(int programId, int channelMediaId, string siteGuid, string ip, int groupId, string deviceUdid)
        {
            List<GroupRule> groupRules = new List<GroupRule>();

            //Check if geo-block applies
            string ruleName;
            if (CheckGeoBlockMedia(groupId, (int)channelMediaId, ip, out ruleName))
            {
                groupRules.Add(new GroupRule()
                {
                    Name = "GeoBlock",
                    IsActive = true,
                    BlockType = eBlockType.Geo,

                });
            }

            //Check if user type match media user types
            if (!string.IsNullOrEmpty(siteGuid) && CheckMediaUserType((int)channelMediaId, int.Parse(siteGuid), groupId) == false)
            {
                groupRules.Add(new GroupRule()
                {
                    RuleID = 0,
                    Name = "UserTypeBlock",
                    BlockType = eBlockType.UserType,
                    IsActive = true
                });
            }

            var response = GetParentalEPGRules(groupId, siteGuid, programId, 0);

            if (response != null && response.status != null && response.status.Code == 0)
            {
                groupRules.AddRange(ConvertParentalToGroupRule(response.rules));
            }

            return groupRules;

            #region old code
            //List<GroupRule> tempRules = getEpgUserRules(nProgramId, nGroupId, nSiteGuid);
            //List<GroupRule> resultRules = GetRules(eGroupRuleType.EPG, nSiteGuid, nMediaId, nGroupId, sIP, tempRules);
            //return resultRules; 
            #endregion
        }

        public static List<GroupRule> GetNpvrRules(RecordedEPGChannelProgrammeObject recordedProgram, int siteGuid, string ip, int groupId, string deviceUdid)
        {
            int mediaGroupId = 0;
            int mediaId = 0;
            List<GroupRule> groupRules = new List<GroupRule>();

            // get media ID and non parent group ID
            DataSet mediaIdResultDataSet = Tvinci.Core.DAL.CatalogDAL.GetMediaByEpgChannelIds(groupId, new List<string>() { recordedProgram.EPG_CHANNEL_ID });

            if (mediaIdResultDataSet != null &&
                mediaIdResultDataSet.Tables != null &&
                mediaIdResultDataSet.Tables.Count > 0 &&
                mediaIdResultDataSet.Tables[0].Rows.Count > 0)
            {
                mediaGroupId = APILogic.Utils.GetIntSafeVal(mediaIdResultDataSet.Tables[0].Rows[0], "GROUP_ID");
                mediaId = APILogic.Utils.GetIntSafeVal(mediaIdResultDataSet.Tables[0].Rows[0], "id");

                if (mediaId != 0)
                {
                    // get EPG rules
                    List<GroupRule> tempRules = getEpgUserRules(recordedProgram, groupId, siteGuid, mediaGroupId);

                    // combine user with EPG rules
                    groupRules = GetRules(eGroupRuleType.EPG, siteGuid, mediaId, groupId, ip, tempRules);
                }
            }

            if (mediaId == 0)
            {
                log.Debug("GetEPGRecordedProgramRules - " +
                    string.Format("Failed to retrieve media ID using EPG ID. group ID: {0}, siteguid: {1}, EPG ID: {2}", groupId, siteGuid, recordedProgram.EPG_IDENTIFIER));
            }

            return groupRules;
        }

        //get the epg rules that are relevant for the user and for the program
        private static List<GroupRule> getEpgUserRules(int nProgramId, int nGroupID, int nSiteGuid)
        {
            List<GroupRule> epgRules = new List<GroupRule>();
            int nParentGroupID = DAL.UtilsDal.GetParentGroupID(nGroupID);
            TvinciEpgBL epgBLTvinci = new TvinciEpgBL(nParentGroupID);  //assuming this is a Tvinci user - does not support yes Epg
            EpgCB epg = epgBLTvinci.GetEpgCB((ulong)nProgramId);
            if (epg != null)
            {
                int nGroupIDNonParent = epg.GroupID;
                List<GroupRule> userRules = getAllEpgUserRules(nSiteGuid, nGroupIDNonParent);
                //add only rules for tags that are tags of the specific EPG
                foreach (GroupRule rule in userRules)
                {
                    string TagKey = rule.TagType.ToLower();
                    if (epg.Tags.ContainsKey(TagKey))
                    {
                        if (epg.Tags[TagKey].Contains(rule.TagValue))
                            epgRules.Add(rule);
                    }
                }
            }
            return epgRules;
        }

        //get the EPG rules that are relevant for the user and for the program
        private static List<GroupRule> getEpgUserRules(RecordedEPGChannelProgrammeObject recordedProgram, int groupId, int siteGuid, int nonParentGroupId)
        {
            List<GroupRule> epgRules = new List<GroupRule>();
            int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupId);
            List<GroupRule> userRules = getAllEpgUserRules(siteGuid, nonParentGroupId);

            //add only rules for tags that are tags of the specific EPG           
            foreach (GroupRule rule in userRules)
            {
                if (rule != null)
                {
                    var ruleTag = recordedProgram.EPG_TAGS.Where(x => x.Key.ToLower() == rule.TagType.ToLower());
                    foreach (EPGDictionary epgItem in ruleTag)
                    {
                        if (!string.IsNullOrEmpty(epgItem.Key))
                        {
                            if (epgItem.Value.ToLower() == rule.TagValue.ToLower())
                                epgRules.Add(rule);
                        }
                    }
                }
            }
            return epgRules;
        }

        //get All epg rules that are relevant for the user
        private static List<GroupRule> getAllEpgUserRules(int nSiteGuid, int nGroupID)
        {
            DataTable rulesDt = DAL.ApiDAL.Get_EPGRules(nSiteGuid.ToString(), nGroupID);
            List<GroupRule> userRules = new List<GroupRule>();
            GroupRule rule = new GroupRule();
            if (rulesDt != null)
            {
                if (rulesDt.Rows != null && rulesDt.Rows.Count > 0)
                {
                    for (int i = 0; i < rulesDt.Rows.Count; i++)
                    {
                        int nRuleID = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "rule_id");
                        int nTagTypeID = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "TAG_TYPE_ID");
                        string sValue = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "VALUE");
                        string sKey = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "Key");
                        string sName = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "Name");
                        object sAgeRestriction = rulesDt.Rows[i]["age_restriction"];
                        int nIsActive = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "is_active");
                        eGroupRuleType eRuleType = (eGroupRuleType)APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "group_rule_type_id");
                        int nBlockAnonymous = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "block_anonymous");
                        bool bBlockAnonymous = (nBlockAnonymous == 1);
                        string tagType = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "tagName");
                        rule = new GroupRule(nRuleID, nTagTypeID, sValue, sKey, sName, sAgeRestriction, nIsActive, eRuleType, bBlockAnonymous, tagType);
                        userRules.Add(rule);
                    }
                }
            }
            return userRules;
        }

        //get a Dictionary of the tag ID and its type 
        private static Dictionary<int, string> getTagTypes(int nGroupID)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select ID, name from EPG_tags_types where status=1 and ";
            selectQuery += "group_id ";
            selectQuery += PageUtils.GetAllGroupTreeStr(nGroupID);

            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    object oName = selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                    if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                        result.Add(int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString()), oName.ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return result;
        }

        private static List<GroupRule> GetRules(eGroupRuleType eRuleType, int nSiteGuid, int nMediaId, int nGroupID, string sIP, List<GroupRule> tempRules)
        {
            List<GroupRule> rules = new List<GroupRule>();

            //Check if geo-block applies
            string ruleName;
            if (CheckGeoBlockMedia(nGroupID, nMediaId, sIP, out ruleName))
            {
                rules.Add(new GroupRule() { Name = "GeoBlock", BlockType = eBlockType.Geo });
            }

            //Check if user type match media user types
            if (nSiteGuid > 0 && CheckMediaUserType(nMediaId, nSiteGuid, nGroupID) == false)
            {
                rules.Add(new GroupRule() { Name = "UserTypeBlock", BlockType = eBlockType.UserType });
            }

            if (tempRules != null && tempRules.Count > 0)
            {
                foreach (GroupRule rule in tempRules)
                {
                    if (rule.GroupRuleType == eRuleType && rule.BlockType != eBlockType.Geo)
                    {
                        if (nSiteGuid > 0)
                        {
                            if (rule.AgeRestriction > 0 && !CheckAgeValidation(rule.AgeRestriction, nSiteGuid)) //check for active????
                            {
                                rule.BlockType = eBlockType.AgeBlock;
                                rules.Add(rule);
                            }
                            else
                            {
                                if (rule.IsActive)
                                {
                                    rule.BlockType = eBlockType.Validation;
                                    rules.Add(rule);
                                }
                            }
                        }
                        else //check for anonymous Rules
                        {
                            if (rule.BlockAnonymous && rule.IsActive)
                            {
                                rule.BlockType = eBlockType.AnonymousAccessBlock;
                                rules.Add(rule);
                            }
                        }
                    }
                }
            }

            return rules;
        }

        public static List<int> ChannelsContainingMedia(List<int> lChannels, int nMediaID, int nGroupID, int nFileTypeID)
        {
            try
            {
                Filter oFilter = new Filter();
                oFilter.m_bOnlyActiveMedia = false;
                oFilter.m_bUseFinalDate = true;
                oFilter.m_bUseStartDate = true;
                oFilter.m_nLanguage = DAL.UtilsDal.GetLangGroupID(nGroupID);
                oFilter.m_sDeviceId = "";
                oFilter.m_sPlatform = "";

                string sSignString = Guid.NewGuid().ToString();
                string sSignatureString = GetWSURL("CatalogSignatureKey");
                string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);


                //Call Catalog to create search object 
                ChannelsContainingMediaRequest subRrequest = new ChannelsContainingMediaRequest();
                subRrequest.m_oFilter = oFilter;
                subRrequest.m_sSignString = sSignString;
                subRrequest.m_sSignature = sSignature;
                subRrequest.m_nGroupID = nGroupID;
                subRrequest.m_nMediaID = nMediaID;
                subRrequest.m_lChannles = lChannels;


                ChannelsContainingMediaResponse response = (ChannelsContainingMediaResponse)subRrequest.GetResponse(subRrequest);
                return response.m_lChannellList.ToList<int>();
            }
            catch (Exception ex)
            {
                log.Error("ChannelsContainingMedia - Failed  due ex = " + ex.Message, ex);
            }

            return null;
        }

        public static List<int> GetMediaChannels(int nGroupID, int nMediaID)
        {
            List<int> lChannels = new List<int>();

            try
            {
                string sSignString = Guid.NewGuid().ToString();
                string sSignatureString = APILogic.Utils.GetWSUrl("CatalogSignatureKey");

                string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

                MediaChannelsRequest oMediaChannelReq = new MediaChannelsRequest()
                {
                    m_nGroupID = nGroupID,
                    m_nMediaID = nMediaID,
                    m_sSignString = sSignString,
                    m_sSignature = sSignature
                };

                MediaChannelsResponse response = oMediaChannelReq.GetResponse(oMediaChannelReq) as MediaChannelsResponse;

                if (response != null && response.m_nChannelIDs != null && response.m_nChannelIDs.Count > 0)
                {
                    lChannels.AddRange(response.m_nChannelIDs);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Exception thrown in GetMediaChannels. Ex={0}", ex.Message), ex);
            }

            return lChannels;
        }

        public static EPGChannelProgrammeObject GetProgramDetails(int nProgramId, int nGroupID)
        {
            //call catalog service for details 
            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = GetWSURL("CatalogSignatureKey");

            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            EpgProgramDetailsRequest request = new EpgProgramDetailsRequest();
            request.m_lProgramsIds = new List<int> { nProgramId };
            request.m_nGroupID = nGroupID;
            request.m_nPageIndex = 0;
            request.m_nPageSize = 0;
            request.m_sSignature = sSignature;
            request.m_sSignString = sSignString;

            EpgProgramResponse response = (EpgProgramResponse)request.GetProgramsByIDs(request);
            EPGChannelProgrammeObject epg = null;
            if (response != null && response.m_nTotalItems > 0)
            {
                if (response.m_lObj[0] != null)
                {
                    ProgramObj programObj = response.m_lObj[0] as ProgramObj;
                    epg = programObj.m_oProgram;
                }
            }
            return epg;
        }

        internal static Tuple<List<int>, bool> Get_MCRulesIdsByMediaId(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<int> ruleIds = new List<int>();
            try
            {
                if (funcParams != null && funcParams.Count == 3)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("mediaId") && funcParams.ContainsKey("mcr"))
                    {
                        int? groupId, mediaId;
                        List<MediaConcurrencyRule> mcr = new List<MediaConcurrencyRule>();
                        UnifiedSearchResult[] medias;
                        string filter = string.Empty;

                        groupId = funcParams["groupId"] as int?;
                        mediaId = funcParams["mediaId"] as int?;
                        mcr = funcParams["mcr"] as List<MediaConcurrencyRule>;

                        if (groupId.HasValue && mediaId.HasValue && mcr != null && mcr.Count > 0)
                        {
                            // find all asset ids that match the tag + tag value ==> if so save the rule id
                            //build serach for each tag and tag values

                            List<string> tempFilter = new List<string>();


                            mcr = mcr.GroupBy(x => x.RuleID).Select(x => x.First()).ToList();
                            Parallel.ForEach(mcr, (rule) =>
                            {
                                tempFilter = rule.AllTagValues.Select(x => string.Format("{0}='{1}'", rule.TagType, x)).ToList();
                                if (tempFilter != null && tempFilter.Count > 0)
                                {
                                    if (tempFilter.Count > 1)
                                    {
                                        filter = string.Format("(and media_id = '{0}' (or {1}))", mediaId.Value, string.Join(" ", tempFilter));
                                    }
                                    else
                                    {
                                        filter = string.Format("(and media_id = '{0}' {1})", mediaId.Value, tempFilter.First());
                                    }
                                    medias = SearchAssets(groupId.Value, filter, 0, 0, true, 0, true, string.Empty, string.Empty, string.Empty, 0, 0, true);
                                    if (medias != null && medias.Count() > 0)// there is a match 
                                    {
                                        ruleIds.Add(rule.RuleID);
                                    }
                                }
                            });

                            res = true;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("Get_MCRulesIdsByMediaId faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<List<int>, bool>(ruleIds.Distinct().ToList(), res);
        }

        /***************************************************************************************************************************
         * 
         This methode get mediaID and business Module id and return list of MediaConcurrencyRule rules that relevant to this media
         * 
         ***************************************************************************************************************************/
        public static List<MediaConcurrencyRule> GetMediaConcurrencyRules(int mediaId, string sIP, int groupId, int bmID, eBusinessModule type)
        {
            List<MediaConcurrencyRule> res = new List<MediaConcurrencyRule>();
            //MediaConcurrencyRule rule = null;
            try
            {
                // check if media concurrency rule exists for group
                string key = LayeredCacheKeys.GetGroupMediaConcurrencyRulesKey(groupId);
                List<MediaConcurrencyRule> mcr = null;
                List<int> ruleIds = null;
                // try to get from cache  

                bool cacheResult = LayeredCache.Instance.Get<List<MediaConcurrencyRule>>(key, ref mcr, APILogic.Utils.Get_MCRulesByGroup, new Dictionary<string, object>() { { "groupId", groupId } },
                                                                                        groupId, LayeredCacheConfigNames.MEDIA_CONCURRENCY_RULES_LAYERED_CACHE_CONFIG_NAME);
                if (!cacheResult)
                {
                    log.Error(string.Format("GetMediaConcurrencyRules - Failed get data from cache groupId = {0}", groupId));
                    return null;
                }

                // get all related rules to media 
                key = LayeredCacheKeys.GetMediaConcurrencyRulesKey(mediaId);
                cacheResult = LayeredCache.Instance.Get<List<int>>(key, ref ruleIds, Get_MCRulesIdsByMediaId, new Dictionary<string, object>() { { "groupId", groupId }, { "mediaId", mediaId }, { "mcr", mcr } },
                                                                    groupId, LayeredCacheConfigNames.MEDIA_CONCURRENCY_RULES_LAYERED_CACHE_CONFIG_NAME,
                                                                    new List<string>() { LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaId) });
                if (!cacheResult)
                {
                    log.Error(string.Format("GetMediaConcurrencyRules - Failed get data from cache groupId={0}, mediaId={1}", groupId, mediaId));
                    return null;
                }
                else if (ruleIds != null && ruleIds.Count > 0)
                {
                    res = mcr.Where(x => ruleIds.Contains(x.RuleID) && x.bmId == bmID && x.Type == type).ToList();
                }
                return res;
            }
            catch (Exception ex)
            {
                log.Error("GetMediaConcurrencyRules - Failed  due ex = " + ex.Message, ex);
                return null;
            }
        }

        private static Dictionary<string, List<string>> GetMediaTags(int nMediaID, int nGroupID)
        {
            Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();
            try
            {
                MediasProtocolRequest request = new MediasProtocolRequest();

                request.m_nGroupID = nGroupID;
                request.m_lMediasIds = new List<int> { nMediaID };
                request.m_oFilter = new Filter();
                request.m_oFilter.m_bOnlyActiveMedia = true;

                string sSignString = Guid.NewGuid().ToString();
                string sSignatureString = GetWSURL("CatalogSignatureKey");
                string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

                request.m_sSignature = sSignature;
                request.m_sSignString = sSignString;

                MediaResponse mediaResponse = (MediaResponse)request.GetMediasByIDs(request);

                if (mediaResponse != null && mediaResponse.m_nTotalItems > 0 && mediaResponse.m_lObj != null)
                {
                    if (mediaResponse.m_lObj[0] != null)
                    {
                        MediaObj mediaObj = mediaResponse.m_lObj[0] as MediaObj;

                        foreach (Tags catalogTag in mediaObj.m_lTags)
                        {
                            string tagName = catalogTag.m_oTagMeta != null ? catalogTag.m_oTagMeta.m_sName : string.Empty;
                            if (tags.ContainsKey(tagName.ToLower()))
                            {
                                tags[tagName.ToLower()].AddRange(catalogTag.m_lValues);
                            }
                            else
                            {
                                tags.Add(tagName.ToLower(), new List<string>());
                                tags[tagName.ToLower()].AddRange(catalogTag.m_lValues);
                            }
                        }
                    }
                }
                return tags;
            }
            catch
            {
                return null;
            }
        }

        public static string GetMediaFileTypeDescription(int nMediaFileID, int nGroupID)
        {
            try
            {
                string sRet = string.Empty;
                //try to get from cache 
                string key = string.Format("api_MediaFileTypeDescription_{0}", nMediaFileID);
                string sMediaFileType = string.Empty;
                bool bInCache = ApiCache.GetItem<string>(key, out sMediaFileType);

                if (!bInCache)
                {
                    DataTable dt = DAL.ApiDAL.Get_MediaFileTypeDescription(nMediaFileID, nGroupID);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        sMediaFileType = APILogic.Utils.GetSafeStr(dt.Rows[0], "DESCRIPTION");

                        ApiCache.AddItem(key, sMediaFileType);
                    }
                }

                return sMediaFileType;

            }
            catch (Exception)
            {
                return string.Empty;
            }

        }

        public static RegionsResponse GetRegions(int groupID, List<string> externalRegionList, RegionOrderBy orderBy)
        {
            RegionsResponse response = null;
            DataSet ds = null;
            try
            {
                if (externalRegionList == null)
                {
                    externalRegionList = new List<string>();

                }

                ds = DAL.ApiDAL.Get_RegionsByExternalRegions(groupID, externalRegionList, orderBy);

                Region region;
                if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
                {
                    response = new RegionsResponse();
                    response.Regions = new List<Region>();

                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            region = new Region()
                            {
                                id = APILogic.Utils.GetIntSafeVal(row, "id"),
                                name = APILogic.Utils.GetSafeStr(row, "name"),
                                externalId = APILogic.Utils.GetSafeStr(row, "external_id"),
                                isDefault = APILogic.Utils.GetIntSafeVal(row, "is_default_region") == 1 ? true : false,
                            };
                            response.Regions.Add(region);
                        }
                    }
                    if (ds.Tables[1] != null && ds.Tables[1].Rows != null)
                    {
                        int regionId;
                        foreach (DataRow row in ds.Tables[1].Rows)
                        {
                            regionId = APILogic.Utils.GetIntSafeVal(row, "region_id");
                            region = response.Regions.Where(r => r.id == regionId).FirstOrDefault();
                            if (region != null)
                            {
                                region.linearChannels.Add(new ApiObjects.KeyValuePair(APILogic.Utils.GetIntSafeVal(row, "media_id").ToString(), APILogic.Utils.GetIntSafeVal(row, "channel_number").ToString()));
                            }
                        }
                    }

                }
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
            }
            catch (Exception)
            {
                response = new RegionsResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        public static List<LanguageObj> GetGroupLanguages(int groupId)
        {
            List<LanguageObj> response = null;
            try
            {
                response = Tvinci.Core.DAL.CatalogDAL.GetGroupLanguages(groupId);
            }
            catch (Exception)
            {
                return null;
            }

            return response;
        }

        #region Parental Rules

        public static ParentalRulesResponse GetParentalRules(int groupId)
        {
            ParentalRulesResponse response = new ParentalRulesResponse()
            {
                rules = new List<ParentalRule>()
            };

            try
            {
                List<ParentalRule> rules = null;
                string key = LayeredCacheKeys.GetGroupParentalRulesKey(groupId);
                bool cacheResult = LayeredCache.Instance.Get<List<ParentalRule>>(key, ref rules, APILogic.Utils.GetGroupParentalRules, new Dictionary<string, object>() { { "groupId", groupId } },
                                                                                        groupId, LayeredCacheConfigNames.GROUP_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME);
                if (!cacheResult)
                {
                    log.Error(string.Format("GetParentalRules - Failed get data from cache groupId = {0}", groupId));
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    return response;
                }

                response.rules = rules;
                response.status = new ApiObjects.Response.Status()
                {
                    Code = (int)eResponseStatus.OK
                };
            }
            catch (Exception ex)
            {
                response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                log.Error("GetParentalRules - " +
                    string.Format("Error in GetParentalRules: group = {0}, ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace),
                    ex);
            }

            return response;
        }

        public static ParentalRulesResponse GetDomainParentalRules(int groupId, int domainId)
        {
            ParentalRulesResponse response = new ParentalRulesResponse()
            {
                rules = new List<ParentalRule>()
            };

            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, string.Empty, domainId);
            response.status = status;

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    response.rules = DAL.ApiDAL.Get_Domain_ParentalRules(groupId, domainId);
                    response.status = new ApiObjects.Response.Status()
                    {
                        Code = (int)eResponseStatus.OK
                    };
                }
                catch (Exception ex)
                {
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.Error("GetDomainParentalRules - " +
                        string.Format("Error in GetDomainParentalRules: group = {0}, domain = {3}, ex = {1}, ST = {2}",
                            groupId, ex.Message, ex.StackTrace, domainId), ex);

                }
            }

            return response;
        }

        public static ParentalRulesResponse GetUserParentalRules(int groupId, string siteGuid, int domainId)
        {
            ParentalRulesResponse response = new ParentalRulesResponse()
            {
                rules = new List<ParentalRule>()
            };

            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, domainId);
            response.status = status;

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    response.rules = DAL.ApiDAL.Get_User_ParentalRules(groupId, siteGuid);

                    response.status = new ApiObjects.Response.Status()
                    {
                        Code = (int)eResponseStatus.OK
                    };
                }
                catch (Exception ex)
                {
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.Error("GetUserParentalRules - " +
                        string.Format("Error in GetUserParentalRules: group = {0}, user = {3}, ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace, siteGuid),
                        ex);
                }
            }

            return response;
        }

        public static ApiObjects.Response.Status SetUserParentalRules(int groupId, string siteGuid, long ruleId, int isActive, int domainId)
        {
            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, domainId);

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    int newId = DAL.ApiDAL.Set_UserParentalRule(groupId, siteGuid, ruleId, isActive);

                    if (newId > 0)
                    {
                        status.Code = (int)eResponseStatus.OK;
                        status.Message = string.Empty;

                        LayeredCache.Instance.SetInvalidationKey(GetUserParentalRuleInvalidationKey(siteGuid));
                    }
                    else if (newId == -260)
                    {
                        status.Code = (int)eResponseStatus.UserParentalRuleNotExists;
                        status.Message = "Cannot disable a default rule that was not specifically enabled previously";
                    }
                    else if (newId == -999)
                    {
                        status.Code = (int)eResponseStatus.RuleNotExists;
                        status.Message = eResponseStatus.RuleNotExists.ToString();
                    }
                    else
                    {
                        status.Code = (int)eResponseStatus.Error;
                    }
                }
                catch (Exception ex)
                {
                    status.Code = (int)eResponseStatus.Error;
                    log.Error("SetUserParentalRules - " +
                               string.Format("Error in SetUserParentalRules: group = {0}, user = {3}, ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace, siteGuid),
                               ex);
                }
            }

            return status;
        }

        private static string GetUserParentalRuleInvalidationKey(string siteGuid)
        {
            return string.Format("user_parental_rules_{0}", siteGuid);
        }

        public static ApiObjects.Response.Status SetDomainParentalRules(int groupId, int domainId, long ruleId, int isActive)
        {
            Users.Domain domain;
            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, string.Empty, domainId, out domain);

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    int newId = DAL.ApiDAL.Set_DomainParentalRule(groupId, domainId, ruleId, isActive);

                    if (newId > 0)
                    {
                        status.Code = (int)eResponseStatus.OK;

                        SetInvalidKeyParentalRulesForDomain(groupId, domainId, domain);
                    }
                    else if (newId == -260)
                    {
                        status.Code = (int)eResponseStatus.UserParentalRuleNotExists;
                        status.Message = "Cannot disable a default rule that was not specifically enabled previously";
                    }
                    else if (newId == -999)
                    {
                        status.Code = (int)eResponseStatus.RuleNotExists;
                        status.Message = eResponseStatus.RuleNotExists.ToString();
                    }
                    else
                    {
                        status.Code = (int)eResponseStatus.Error;
                    }
                }
                catch (Exception ex)
                {
                    status.Code = (int)eResponseStatus.Error;
                    log.Error("SetDomainParentalRules - " +
                        string.Format("Error in SetDomainParentalRules: group = {0}, domain = {3}, ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace, domainId),
                        ex);
                }
            }

            return status;
        }

        public static PinResponse GetParentalPIN(int groupId, int domainId, string siteGuid, int? ruleId = null)
        {
            PinResponse response = new PinResponse();

            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, domainId);
            response.status = status;

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    var ruleValidationStatus = ValidateRuleId(groupId, ruleId);

                    // If validation was not successful - return relevant error
                    if (ruleValidationStatus == null)
                    {
                        response.status = status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "error validating rule ID");
                    }
                    else if (ruleValidationStatus.Code != (int)eResponseStatus.OK)
                    {
                        response.status = status = ruleValidationStatus;
                    }
                    else
                    {
                        eRuleLevel level = eRuleLevel.User;
                        string pin = DAL.ApiDAL.Get_ParentalPIN(groupId, domainId, siteGuid, out level, false, ruleId);

                        if (!string.IsNullOrEmpty(pin))
                        {
                            response.pin = pin;
                            response.level = level;
                            response.status = new ApiObjects.Response.Status()
                            {
                                Code = (int)eResponseStatus.OK
                            };
                        }
                        else
                        {
                            response.status = new ApiObjects.Response.Status()
                            {
                                Code = (int)eResponseStatus.NoPinDefined,
                                Message = "No PIN was found"
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.Error("GetParentalPIN - " +
                        string.Format("Error in GetParentalPIN: group = {0}, user = {3}/domain = {4} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace, siteGuid, domainId), ex);
                }
            }

            return response;
        }

        public static ApiObjects.Response.Status SetParentalPIN(int groupId, int domainId, string siteGuid, string pin, int? ruleId = null)
        {
            Users.Domain domain;
            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, domainId, out domain);

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    var ruleValidationStatus = ValidateRuleId(groupId, ruleId);

                    // If validation was not successful - return relevant error
                    if (ruleValidationStatus == null)
                    {
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "error validating rule ID");
                    }
                    else if (ruleValidationStatus.Code != (int)eResponseStatus.OK)
                    {
                        status = ruleValidationStatus;
                    }
                    // Otherwise, move on
                    else
                    {
                        int newId = DAL.ApiDAL.Set_ParentalPIN(groupId, siteGuid, domainId, pin, ruleId);

                        if (newId > 0)
                        {
                            status.Code = (int)eResponseStatus.OK;

                            // if we updated a user only - set its key as invalid
                            if (!string.IsNullOrEmpty(siteGuid))
                            {
                                LayeredCache.Instance.SetInvalidationKey(GetUserParentalRuleInvalidationKey(siteGuid));
                            }
                            // otherwise - set all of the domain's users keys as invalid
                            else
                            {
                                SetInvalidKeyParentalRulesForDomain(groupId, domainId, domain);
                            }
                        }
                        else
                        {
                            status.Code = (int)eResponseStatus.Error;
                        }
                    }
                }
                catch (Exception ex)
                {
                    status.Code = (int)eResponseStatus.Error;
                    log.Error("SetParentalPIN - " +
                        string.Format("Error in SetParentalPIN: group = {0}, user = {3}/domain = {4} ex = {1}, ST = {2}",
                            groupId, ex.Message, ex.StackTrace, siteGuid, domainId), ex);

                }
            }

            return status;
        }

        /// <summary>
        /// For a given domain, marks its parental rules as invalid in our layered cache
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="domainId"></param>
        /// <param name="domain"></param>
        private static void SetInvalidKeyParentalRulesForDomain(int groupId, int domainId, Users.Domain domain)
        {
            // Get domain from web service if it wasn't given
            if (domain == null)
            {
                ValidateUserAndDomain(groupId, string.Empty, domainId, out domain);
            }

            // If we succesfully obtained a domain object
            if (domain != null)
            {
                // Set invalidation key for each of its users
                foreach (var userId in domain.m_UsersIDs)
                {
                    LayeredCache.Instance.SetInvalidationKey(GetUserParentalRuleInvalidationKey(userId.ToString()));
                }
            }
        }

        private static ApiObjects.Response.Status ValidateRuleId(int groupId, int? ruleId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.OK);

            // If there was no rule Id sent - old behavior, ignore the rule Id, not validation required
            if (ruleId != null && ruleId.HasValue)
            {
                // Get all parental rules of this group and check if rule Id is valid
                var groupRules = GetParentalRules(groupId);

                // if something went wrong when getting rules - return relevant error
                if (groupRules.status != null && groupRules.status.Code != (int)eResponseStatus.OK)
                {
                    status = groupRules.status;
                }
                // before we try... check null object
                else if (groupRules.rules == null || groupRules.rules.Count == 0)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.RuleNotExists,
                        string.Format("Rule with id {0} does not exist for this group", ruleId));
                }
                else
                {
                    long longRuleId = (long)ruleId.Value;

                    // Search for a rule with this Id
                    var rule = groupRules.rules.FirstOrDefault(currentRule => currentRule.id == longRuleId);

                    // If we found, the rule is valid
                    if (rule == null)
                    {
                        status = new ApiObjects.Response.Status((int)eResponseStatus.RuleNotExists,
                        string.Format("Rule with id {0} does not exist for this group", ruleId));
                    }
                }
            }

            return status;
        }

        public static PurchaseSettingsResponse GetPurchaseSettings(int groupId, int domainId, string siteGuid)
        {
            PurchaseSettingsResponse response = new PurchaseSettingsResponse();

            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, domainId);
            response.status = status;

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    eRuleLevel level = eRuleLevel.User;
                    ePurchaeSettingsType type = ePurchaeSettingsType.Block;

                    bool success = DAL.ApiDAL.Get_PurchaseSettings(groupId, domainId, siteGuid, out level, out type);

                    if (success)
                    {
                        response.level = level;
                        response.type = type;
                        response.status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.OK
                        };
                    }
                    else
                    {
                        response.status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.Error,
                            Message = "No purchase settings found"
                        };
                    }
                }
                catch (Exception ex)
                {
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.Error("GetPurchaseSettings - " +
                        string.Format("Error in GetPurchaseSettings: group = {0}, user = {3}/domain = {4} ex = {1}, ST = {2}",
                            groupId, ex.Message, ex.StackTrace, siteGuid, domainId), ex);

                }
            }

            return response;
        }

        public static ApiObjects.Response.Status SetPurchaseSettings(int groupId, int domainId, string siteGuid, int purchaeSettingsType)
        {
            if (!Enum.IsDefined(typeof(ePurchaeSettingsType), purchaeSettingsType))
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.PurchaseSettingsTypeInvalid, PURCHASE_SETTINGS_TYPE_INVALID);
            }

            return SetPurchaseSettings(groupId, domainId, siteGuid, (ePurchaeSettingsType)purchaeSettingsType);
        }

        public static ApiObjects.Response.Status SetPurchaseSettings(int groupId, int domainId, string siteGuid, ePurchaeSettingsType ePurchaeSettingsType)
        {
            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, domainId);

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    int newId = DAL.ApiDAL.Set_PurchaseSettings(groupId, siteGuid, domainId, ePurchaeSettingsType);

                    if (newId > 0)
                    {
                        status.Code = (int)eResponseStatus.OK;
                    }
                    else
                    {
                        status.Code = (int)eResponseStatus.Error;
                    }
                }
                catch (Exception ex)
                {
                    status.Code = (int)eResponseStatus.Error;
                    log.Error("SetPurchaseSettings - " +
                        string.Format("Error in SetPurchaseSettings: group = {0}, user = {3}/domain = {4} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace, siteGuid, domainId),
                        ex);
                }
            }

            return status;
        }

        public static PurchaseSettingsResponse GetPurchasePIN(int groupId, int domainId, string siteGuid)
        {
            PurchaseSettingsResponse response = new PurchaseSettingsResponse();

            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, domainId);
            response.status = status;

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    eRuleLevel level = eRuleLevel.User;
                    string pin;

                    /*
                    eRuleLevel level = eRuleLevel.User;
                    string pin = DAL.ApiDAL.Get_ParentalPIN(groupId, domainId, siteGuid, out level, false);

                    if (!string.IsNullOrEmpty(pin))
                    {
                        response.pin = pin;
                        response.level = level;
                        response.status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.OK
                        };
                    }
                    else
                    {
                        response.status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.NoPinDefined,
                            Message = "No PIN was found"
                        };
                    }
                     */

                    bool success = DAL.ApiDAL.Get_PurchasePin(groupId, domainId, siteGuid, out level, out pin, false);

                    if (!success)
                    {
                        response.status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.Error,
                            Message = "No purchase settings found"
                        };
                    }
                    else if (string.IsNullOrEmpty(pin))
                    {
                        response.level = eRuleLevel.Group;
                        response.status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.NoPinDefined,
                            Message = "No PIN was found"
                        };
                    }
                    else
                    {
                        response.pin = pin;
                        response.level = level;
                        response.status = new ApiObjects.Response.Status()
                        {
                            Code = (int)eResponseStatus.OK
                        };
                    }
                }
                catch (Exception ex)
                {
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.Error("GetPurchasePIN - " +
                        string.Format("Error in GetPurchasePIN: group = {0}, user = {3}/domain = {4} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace, siteGuid, domainId),
                        ex);
                }
            }

            return response;
        }

        public static ApiObjects.Response.Status SetPurchasePIN(int groupId, int domainId, string siteGuid, string pin)
        {
            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, domainId);

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    int newId = DAL.ApiDAL.Set_PurchasePIN(groupId, siteGuid, domainId, pin);

                    if (newId > 0)
                    {
                        status.Code = (int)eResponseStatus.OK;
                    }
                    else
                    {
                        status.Code = (int)eResponseStatus.Error;
                    }
                }
                catch (Exception ex)
                {
                    status.Code = (int)eResponseStatus.Error;
                    log.Error("SetPurchasePIN - " +
                        string.Format("Error in SetPurchasePIN: group = {0}, user = {3}/domain = {4} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace, siteGuid, domainId),
                        ex);
                }
            }

            return status;
        }

        public static ApiObjects.Response.Status ValidateParentalPIN(int groupId, string siteGuid, string pin, int domainId, int? ruleId)
        {
            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, domainId);

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    var ruleValidationStatus = ValidateRuleId(groupId, ruleId);

                    // If validation was not successful - return relevant error
                    if (ruleValidationStatus == null)
                    {
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "error validating rule ID");
                    }
                    else if (status.Code != (int)eResponseStatus.OK)
                    {
                        status = ruleValidationStatus;
                    }
                    else
                    {
                        eRuleLevel level = eRuleLevel.User;

                        string parentalPin = DAL.ApiDAL.Get_ParentalPIN(groupId, 0, siteGuid, out level, true, ruleId);

                        if (string.IsNullOrEmpty(parentalPin))
                        {
                            status.Code = (int)eResponseStatus.NoPinDefined;
                            status.Message = "User has no PIN defined";
                        }
                        else if (pin == parentalPin)
                        {
                            status.Code = (int)eResponseStatus.OK;
                        }
                        else
                        {
                            status.Code = (int)eResponseStatus.PinMismatch;
                            status.Message = "Input PIN and user PIN do not match";
                        }
                    }
                }
                catch (Exception ex)
                {
                    status.Code = (int)eResponseStatus.Error;
                    log.Error("ValidateParentalPIN - " +
                        string.Format("Error in SetPurchasePIN: group = {0}, user = {3} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace, siteGuid),
                        ex);
                }
            }

            return status;
        }

        public static ApiObjects.Response.Status ValidatePurchasePIN(int groupId, string siteGuid, string pin, int domainId)
        {
            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, domainId);

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    eRuleLevel level = eRuleLevel.User;
                    string parentalPin;

                    bool success = DAL.ApiDAL.Get_PurchasePin(groupId, 0, siteGuid, out level, out parentalPin, true);

                    if (!success)
                    {
                        status.Code = (int)eResponseStatus.Error;
                        status.Message = "No purchase settings found";
                    }
                    else if (string.IsNullOrEmpty(parentalPin))
                    {
                        status.Code = (int)eResponseStatus.NoPinDefined;
                        status.Message = "User has no PIN defined";
                    }
                    else if (pin == parentalPin)
                    {
                        status.Code = (int)eResponseStatus.OK;
                    }
                    else
                    {
                        status.Code = (int)eResponseStatus.PinMismatch;
                        status.Message = "Input PIN and user PIN do not match";
                    }
                }
                catch (Exception ex)
                {
                    status.Code = (int)eResponseStatus.Error;
                    log.Error("ValidatePurchasePIN - " +
                        string.Format("Error in SetPurchasePIN: group = {0}, user = {3} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace, siteGuid),
                        ex);
                }
            }

            return status;
        }

        public static ParentalRulesResponse GetParentalMediaRules(int groupId, string siteGuid, long mediaId, long domainId)
        {
            List<ParentalRule> rules = new List<ParentalRule>();
            ParentalRulesResponse response = new ParentalRulesResponse()
            {
                rules = new List<ParentalRule>()
            };

            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, (int)domainId);
            response.status = status;

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    List<ParentalRule> groupsParentalRules = null;
                    List<long> mediaRuleIds = new List<long>();
                    Dictionary<long, eRuleLevel> userParentalRules = null;

                    // group rules                 
                    string key = LayeredCacheKeys.GetGroupParentalRulesKey(groupId);
                    // try to get from cache  
                    bool cacheResult = LayeredCache.Instance.Get<List<ParentalRule>>(key,
                        ref groupsParentalRules, APILogic.Utils.GetGroupParentalRules,
                        new Dictionary<string, object>() { { "groupId", groupId } }, groupId, LayeredCacheConfigNames.GROUP_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME);

                    if (!cacheResult || groupsParentalRules == null)
                    {
                        log.Error(string.Format("GetParentalMediaRules - GetGroupParentalRules - Failed get data from cache groupId = {0}", groupId));
                        return null;
                    }

                    if (groupsParentalRules.Count == 0)
                    {
                        return response;
                    }

                    // media rules id 
                    key = LayeredCacheKeys.GetMediaParentalRulesKey(groupId, mediaId);
                    cacheResult = LayeredCache.Instance.Get<List<long>>(key, ref mediaRuleIds, GetMediaParentalRules,
                        new Dictionary<string, object>() 
                            { 
                                { "groupId", groupId }, 
                                { "mediaId", mediaId }, 
                                { "groupsParentalRules", groupsParentalRules } 
                            },
                        groupId, LayeredCacheConfigNames.MEDIA_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaId) });

                    if (!cacheResult)
                    {
                        log.Error(string.Format("GetParentalMediaRules - GetMediaParentalRules - Failed get data from cache groupId={0}, mediaId={1}", groupId, mediaId));
                        return null;
                    }
                    else if (mediaRuleIds != null && mediaRuleIds.Count > 0)
                    {
                        List<string> userParentalRulesInvalidationKeys = new List<string>();
                        userParentalRulesInvalidationKeys.Add(GetUserParentalRuleInvalidationKey(siteGuid));

                        // user rules 
                        key = LayeredCacheKeys.GetUserParentalRulesKey(groupId, siteGuid);
                        cacheResult = LayeredCache.Instance.Get<Dictionary<long, eRuleLevel>>(key, ref userParentalRules,
                            APILogic.Utils.GetUserParentalRules, new Dictionary<string, object>() { { "groupId", groupId }, { "userId", siteGuid } },
                            groupId, LayeredCacheConfigNames.USER_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME, userParentalRulesInvalidationKeys);

                        if (!cacheResult || userParentalRules == null)
                        {
                            log.Error(string.Format("GetParentalMediaRules - GetUserParentalRules - Failed get data from cache groupId = {0}, userId = {1}", groupId, siteGuid));
                            return null;
                        }

                        // if user has at least one rule applied on him on user or domain level
                        if (userParentalRules.Count > 0)
                        {
                            // If user ignored group's default rules - and if this is the only one - user has 0 parental rules for this media, he's free.
                            if (userParentalRules.Count == 1 && userParentalRules.FirstOrDefault().Key < 0)
                            {
                                response.rules = new List<ParentalRule>();
                            }
                            else
                            {
                                // User does have rules. Let's see which of the rules that are relevant to the media are also relevant to the user (intersection)
                                rules = groupsParentalRules.Where(x => mediaRuleIds.Contains(x.id) && userParentalRules.ContainsKey(x.id)).ToList();
                            }
                        }
                        else if (groupsParentalRules.Count > 0) // check on group rules 
                        {
                            // check if media related to user parental rules - if needed 
                            rules = groupsParentalRules.Where(x => mediaRuleIds.Contains(x.id) && x.isDefault).ToList();
                        }
                    }

                    response.rules = rules != null ? rules : new List<ParentalRule>();

                    response.status = new ApiObjects.Response.Status()
                    {
                        Code = (int)eResponseStatus.OK
                    };

                }
                catch (Exception ex)
                {
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.Error("GetParentalMediaRules - " + string.Format("Error in GetParentalMediaRules: group = {0}, user = {3}, media = {4}, ex = {1}, ST = {2}",
                        groupId, ex.Message, ex.StackTrace, siteGuid, mediaId),
                        ex);
                }
            }

            return response;
        }

        private static Tuple<List<long>, bool> GetMediaParentalRules(Dictionary<string, object> funcParams)
        {
            bool result = false;
            List<long> ruleIds = new List<long>();

            try
            {
                if (funcParams != null && funcParams.Count == 3)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("mediaId") && funcParams.ContainsKey("groupsParentalRules"))
                    {
                        int? groupId;
                        long? mediaId;
                        List<ParentalRule> groupsParentalRules = new List<ParentalRule>();
                        UnifiedSearchResult[] medias;
                        string filter = string.Empty;

                        groupId = funcParams["groupId"] as int?;
                        mediaId = funcParams["mediaId"] as long?;
                        groupsParentalRules = funcParams["groupsParentalRules"] as List<ParentalRule>;

                        if (groupId.HasValue && mediaId.HasValue && groupsParentalRules != null && groupsParentalRules.Count > 0)
                        {
                            // find all asset ids that match the tag + tag value ==> if so save the rule id
                            //build serach for each tag and tag values

                            List<string> tempFilter = new List<string>();


                            groupsParentalRules = groupsParentalRules.Where(x => !string.IsNullOrEmpty(x.mediaTagType)).ToList();
                            Parallel.ForEach(groupsParentalRules, (rule) =>
                            {
                                tempFilter = rule.mediaTagValues.Select(x => string.Format("{0}='{1}'", rule.mediaTagType, x)).ToList();

                                if (tempFilter != null && tempFilter.Count > 0)
                                {
                                    if (tempFilter.Count > 1)
                                    {
                                        filter = string.Format("(and media_id='{0}' (or {1}))", mediaId.Value, string.Join(" ", tempFilter));
                                    }
                                    else
                                    {
                                        filter = string.Format("(and media_id='{0}' {1})", mediaId.Value, tempFilter.First());
                                    }

                                    medias = SearchAssets(groupId.Value, filter, 0, 0, true, 0, true, string.Empty, string.Empty, string.Empty, 0, 0, true);
                                    if (medias != null && medias.Count() > 0)// there is a match 
                                    {
                                        ruleIds.Add(rule.id);
                                    }
                                }
                            });

                            result = true;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaParentalRules faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<List<long>, bool>(ruleIds.Distinct().ToList(), result);
        }

        public static GenericRuleResponse GetMediaRules(int groupId, string siteGuid, long mediaId, long domainId, string ip, string udid, GenericRuleOrderBy orderBy = GenericRuleOrderBy.NameAsc)
        {
            GenericRuleResponse response = new GenericRuleResponse();

            response.Status = ValidateUserAndDomain(groupId, siteGuid, (int)domainId);

            if (response.Status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    //Check if geo-block applies
                    string ruleName;
                    if (CheckGeoBlockMedia(groupId, (int)mediaId, ip, out ruleName))
                    {
                        response.Rules.Add(new GenericRule() { Name = ruleName, RuleType = RuleType.Geo, Description = string.Empty });
                    }

                    //Check if user type match media user types
                    if (!string.IsNullOrEmpty(siteGuid) && CheckMediaUserType((int)mediaId, int.Parse(siteGuid), groupId) == false)
                    {
                        response.Rules.Add(new GenericRule() { Name = "UserTypeBlock", RuleType = RuleType.UserType, Description = string.Empty });
                    }

                    ParentalRulesResponse parentalRulesResponse = GetParentalMediaRules(groupId, siteGuid, mediaId, domainId);
                    if (parentalRulesResponse != null && parentalRulesResponse.rules != null)
                    {
                        foreach (ParentalRule rule in parentalRulesResponse.rules)
                        {
                            response.Rules.Add(new GenericRule()
                            {
                                Id = rule.id,
                                Description = rule.description,
                                Name = rule.name,
                                RuleType = RuleType.Parental
                            });

                        }
                    }

                    // order results
                    switch (orderBy)
                    {
                        case GenericRuleOrderBy.NameAsc:
                            response.Rules.OrderBy(r => r.Name).ToList();
                            break;
                        case GenericRuleOrderBy.NameDesc:
                            response.Rules.OrderByDescending(r => r.Name).ToList();
                            break;
                        default:
                            break;
                    }

                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
                }
                catch (Exception ex)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");

                    log.Error("GetMediaRules - " + string.Format("Error in GetMediaRules: group = {0}, user = {3}, media = {4}, ex = {1}, ST = {2}",
                        groupId, ex.Message, ex.StackTrace, siteGuid, mediaId),
                        ex);
                }
            }

            return response;
        }

        public static ParentalRulesResponse GetParentalEPGRules(int groupId, string siteGuid, long epgId, long domainId)
        {
            List<ParentalRule> rules = null;
            ParentalRulesResponse response = new ParentalRulesResponse()
            {
                rules = new List<ParentalRule>()
            };

            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, (int)domainId);
            response.status = status;

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    List<ParentalRule> groupParentalRules = null;
                    List<long> epgRuleIds = new List<long>();
                    Dictionary<long, eRuleLevel> userParentalRules = null;

                    // group rules                 
                    string key = LayeredCacheKeys.GetGroupParentalRulesKey(groupId);
                    // try to get from cache  
                    bool cacheResult = LayeredCache.Instance.Get<List<ParentalRule>>(key, ref groupParentalRules, APILogic.Utils.GetGroupParentalRules, new Dictionary<string, object>() { { "groupId", groupId } },
                                                                                            groupId, LayeredCacheConfigNames.GROUP_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME);
                    if (!cacheResult || groupParentalRules == null)
                    {
                        log.Error(string.Format("GetParentalEPGRules - GetGroupParentalRules - Failed get data from cache groupId = {0}", groupId));
                        return null;
                    }

                    if (groupParentalRules.Count == 0)
                    {
                        return response;
                    }

                    // epg rules id 
                    key = LayeredCacheKeys.GetEpgParentalRulesKey(groupId, epgId);
                    cacheResult = LayeredCache.Instance.Get<List<long>>(key, ref epgRuleIds, GetEpgParentalRules,
                        new Dictionary<string, object>() 
                            { 
                                { "groupId", groupId }, 
                                { "epgId", epgId }, 
                                { "groupParentalRules", groupParentalRules } 
                            },
                        groupId, LayeredCacheConfigNames.EPG_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME);

                    if (!cacheResult)
                    {
                        log.Error(string.Format("GetParentalEPGRules - GetEpgParentalRules - Failed get data from cache groupId={0}, mediaId={1}", groupId, epgId));
                        return null;
                    }
                    else if (epgRuleIds != null && epgRuleIds.Count > 0)
                    {
                        List<string> userParentalRulesInvalidationKeys = new List<string>();
                        userParentalRulesInvalidationKeys.Add(GetUserParentalRuleInvalidationKey(siteGuid));

                        // user rules 
                        key = LayeredCacheKeys.GetUserParentalRulesKey(groupId, siteGuid);
                        cacheResult = LayeredCache.Instance.Get<Dictionary<long, eRuleLevel>>(key, ref userParentalRules,
                            APILogic.Utils.GetUserParentalRules, new Dictionary<string, object>() { { "groupId", groupId }, { "userId", siteGuid } },
                            groupId, LayeredCacheConfigNames.USER_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME, userParentalRulesInvalidationKeys);

                        if (!cacheResult || userParentalRules == null)
                        {
                            log.Error(string.Format("GetParentalEPGRules - GetUserParentalRules - Failed get data from cache groupId = {0}, userId = {1}", groupId, siteGuid));
                            return null;
                        }

                        // if user has at least one rule applied on him on user or domain level
                        if (userParentalRules.Count > 0)
                        {
                            // If user ignored group's default rules - and if this is the only one - user has 0 parental rules for this media, he's free.
                            if (userParentalRules.Count == 1 && userParentalRules.FirstOrDefault().Key < 0)
                            {
                                rules = new List<ParentalRule>();
                            }
                            else
                            {
                                // User does have rules. Let's see which of the rules that are relevant to the media are also relevant to the user (intersection)
                                rules = groupParentalRules.Where(x => epgRuleIds.Contains(x.id) && userParentalRules.ContainsKey(x.id)).ToList();
                            }
                        }
                        else if (groupParentalRules.Count > 0) // check on group rules 
                        {
                            // check if media related to user parental rules - if needed 
                            rules = groupParentalRules.Where(x => epgRuleIds.Contains(x.id) && x.isDefault).ToList();
                        }
                    }

                    response.rules = rules != null ? rules : new List<ParentalRule>();

                    response.status = new ApiObjects.Response.Status()
                    {
                        Code = (int)eResponseStatus.OK
                    };
                }
                catch (Exception ex)
                {
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.Error("GetParentalEPGRules - " + string.Format("Error in GetParentalEPGRules: group = {0}, user = {3}, epg = {4}, ex = {1}, ST = {2}",
                        groupId, ex.Message, ex.StackTrace, siteGuid, epgId), ex);
                }
            }

            return response;
        }

        private static Tuple<List<long>, bool> GetEpgParentalRules(Dictionary<string, object> funcParams)
        {
            bool result = false;
            List<long> ruleIds = new List<long>();

            try
            {
                if (funcParams != null && funcParams.Count == 3)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("epgId") && funcParams.ContainsKey("groupParentalRules"))
                    {
                        int? groupId;
                        long? epgId;
                        List<ParentalRule> groupParentalRules = new List<ParentalRule>();

                        groupId = funcParams["groupId"] as int?;
                        epgId = funcParams["epgId"] as long?;
                        groupParentalRules = funcParams["groupParentalRules"] as List<ParentalRule>;

                        if (groupId.HasValue && epgId.HasValue && groupParentalRules != null && groupParentalRules.Count > 0)
                        {
                            groupParentalRules = groupParentalRules.Where(x => !string.IsNullOrEmpty(x.epgTagType)).ToList();
                            if (groupParentalRules == null || groupParentalRules.Count == 0)
                            {
                                return new Tuple<List<long>, bool>(new List<long>(), result);
                            }

                            // get epg program from CB ==> check matches to tag type and values 

                            TvinciEpgBL epgBLTvinci = new TvinciEpgBL(groupId.Value);  //assuming this is a Tvinci user - does not support yes Epg
                            EpgCB epg = epgBLTvinci.GetEpgCB((ulong)epgId.Value);
                            if (epg != null)
                            {
                                Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();

                                foreach (KeyValuePair<string, List<string>> tag in epg.Tags)
                                {
                                    tags.Add(tag.Key.ToLower(), new List<string>(tag.Value.ConvertAll(x => x.ToLower())).ToList());
                                }

                                groupParentalRules = groupParentalRules.Where(x => tags.Select(t => t.Key.ToLower()).Contains(x.epgTagType.ToLower())).ToList();

                                Parallel.ForEach(groupParentalRules, (rule) =>
                                {
                                    if (rule.epgTagValues.Any(x => tags[rule.epgTagType.ToLower()].Contains(x.ToLower())))
                                    {
                                        ruleIds.Add(rule.id);
                                    }
                                });
                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetEpgParentalRules faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<List<long>, bool>(ruleIds.Distinct().ToList(), result);
        }

        public static GenericRuleResponse GetEpgRules(int groupId, string siteGuid, long epgId, long channelMediaId, long domainId, string ip, GenericRuleOrderBy orderBy = GenericRuleOrderBy.NameAsc)
        {
            GenericRuleResponse response = new GenericRuleResponse();

            response.Status = ValidateUserAndDomain(groupId, siteGuid, (int)domainId);

            if (response.Status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    if (channelMediaId == 0)
                    {
                        // get epg channel media id
                        channelMediaId = DAL.ApiDAL.GetLinearMediaIdByEpgId(epgId);
                    }

                    if (channelMediaId > 0)
                    {
                        //Check if geo-block applies
                        string ruleName;
                        if (CheckGeoBlockMedia(groupId, (int)channelMediaId, ip, out ruleName))
                        {
                            response.Rules.Add(new GenericRule()
                            {
                                Name = ruleName,
                                RuleType = RuleType.Geo,
                                Description = string.Empty
                            });
                        }

                        //Check if user type match media user types
                        if (!string.IsNullOrEmpty(siteGuid) && CheckMediaUserType((int)channelMediaId, int.Parse(siteGuid), groupId) == false)
                        {
                            response.Rules.Add(new GenericRule()
                            {
                                Name = "UserTypeBlock",
                                RuleType = RuleType.UserType,
                                Description = string.Empty
                            });
                        }

                        ParentalRulesResponse parentalRulesResponse = GetParentalEPGRules(groupId, siteGuid, epgId, domainId);
                        if (parentalRulesResponse != null && parentalRulesResponse.rules != null)
                        {
                            foreach (ParentalRule rule in parentalRulesResponse.rules)
                            {
                                response.Rules.Add(new GenericRule()
                                {
                                    Id = rule.id,
                                    Description = rule.description,
                                    Name = rule.name,
                                    RuleType = RuleType.Parental
                                });
                            }
                        }

                        // order results
                        switch (orderBy)
                        {
                            case GenericRuleOrderBy.NameAsc:
                                response.Rules.OrderBy(r => r.Name).ToList();
                                break;
                            case GenericRuleOrderBy.NameDesc:
                                response.Rules.OrderByDescending(r => r.Name).ToList();
                                break;
                            default:
                                break;
                        }

                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
                    }
                }
                catch (Exception ex)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.Error("GetEPGRules - " +
                        string.Format("Error in GetEPGRules: group = {0}, user = {3}, epg = {4}, ex = {1}, ST = {2}",
                        groupId, ex.Message, ex.StackTrace, siteGuid, epgId), ex);
                }
            }

            return response;
        }


        public static ParentalRulesTagsResponse GetUserParentalRuleTags(int groupId, string siteGuid, long domainId)
        {
            ParentalRulesTagsResponse response = new ParentalRulesTagsResponse();

            ApiObjects.Response.Status status = ValidateUserAndDomain(groupId, siteGuid, (int)domainId);
            response.status = status;

            if (status.Code == (int)eResponseStatus.OK)
            {
                try
                {
                    List<long> ruleIds = DAL.ApiDAL.Get_User_ParentalRulesIDs(groupId, siteGuid);
                    List<ParentalRule> rules = ParentalRulesCache.Instance().Get(groupId, ruleIds);

                    foreach (var rule in rules)
                    {
                        // Transform lists of tags from ParentalRule class to lists of tag values
                        response.mediaTags.AddRange(
                            rule.mediaTagValues.Select(tag => (new IdValuePair()
                                {
                                    id = rule.mediaTagTypeId,
                                    value = tag
                                })));

                        response.epgTags.AddRange(
                            rule.epgTagValues.Select(tag => (new IdValuePair()
                            {
                                id = rule.epgTagTypeId,
                                value = tag
                            })));
                    }

                    response.status = new ApiObjects.Response.Status()
                    {
                        Code = (int)eResponseStatus.OK
                    };
                }
                catch (Exception ex)
                {
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.Error("GetUserParentalRuleTags - " +
                        string.Format("Error in GetUserParentalRuleTags: group = {0}, user = {3}, ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace, siteGuid),
                        ex);
                }
            }

            return response;
        }

        #endregion

        #region Utility
        private static ApiObjects.Response.Status ValidateUserAndDomain(int groupId, string siteGuid, int domainId)
        {
            Users.Domain domain;
            return ValidateUserAndDomain(groupId, siteGuid, domainId, out domain);
        }

        private static ApiObjects.Response.Status ValidateUserAndDomain(int groupId, string siteGuid, int domainId, out Users.Domain domain)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            domain = null;

            // If no user - go immediately to domain validation
            if (string.IsNullOrEmpty(siteGuid) || siteGuid == "0")
            {
                status.Code = (int)eResponseStatus.OK;
            }
            else
            {
                // Get response from users WS
                ResponseStatus userStatus = APILogic.Utils.ValidateUser(groupId, siteGuid, domainId);

                // Most of the cases are not interesting - focus only on those that matter
                switch (userStatus)
                {
                    case ResponseStatus.OK:
                        {
                            status.Code = (int)eResponseStatus.OK;
                            status.Message = string.Empty;
                            break;
                        }
                    case ResponseStatus.UserDoesNotExist:
                        {
                            status.Code = (int)eResponseStatus.UserDoesNotExist;
                            break;
                        }
                    case ResponseStatus.UserNotIndDomain:
                        {
                            status.Code = (int)eResponseStatus.UserNotInDomain;
                            status.Message = "User does not belong to given domain";
                            break;
                        }
                    case ResponseStatus.UserWithNoDomain:
                        {
                            status.Code = (int)eResponseStatus.UserWithNoDomain;
                            break;
                        }
                    case ResponseStatus.UserSuspended:
                        {
                            status.Code = (int)eResponseStatus.UserSuspended;
                            break;
                        }
                    // Most cases will return general error
                    default:
                        {
                            status.Code = (int)eResponseStatus.Error;
                            status.Message = "Error validating user";
                            break;
                        }
                }
            }

            // If user is valid (or we don't have one)
            if (status.Code == (int)eResponseStatus.OK && domainId != 0)
            {
                // Get response from domains WS
                eResponseStatus domainStatus = APILogic.Utils.ValidateDomain(groupId, domainId, out domain);

                switch (domainStatus)
                {
                    case eResponseStatus.OK:

                        // Both user and domain are ok
                        status.Code = (int)domainStatus;
                        break;

                    case eResponseStatus.DomainNotExists:

                        // Specifically we are interested in non existing domains
                        status.Code = (int)domainStatus;
                        status.Message = "Domain does not exist";
                        break;

                    default:

                        // Most cases will return general error
                        status.Code = (int)eResponseStatus.Error;
                        status.Message = "Error validating domain";
                        break;
                }
            }
            return status;
        }

        #endregion

        public static bool BuildIPToCountryIndex(int groupId)
        {
            bool result = false;

            var queue = new SetupTasksQueue();

            var data = new CelerySetupTaskData(groupId, eSetupTask.BuildIPToCountry, new Dictionary<string, object>());

            try
            {
                result = queue.Enqueue(data, "BUILD_IP_TO_COUNTRY");
            }
            catch (Exception ex)
            {
                log.Error("BuildIPToCountryIndex - " +
                        string.Format("Error in BuildIPToCountryIndex: group = {0} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace),
                        ex);
            }

            return result;
        }

        public static List<ApiObjects.KeyValuePair> GetErrorCodesDictionary()
        {
            List<ApiObjects.KeyValuePair> response = new List<ApiObjects.KeyValuePair>();
            try
            {
                foreach (var value in Enum.GetValues(typeof(eResponseStatus)))
                {
                    response.Add(new ApiObjects.KeyValuePair(value.ToString(), ((int)value).ToString()));
                }
            }
            catch (Exception)
            {
                return null;
            }

            return response;
        }

        #region OSS ADAPTER
        public static OSSAdapterResponse InsertOSSAdapter(int groupID, OSSAdapter ossAdapter)
        {
            OSSAdapterResponse response = new OSSAdapterResponse();

            try
            {
                if (ossAdapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoOSSAdapterToInsert, NO_OSS_ADAPTER_TO_INSERT);
                    return response;
                }

                if (string.IsNullOrEmpty(ossAdapter.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(ossAdapter.AdapterUrl))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(ossAdapter.ExternalIdentifier))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                    return response;
                }

                // Create Shared secret 
                ossAdapter.SharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                //check External Identiifer uniqueness 
                OSSAdapter returnOssAdapter = DAL.ApiDAL.GetOSSAdapterInternalID(groupID, ossAdapter.ExternalIdentifier);

                if (returnOssAdapter != null && returnOssAdapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierMustBeUnique, ERROR_EXT_ID_ALREADY_IN_USE);
                    return response;
                }

                response.OSSAdapter = DAL.ApiDAL.InsertOSSAdapter(groupID, ossAdapter);
                if (response.OSSAdapter != null && response.OSSAdapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "new OSS Adapter insert");

                    if (!SendConfigurationToAdapter(groupID, response.OSSAdapter))
                    {
                        log.ErrorFormat("InsertOSSAdapter - SendConfigurationToAdapter failed : AdapterID = {0}", response.OSSAdapter.ID);
                    }
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to insert new OSS Adapter");
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }
            return response;
        }

        public static ApiObjects.Response.Status DeleteOSSAdapter(int groupID, int ossAdapterId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {

                if (ossAdapterId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterIdentifierRequired, OSS_ADAPTER_ID_REQUIRED);
                    return response;
                }

                // in case ossAdapterId is the group selected OSSAdapter  - delete isn’t allowed
                //-------------------------------------------------------------------------------
                object defaultOSSAdapter = ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "OSS_ADAPTER", "GROUP_ID", "=", groupID, "billing_connection");
                int ossAdapterIdentifier = 0;
                if (defaultOSSAdapter != null && int.TryParse(defaultOSSAdapter.ToString(), out ossAdapterIdentifier) && ossAdapterIdentifier > 0)
                {
                    if (ossAdapterIdentifier == ossAdapterId)
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.ActionIsNotAllowed, ACTION_IS_NOT_ALLOWED);
                        return response;
                    }
                }

                //check OSS Adapter exist
                OSSAdapter ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId);
                if (ossAdapter == null || ossAdapter.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                    return response;
                }


                bool isSet = DAL.ApiDAL.DeleteOSSAdapter(groupID, ossAdapterId);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "oss adapter deleted");
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                }

            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, ossAdapterID={1}", groupID, ossAdapterId), ex);
            }
            return response;
        }

        public static OSSAdapterResponse SetOSSAdapter(int groupID, OSSAdapter ossAdapter)
        {
            OSSAdapterResponse response = new OSSAdapterResponse();

            try
            {
                if (ossAdapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoOSSAdapterToUpdate, NO_OSS_ADAPTER_TO_UPDATE);
                    return response;
                }

                if (ossAdapter.ID == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterIdentifierRequired, OSS_ADAPTER_ID_REQUIRED);
                    return response;
                }
                if (string.IsNullOrEmpty(ossAdapter.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(ossAdapter.ExternalIdentifier))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(ossAdapter.AdapterUrl))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                    return response;
                }

                // SharedSecret generated only at insert 
                // this value not relevant at update and should be ignored
                //--------------------------------------------------------
                ossAdapter.SharedSecret = null;

                // check OssAdapter with this ID exists
                OSSAdapter existingOssAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapter.ID);
                if (existingOssAdapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, ADAPTER_NOT_EXIST);
                    return response;
                }

                //check External Identifier uniqueness 
                existingOssAdapter = DAL.ApiDAL.GetOSSAdapterInternalID(groupID, ossAdapter.ExternalIdentifier);
                if (existingOssAdapter != null && existingOssAdapter.ID != ossAdapter.ID)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierMustBeUnique, ERROR_EXT_ID_ALREADY_IN_USE);
                    return response;
                }

                response.OSSAdapter = DAL.ApiDAL.SetOSSAdapter(groupID, ossAdapter);

                if (response.OSSAdapter != null && response.OSSAdapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "oss adapter set changes");

                    if (!ossAdapter.SkipSettings)
                    {
                        bool isSet = DAL.ApiDAL.SetOSSAdapterSettings(groupID, ossAdapter.ID, ossAdapter.Settings);
                        if (isSet)
                        {
                            response.OSSAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapter.ID);
                        }
                        else
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "oss adapter failed set changes, check your params");
                        }
                    }

                    bool isSendSucceeded = SendConfigurationToAdapter(groupID, response.OSSAdapter);
                    if (!isSendSucceeded)
                    {
                        log.DebugFormat("SetOSSAdapter - SendConfigurationToAdapter failed : AdapterID = {0}", ossAdapter.ID);
                    }
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "oss adapter failed set changes");
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, ossAdapterId={1}, name={2}, adapterUrl={3}, isActive={4}",
                    groupID, ossAdapter.ID, ossAdapter.Name, ossAdapter.AdapterUrl, ossAdapter.IsActive), ex);
            }
            return response;
        }

        private static bool SendConfigurationToAdapter(int groupId, OSSAdapter ossAdapter)
        {
            try
            {
                if (ossAdapter != null && !string.IsNullOrEmpty(ossAdapter.AdapterUrl))
                {
                    //set unixTimestamp
                    long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                    //set signature
                    string signature = string.Concat(ossAdapter.ID, ossAdapter.Settings != null ? string.Concat(ossAdapter.Settings.Select(s => string.Concat(s.key, s.value))) : string.Empty,
                        groupId, unixTimestamp);

                    using (APILogic.OSSAdapterService.ServiceClient client = new APILogic.OSSAdapterService.ServiceClient(string.Empty, ossAdapter.AdapterUrl))
                    {
                        if (!string.IsNullOrEmpty(ossAdapter.AdapterUrl))
                        {
                            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(ossAdapter.AdapterUrl);
                        }

                        APILogic.OSSAdapterService.AdapterStatus adapterResponse = client.SetConfiguration(
                            ossAdapter.ID,
                            ossAdapter.Settings != null ? ossAdapter.Settings.Select(s => new APILogic.OSSAdapterService.KeyValue() { Key = s.key, Value = s.value }).ToArray() : null,
                            groupId,
                            unixTimestamp,
                            System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(ossAdapter.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                        if (adapterResponse != null && adapterResponse.Code == (int)OSSAdapterStatus.OK)
                        {
                            log.DebugFormat("OSS Adapter SetConfiguration Result: AdapterID = {0}, AdapterStatus = {1}", ossAdapter.ID, adapterResponse.Code);
                            return true;
                        }
                        else
                        {
                            log.ErrorFormat("OSS Adapter SetConfiguration Result: AdapterID = {0}, AdapterStatus = {1}",
                                ossAdapter.ID, adapterResponse != null ? adapterResponse.Code.ToString() : "ERROR");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SendConfigurationToAdapter Failed: AdapterID = {0}, ex = {1}", ossAdapter.ID, ex);
            }
            return false;
        }

        public static OSSAdapterResponseList GetOSSAdapters(int groupID)
        {
            OSSAdapterResponseList response = new OSSAdapterResponseList();
            try
            {
                response.OSSAdapters = DAL.ApiDAL.GetOSSAdapterList(groupID);
                if (response.OSSAdapters == null || response.OSSAdapters.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no oss adapter related to group");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new OSSAdapterResponseList();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }

        public static OSSAdapterResponse GetOSSAdapter(int groupID, int ossAdapterId)
        {
            OSSAdapterResponse response = new OSSAdapterResponse();
            try
            {
                response.OSSAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId);
                if (response.OSSAdapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, "OSS-Adapter not exist");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, ossAdapterId={1}", groupID, ossAdapterId), ex);
            }

            return response;
        }

        public static ApiObjects.Response.Status InsertOSSAdapterSettings(int groupID, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (ossAdapterId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterIdentifierRequired, OSS_ADAPTER_ID_REQUIRED);
                    return response;
                }

                if (settings == null || settings.Count == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterParamsRequired, NO_PARAMS_TO_INSERT);
                    return response;
                }

                //check OSS Adapter exist
                OSSAdapter ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId);
                if (ossAdapter == null || ossAdapter.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                    return response;
                }

                int matchingKeyAmount = GetMatchingKeyAmount(ossAdapter.Settings, settings);
                if (matchingKeyAmount > 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ConflictedParams, CONFLICTED_PARAMS);
                    return response;
                }

                bool isInsert = DAL.ApiDAL.InsertOSSAdapterSettings(groupID, ossAdapterId, settings);
                if (isInsert)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "oss adapter configs insert");

                    //Get oss Adapter updated                        
                    ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId);
                    if (!SendConfigurationToAdapter(groupID, ossAdapter))
                    {
                        log.ErrorFormat("InsertOSSAdapterSettings - SendConfigurationToAdapter failed : AdapterID = {0}", ossAdapterId);
                    }
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to insert oss adapter configs");
                }
            }
            catch (Exception ex)
            {

                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, ossAdapterId={1}", groupID, ossAdapterId), ex);
            }
            return response;

        }

        private static int GetMatchingKeyAmount(List<OSSAdapterSettings> originalList, List<OSSAdapterSettings> settings)
        {
            int matchingKeyAmount = 0;
            OSSAdapterSettings result;
            foreach (OSSAdapterSettings originalSettings in originalList)
            {
                result = settings.Find(x => x.key == originalSettings.key);
                if (result != null)
                {
                    matchingKeyAmount++; ;
                }

            }

            return matchingKeyAmount;
        }

        public static ApiObjects.Response.Status SetOSSAdapterSettings(int groupID, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            try
            {
                if (ossAdapterId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterIdentifierRequired, OSS_ADAPTER_ID_REQUIRED);
                    return response;
                }

                if (settings == null || settings.Count == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterParamsRequired, NO_PARAMS_TO_INSERT);
                    return response;
                }

                //check OSS Adapter exist
                OSSAdapter ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId);
                if (ossAdapter == null || ossAdapter.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                    return response;
                }

                int matchingKeyAmount = GetMatchingKeyAmount(ossAdapter.Settings, settings);
                if (matchingKeyAmount != settings.Count)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ConflictedParams, CONFLICTED_PARAMS);
                    return response;
                }


                bool isSet = DAL.ApiDAL.SetOSSAdapterSettings(groupID, ossAdapterId, settings);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "oss adapter set changes");
                    //Get oss Adapter updated                        
                    ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId);
                    if (!SendConfigurationToAdapter(groupID, ossAdapter))
                    {
                        log.ErrorFormat("SetOSSAdapterSettings - SendConfigurationToAdapter failed : AdapterID = {0}", ossAdapterId);
                    }
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "oss adapter failed set changes, check your params");
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID = {0} , ossAdapterId={1}", groupID, ossAdapterId), ex);
            }
            return response;
        }

        public static ApiObjects.Response.Status DeleteOSSAdapterSettings(int groupID, int ossAdapterId, List<OSSAdapterSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (ossAdapterId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterIdentifierRequired, OSS_ADAPTER_ID_REQUIRED);
                    return response;
                }

                if (settings == null || settings.Count == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterParamsRequired, NO_PARAMS_TO_DELETE);
                    return response;
                }

                //check OSS Adapter exist
                OSSAdapter ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId);
                if (ossAdapter == null || ossAdapter.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                    return response;
                }

                int matchingKeyAmount = GetMatchingKeyAmount(ossAdapter.Settings, settings);
                if (matchingKeyAmount != settings.Count)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ConflictedParams, CONFLICTED_PARAMS);
                    return response;
                }

                bool isSet = DAL.ApiDAL.DeleteOSSAdapter(groupID, ossAdapterId, settings);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "oss adapter configs delete");

                    //Get oss Adapter updated                        
                    ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId);
                    if (!SendConfigurationToAdapter(groupID, ossAdapter))
                    {
                        log.ErrorFormat("DeleteOSSAdapterSettings - SendConfigurationToAdapter failed : AdapterID = {0}", ossAdapterId);
                    }
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "oss adapter configs faild delete");
                }

            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, ossAdapterId={1}", groupID, ossAdapterId), ex);
            }
            return response;
        }

        public static OSSAdapterSettingsResponse GetOSSAdapterSettings(int groupID)
        {
            OSSAdapterSettingsResponse response = new OSSAdapterSettingsResponse();
            try
            {
                response.OSSAdapters = DAL.ApiDAL.GetOSSAdapterSettingsList(groupID, 0);
                if (response.OSSAdapters == null || response.OSSAdapters.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no oss adapter config related to group");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new OSSAdapterSettingsResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID = {0} ", groupID), ex);
            }

            return response;
        }

        public static OSSAdapterBillingDetailsResponse GetUserBillingDetails(int groupID, long householdId, int ossAdapterId, string userIP)
        {
            OSSAdapterBillingDetailsResponse response = new OSSAdapterBillingDetailsResponse();
            try
            {
                if (ossAdapterId == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterIdentifierRequired, OSS_ADAPTER_ID_REQUIRED);
                    return response;
                }

                // Get Oss Adapter Data                
                OSSAdapter ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId, 1);

                if (ossAdapter == null || ossAdapter.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                    return response;
                }

                // call oss Adapter
                //return oss Adapter data
                response = GetHouseholdPaymentGatewaySettings(groupID, ossAdapter, householdId, userIP);


            }
            catch (Exception ex)
            {
                response = new OSSAdapterBillingDetailsResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed GetUserBillingDetails groupID = {0}, householdId = {1}, ossAdapterId={2}", groupID, householdId, ossAdapterId), ex);
            }

            return response;
        }

        private static OSSAdapterBillingDetailsResponse GetHouseholdPaymentGatewaySettings(int groupID, OSSAdapter ossAdapter, long householdId, string userIP)
        {
            OSSAdapterBillingDetailsResponse response = new OSSAdapterBillingDetailsResponse();

            string logString = string.Format("groupID: {0}, householdId: {1}, ossAdapterUrl: {2}",
                groupID,
                householdId,
                ossAdapter.AdapterUrl != null ? ossAdapter.AdapterUrl : string.Empty);

            if (string.IsNullOrEmpty(ossAdapter.AdapterUrl))
            {
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "oss adapter has no adapter URL" };
                return response;
            }

            HouseholdBillingRequest request = new HouseholdBillingRequest() { OSSAdapter = ossAdapter, HouseholdId = householdId, UserIP = userIP };

            APILogic.OSSAdapterService.HouseholdPaymentGatewayResponse adapterResponse = AdaptersController.GetInstance(ossAdapter.ID).GetHouseholdPaymentGatewaySettings(request);

            response = ValidateAdapterResponse(adapterResponse, logString);

            if (response == null)
            {
                response = new OSSAdapterBillingDetailsResponse()
                {
                    Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "Error validating adapter response" }
                };
            }

            // if response from validation is ok, continue
            // if it is not ok, use the response from validation as final response
            else if (response.Status.Code == (int)eResponseStatus.OK)
            {
                if (adapterResponse.Status.Code == (int)OSSAdapterStatus.OK)
                {
                    switch (adapterResponse.Configuration.StateCode)
                    {
                        case (int)eOSSAdapterState.OK:
                            {
                                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK, Message = "OK" };
                                response.PaymentGatewayId = adapterResponse.Configuration.PaymentGatewayId;
                                response.ChargeId = adapterResponse.Configuration.ChargeId;
                                break;
                            }

                        case (int)eOSSAdapterState.NoConfigurationForHousehold:
                            {
                                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "No configuration for household" };
                                log.ErrorFormat("{0}. log string: {1}", "No configuration for household", logString);
                                break;
                            }

                        default:
                            {
                                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.UnknownOSSAdapterState, Message = "Unknown oss adapter state" };
                                log.ErrorFormat("Could not parse adapter result ENUM. Received: {0}, log string: {1}",
                                    adapterResponse.Configuration.StateCode, logString);
                                break;
                            }
                    }
                }

                else
                {
                    ApiObjects.Response.Status status = new ApiObjects.Response.Status();
                    switch (adapterResponse.Status.Code)
                    {
                        case (int)OSSAdapterStatus.NoConfigurationFound:
                            status.Code = (int)eResponseStatus.NoConfigurationFound;
                            status.Message = "OSS Adapter : No Configuration Found";
                            break;
                        case (int)OSSAdapterStatus.SignatureMismatch:
                            status.Code = (int)eResponseStatus.SignatureMismatch;
                            status.Message = "OSS Adapter : Signature Mismatch";
                            break;
                        case (int)OSSAdapterStatus.Error:
                        default:
                            status.Code = (int)eResponseStatus.Error;
                            status.Message = "Unknown oss adapter error";
                            break;
                    }

                    response.Status = status;
                }
            }

            return response;
        }

        private static OSSAdapterBillingDetailsResponse ValidateAdapterResponse(APILogic.OSSAdapterService.HouseholdPaymentGatewayResponse adapterResponse, string logString)
        {

            OSSAdapterBillingDetailsResponse response = null;

            if (adapterResponse == null || adapterResponse.Status == null)
            {
                // Adapter response is null
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "OSS Adapter response is null" };
                log.ErrorFormat("OSS Adapter response is null. log string: {0}", logString);
                return response;
            }

            if (adapterResponse.Status.Code == (int)OSSAdapterStatus.OK && adapterResponse.Configuration == null)
            {
                log.DebugFormat(@"OSS Adapter Transaction Result Status: Message = {0}, Code = {1}",
                   adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty, adapterResponse.Status.Code);

                // Adapter Configuration response is null
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "OSS Adapter Configuration response is null" };

                log.ErrorFormat("OSS Adapter Configuration response is null. log string: {0}", logString);
                return response;
            }

            //check for empty Configuration value
            if (string.IsNullOrEmpty(adapterResponse.Configuration.ChargeId) || string.IsNullOrEmpty(adapterResponse.Configuration.PaymentGatewayId))
            {
                // Adapter Configuration response with empty or null fields
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "OSS Adapter Configuration response with empty or null fields" };
                log.ErrorFormat("OSS Adapter Configuration response is null. log string: {0}", logString);
                return response;
            }

            // response is valid until told otherwise
            response = new OSSAdapterBillingDetailsResponse() { Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK } };


            return response;
        }

        public static ApiObjects.Response.Status SetOSSAdapterConfiguration(int groupID, int ossAdapterId)
        {

            // get oss Adapter
            //call ossadapter set configuraion 

            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                if (ossAdapterId == 0)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterIdentifierRequired, OSS_ADAPTER_ID_REQUIRED);
                    return status;
                }

                //get ossAdapter
                OSSAdapter ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId);

                if (ossAdapter == null || ossAdapter.ID <= 0)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                    return status;
                }

                if (SendConfigurationToAdapter(groupID, ossAdapter))
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }


            }
            catch (Exception ex)
            {
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed ex={0}", ex);
            }

            return status;
        }

        public static OSSAdapterResponse GenerateOSSSharedSecret(int groupID, int ossAdapterId)
        {
            OSSAdapterResponse response = new OSSAdapterResponse();

            try
            {
                if (ossAdapterId <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterIdentifierRequired, OSS_ADAPTER_ID_REQUIRED);
                    return response;
                }

                //check OSS Adapter exist
                OSSAdapter ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterId);
                if (ossAdapter == null || ossAdapter.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                    return response;
                }

                // Create Shared secret 
                string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                response.OSSAdapter = DAL.ApiDAL.SetOSSAdapterSharedSecret(groupID, ossAdapterId, sharedSecret);

                if (response.OSSAdapter != null && response.OSSAdapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "oss adapter generate shared secret");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "oss adapter failed set changes");
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, ossAdapterId={1}", groupID, ossAdapterId), ex);
            }
            return response;
        }

        #endregion

        //public static bool UpdateCache(int groupId, string bucket, string[] keys)
        //{
        //    bool result = false;

        //    var queue = new UpdateCacheQueue();

        //    CouchbaseManager.eCouchbaseBucket couchbaseBucket = CouchbaseManager.eCouchbaseBucket.DEFAULT;

        //    if (Enum.TryParse<CouchbaseManager.eCouchbaseBucket>(bucket, out couchbaseBucket))
        //    {
        //        var data = new UpdateCacheData(groupId, bucket, keys);

        //        try
        //        {
        //            result = queue.Enqueue(data, "PROCESS_UPDATE_CACHE");
        //        }
        //        catch (Exception ex)
        //        {
        //            log.Error("UpdateCache - " +
        //                    string.Format("Error in UpdateCache: group = {0} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace),
        //                    ex);
        //        }
        //    }
        //    else
        //    {
        //        log.ErrorFormat("UpdateCache - invalid couchbase bucket received: {0}", bucket);
        //    }

        //    return result;
        //}

        public static bool UpdateGeoBlockRulesCache(int groupId)
        {
            bool result = false;

            try
            {
                List<int> countries = DAL.ApiDAL.GetAllCountries();

                if (countries != null)
                {
                    List<string> keys = countries.Select(countryId => string.Format("country_to_rules_{0}_{1}", groupId, countryId)).ToList();

                    result = QueueUtils.UpdateCache(groupId, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys.ToArray());

                }
            }

            catch (Exception ex)
            {

                log.Error("UpdateGeoBlockRulesCache - " +
                        string.Format("Error in UpdateGeoBlockRulesCache: group = {0} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace),
                        ex);
            }

            return result;
        }

        #region Recommendation Engine
        public static RecommendationEngineResponse InsertRecommendationEngine(int groupID, RecommendationEngine recommendationEngine)
        {
            RecommendationEngineResponse response = new RecommendationEngineResponse();

            try
            {
                if (recommendationEngine == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoRecommendationEngineToInsert, NO_RECOMMENDATION_ENGINE_TO_INSERT);
                    return response;
                }

                if (string.IsNullOrEmpty(recommendationEngine.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(recommendationEngine.AdapterUrl))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(recommendationEngine.ExternalIdentifier))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                    return response;
                }

                // Create Shared secret 
                recommendationEngine.SharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                //check External Identiifer uniqueness 
                RecommendationEngine currentRecommendationEngineId = CatalogDAL.GetRecommendationEngineInternalID(groupID, recommendationEngine.ExternalIdentifier);

                if (currentRecommendationEngineId != null && currentRecommendationEngineId.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierMustBeUnique, ERROR_EXT_ID_ALREADY_IN_USE);
                    return response;
                }

                response.RecommendationEngine = CatalogDAL.InsertRecommendationEngine(groupID, recommendationEngine);
                if (response.RecommendationEngine != null && response.RecommendationEngine.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "new recommendation engine insert");

                    if (!SendConfigurationToAdapter(groupID, response.RecommendationEngine))
                    {
                        log.ErrorFormat("InsertRecommendationEngine - SendConfigurationToAdapter failed : AdapterID = {0}", response.RecommendationEngine.ID);
                    }
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to insert new recommendation engine ");
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }
            return response;
        }

        public static ApiObjects.Response.Status DeleteRecommendationEngine(int groupID, int recommendationEngineId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {

                if (recommendationEngineId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineIdentifierRequired, RECOMMENDATION_ENGINE_ID_REQUIRED);
                    return response;
                }

                //TODO:Anat Check the logic for delete 

                //// in case recommendationEngineId is the group selected recommendationEngine  - delete isn’t allowed
                ////-------------------------------------------------------------------------------
                //object reommendationEngineAdapter = ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "RECOMMENDATION_ENGINE", "GROUP_ID", "=", groupID);
                //int recommendationEngineIdentifier = 0;
                //if (reommendationEngineAdapter != null && int.TryParse(reommendationEngineAdapter.ToString(), out recommendationEngineIdentifier) && recommendationEngineIdentifier > 0)
                //{
                //    if (recommendationEngineIdentifier == recommendationEngineId)
                //    {
                //        response = new ApiObjects.Response.Status((int)eResponseStatus.ActionIsNotAllowed, ACTION_IS_NOT_ALLOWED);
                //        return response;
                //    }
                //}

                //check if recommendationEngineId exist
                RecommendationEngine recommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngineId);

                if (recommendationEngine == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineNotExist, RECOMMENDATION_ENGINE_NOT_EXIST);
                    return response;
                }

                bool isSet = CatalogDAL.DeleteRecommendationEngine(groupID, recommendationEngineId);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "recommendation engine deleted");

                    string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                    string[] keys = new string[1] 
                    { 
                        string.Format("{0}_recommendation_engine_{1}", version, recommendationEngineId)
                    };

                    QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineNotExist, RECOMMENDATION_ENGINE_NOT_EXIST);
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, recommendationEngineId={1}", groupID, recommendationEngineId), ex);
            }
            return response;
        }

        public static RecommendationEngineResponse SetRecommendationEngine(int groupID, RecommendationEngine recommendationEngine)
        {
            RecommendationEngineResponse response = new RecommendationEngineResponse();

            try
            {
                if (recommendationEngine == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoRecommendationEngineToUpdate, NO_RECOMMENDATION_ENGINE_TO_UPDATE);
                    return response;
                }

                if (recommendationEngine.ID == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineIdentifierRequired, RECOMMENDATION_ENGINE_ID_REQUIRED);
                    return response;
                }
                if (string.IsNullOrEmpty(recommendationEngine.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(recommendationEngine.ExternalIdentifier))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(recommendationEngine.AdapterUrl))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                    return response;
                }

                // SharedSecret generated only at insert 
                // this value not relevant at update and should be ignored
                //--------------------------------------------------------
                recommendationEngine.SharedSecret = null;

                //check recommendation engine exist
                RecommendationEngine originalRecommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngine.ID);
                if (originalRecommendationEngine == null || originalRecommendationEngine.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineNotExist, RECOMMENDATION_ENGINE_NOT_EXIST);
                    return response;
                }

                //check External Identifier uniqueness 
                RecommendationEngine currentRecommendationEngine = CatalogDAL.GetRecommendationEngineInternalID(groupID, recommendationEngine.ExternalIdentifier);

                if (currentRecommendationEngine != null && currentRecommendationEngine.ID > 0 && recommendationEngine.ID != currentRecommendationEngine.ID)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierMustBeUnique, ERROR_EXT_ID_ALREADY_IN_USE);
                    return response;
                }

                response.RecommendationEngine = CatalogDAL.SetRecommendationEngine(groupID, recommendationEngine);

                if (response.RecommendationEngine != null && response.RecommendationEngine.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "recommendation engine set changes");
                    log.DebugFormat("recommendation engine id  {0} set database changes", recommendationEngine.ID);

                    if (!recommendationEngine.SkipSettings)
                    {
                        bool isSet = CatalogDAL.SetRecommendationEngineSettings(groupID, recommendationEngine.ID, recommendationEngine.Settings);
                        if (isSet)
                        {
                            response.RecommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngine.ID);
                        }
                        else
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "recommendation engine failed set settings");
                        }
                    }

                    string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                    string[] keys = new string[1]{ 
                        string.Format("{0}_recommendation_engine_{1}", version, recommendationEngine.ID)
                    };

                    QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
                    log.DebugFormat("recommendation engine id  {0} UpdateCache", recommendationEngine.ID);

                    bool isSendSucceeded = SendConfigurationToAdapter(groupID, response.RecommendationEngine);
                    log.DebugFormat("recommendation engine id  {0} SendConfigurationToAdapter", recommendationEngine.ID);
                    if (!isSendSucceeded)
                    {
                        log.DebugFormat("SetRecommendationEngine - SendConfigurationToAdapter failed : AdapterID = {0}", recommendationEngine.ID);
                    }
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "recommendation engine failed set changes");
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, recommendationEngineId={1}, name={2}, adapterUrl={3}, isActive={4}",
                    groupID, recommendationEngine.ID, recommendationEngine.Name, recommendationEngine.AdapterUrl, recommendationEngine.IsActive), ex);
            }
            return response;
        }

        private static bool SendConfigurationToAdapter(int groupId, RecommendationEngine recommendationEngine)
        {
            try
            {
                if (recommendationEngine != null && !string.IsNullOrEmpty(recommendationEngine.AdapterUrl))
                {
                    //set unixTimestamp
                    long unixTimestamp = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                    //set signature
                    string signature = string.Concat(recommendationEngine.ID, recommendationEngine.Settings != null ? string.Concat(recommendationEngine.Settings.Select(s => string.Concat(s.key, s.value))) : string.Empty,
                        groupId, unixTimestamp);

                    using (AdapterControllers.RecommendationEngineAdapter.ServiceClient client = new AdapterControllers.RecommendationEngineAdapter.ServiceClient(string.Empty, recommendationEngine.AdapterUrl))
                    {
                        if (!string.IsNullOrEmpty(recommendationEngine.AdapterUrl))
                        {
                            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(recommendationEngine.AdapterUrl);
                        }

                        AdapterControllers.RecommendationEngineAdapter.AdapterStatus adapterResponse = client.SetConfiguration(
                            recommendationEngine.ID,
                            recommendationEngine.Settings != null ? recommendationEngine.Settings.Select(s => new AdapterControllers.RecommendationEngineAdapter.KeyValue() { Key = s.key, Value = s.value }).ToArray() : null,
                            groupId,
                            unixTimestamp,
                            System.Convert.ToBase64String(TVinciShared.EncryptUtils.AesEncrypt(recommendationEngine.SharedSecret, TVinciShared.EncryptUtils.HashSHA1(signature))));

                        if (adapterResponse != null && adapterResponse.Code == (int)OSSAdapterStatus.OK)
                        {
                            log.DebugFormat("RecommendationEngine SetConfiguration Result: AdapterID = {0}, AdapterStatus = {1}", recommendationEngine.ID, adapterResponse.Code);
                            return true;
                        }
                        else
                        {
                            log.ErrorFormat("RecommendationEngine SetConfiguration Result: AdapterID = {0}, AdapterStatus = {1}",
                                recommendationEngine.ID, adapterResponse != null ? adapterResponse.Code.ToString() : "ERROR");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SendConfigurationToAdapter Failed: AdapterID = {0}, ex = {1}", recommendationEngine.ID, ex);
            }
            return false;
        }

        public static RecommendationEnginesResponseList GetRecommendationEngines(int groupID)
        {
            RecommendationEnginesResponseList response = new RecommendationEnginesResponseList();
            try
            {
                response.RecommendationEngines = CatalogDAL.GetRecommendationEngineList(groupID);
                if (response.RecommendationEngines == null || response.RecommendationEngines.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no recommendation engine related to group");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new RecommendationEnginesResponseList();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }

        public static RecommendationEnginesResponseList ListRecommendationEngines(int groupID)
        {
            RecommendationEnginesResponseList response = new RecommendationEnginesResponseList();
            try
            {
                response.RecommendationEngines = CatalogDAL.ListRecommendationEngineList(groupID);
                if (response.RecommendationEngines == null || response.RecommendationEngines.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no recommendation engine related to group");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new RecommendationEnginesResponseList();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }

        public static ApiObjects.Response.Status InsertRecommendationEngineSettings(int groupID, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (recommendationEngineId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineIdentifierRequired, RECOMMENDATION_ENGINE_ID_REQUIRED);
                    return response;
                }
                if (settings == null || settings.Count == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineParamsRequired, NO_PARAMS_TO_INSERT);
                    return response;
                }

                //check recommendation engine exist
                RecommendationEngine recommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngineId);
                if (recommendationEngine == null || recommendationEngine.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineNotExist, RECOMMENDATION_ENGINE_NOT_EXIST);
                    return response;
                }

                int matchingKeyAmount = GetMatchingKeyAmount(recommendationEngine.Settings, settings);
                if (matchingKeyAmount > 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ConflictedParams, CONFLICTED_PARAMS);
                    return response;
                }

                bool isInsert = CatalogDAL.InsertRecommendationEngineSettings(groupID, recommendationEngineId, settings);
                if (isInsert)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "recommendation engine configs insert");

                    //Get recommendationEngine updated                        
                    recommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngineId);
                    if (!SendConfigurationToAdapter(groupID, recommendationEngine))
                    {
                        log.ErrorFormat("InsertRecommendationEngineSettings - SendConfigurationToAdapter failed : recommendationEngineId = {0}", recommendationEngineId);
                    }
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to insert recommendation engine configs");
                }
            }
            catch (Exception ex)
            {

                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, recommendationEngineId={1}", groupID, recommendationEngineId), ex);
            }
            return response;

        }

        private static int GetMatchingKeyAmount(List<RecommendationEngineSettings> originalList, List<RecommendationEngineSettings> settings)
        {
            int matchingKeyAmount = 0;
            RecommendationEngineSettings result;
            foreach (RecommendationEngineSettings originalSettings in originalList)
            {
                result = settings.Find(x => x.key == originalSettings.key);
                if (result != null)
                {
                    matchingKeyAmount++; ;
                }

            }

            return matchingKeyAmount;
        }

        public static ApiObjects.Response.Status SetRecommendationEngineSettings(int groupID, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            try
            {
                if (recommendationEngineId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineIdentifierRequired, RECOMMENDATION_ENGINE_ID_REQUIRED);
                }

                if (settings == null || settings.Count == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterParamsRequired, NO_PARAMS_TO_INSERT);
                    return response;
                }

                //check recommendation engine exist
                RecommendationEngine recommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngineId);
                if (recommendationEngine == null || recommendationEngine.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineNotExist, RECOMMENDATION_ENGINE_NOT_EXIST);
                    return response;
                }

                int matchingKeyAmount = GetMatchingKeyAmount(recommendationEngine.Settings, settings);
                if (matchingKeyAmount != settings.Count)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ConflictedParams, CONFLICTED_PARAMS);
                    return response;
                }

                bool isSet = CatalogDAL.SetRecommendationEngineSettings(groupID, recommendationEngineId, settings);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "recommendation engine set changes");

                    string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                    string[] keys = new string[1] 
                    { 
                        string.Format("{0}_recommendation_engine_{1}", version, recommendationEngineId)
                    };

                    QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
                    //Get recommendation engine  updated                        
                    recommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngineId);

                    if (!SendConfigurationToAdapter(groupID, recommendationEngine))
                    {
                        log.ErrorFormat("SetRecommendationEngineSettings - SendConfigurationToAdapter failed : AdapterID = {0}", recommendationEngineId);
                    }
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "recommendation engine failed set changes, check your params");
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID = {0} , recommendationEngineId={1}", groupID, recommendationEngineId), ex);
            }
            return response;
        }

        public static ApiObjects.Response.Status DeleteRecommendationEngineSettings(int groupID, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (recommendationEngineId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineIdentifierRequired, RECOMMENDATION_ENGINE_ID_REQUIRED);
                    return response;
                }

                if (settings == null || settings.Count == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineParamsRequired, NO_PARAMS_TO_DELETE);
                    return response;
                }

                //check recommendation engine exist
                RecommendationEngine recommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngineId);
                if (recommendationEngine == null || recommendationEngine.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineNotExist, RECOMMENDATION_ENGINE_NOT_EXIST);
                    return response;
                }

                int matchingKeyAmount = GetMatchingKeyAmount(recommendationEngine.Settings, settings);
                if (matchingKeyAmount != settings.Count)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ConflictedParams, CONFLICTED_PARAMS);
                    return response;
                }

                bool isSet = CatalogDAL.DeleteRecommendationEngineSettings(groupID, recommendationEngineId, settings);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "recommendation engine configs delete");

                    string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                    string[] keys = new string[1] 
                    { 
                        string.Format("{0}_recommendation_engine_{1}", version, recommendationEngineId)
                    };

                    QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
                    //Get recommendation engine updated                        
                    recommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngineId);
                    if (!SendConfigurationToAdapter(groupID, recommendationEngine))
                    {
                        log.ErrorFormat("DeleteRecommendationEngineSettings - SendConfigurationToAdapter failed : AdapterID = {0}", recommendationEngineId);
                    }
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "recommendation engine configs faild delete");
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, recommendationEngineId={1}", groupID, recommendationEngineId), ex);
            }
            return response;
        }

        public static RecommendationEngineSettinsResponse GetRecommendationEngineSettings(int groupID)
        {
            RecommendationEngineSettinsResponse response = new RecommendationEngineSettinsResponse();
            try
            {
                response.RecommendationEngines = CatalogDAL.GetRecommendationEngineSettingsList(groupID, 0);
                if (response.RecommendationEngines == null || response.RecommendationEngines.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no recommendation engine  config related to group");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new RecommendationEngineSettinsResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID = {0} ", groupID), ex);
            }

            return response;
        }

        public static RecommendationEngineResponse UpdateRecommendationEngineConfiguration(int groupID, int recommendationEngineId)
        {
            RecommendationEngineResponse response = new RecommendationEngineResponse();

            try
            {
                if (recommendationEngineId == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineIdentifierRequired, RECOMMENDATION_ENGINE_ID_REQUIRED);
                    return response;
                }

                RecommendationEngine engine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngineId);

                if (engine == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineNotExist, RECOMMENDATION_ENGINE_NOT_EXIST);
                }

                AdapterControllers.RecommendationAdapterController adapterController = AdapterControllers.RecommendationAdapterController.GetInstance();

                bool result = adapterController.SendConfiguration(engine, groupID);

                if (result)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "recommendation engine configuration sent successfully");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "recommendation engine failed to get changes");
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, recommendationEngineId={1}",
                    groupID, recommendationEngineId), ex);
            }
            return response;
        }

        public static RecommendationEngineResponse GenerateRecommendationEngineSharedSecret(int groupID, int recommendationEngineId)
        {
            RecommendationEngineResponse response = new RecommendationEngineResponse();

            try
            {
                if (recommendationEngineId <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineIdentifierRequired, RECOMMENDATION_ENGINE_ID_REQUIRED);
                    return response;
                }

                //check recommendation engine exist
                RecommendationEngine recommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, recommendationEngineId);
                if (recommendationEngine == null || recommendationEngine.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineNotExist, RECOMMENDATION_ENGINE_NOT_EXIST);
                    return response;
                }

                // Create Shared secret 
                string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                response.RecommendationEngine = CatalogDAL.SetRecommendationEngineSharedSecret(groupID, recommendationEngineId, sharedSecret);

                if (response.RecommendationEngine != null && response.RecommendationEngine.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "recommendation engine generate shared secret");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "recommendation engine  failed set changes");
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, recommendationEngineId={1}", groupID, recommendationEngineId), ex);
            }
            return response;
        }


        #endregion

        #region External Channels
        public static ExternalChannelResponse InsertExternalChannel(int groupID, ExternalChannel externalChannel)
        {
            ExternalChannelResponse response = new ExternalChannelResponse();

            try
            {
                if (externalChannel == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoExternalChannelToInsert, NO_EXTERNAL_CHANNEL_TO_INSERT);
                    return response;
                }

                if (string.IsNullOrEmpty(externalChannel.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(externalChannel.ExternalIdentifier))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                    return response;
                }

                //check External Identiifer uniqueness 
                ExternalChannel returnExternalChannel = CatalogDAL.GetExternalChannelInternalID(groupID, externalChannel.ExternalIdentifier);

                if (returnExternalChannel != null && returnExternalChannel.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierMustBeUnique, ERROR_EXT_ID_ALREADY_IN_USE);
                    return response;
                }

                if (externalChannel.RecommendationEngineId <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineIdentifierRequired, RECOMMENDATION_ENGINE_ID_REQUIRED);
                    return response;
                }

                RecommendationEngine recommendationEngine = CatalogDAL.GetRecommendationEngine(groupID, externalChannel.RecommendationEngineId);

                if (recommendationEngine == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.RecommendationEngineNotExist, RECOMMENDATION_ENGINE_NOT_EXIST);
                    return response;
                }

                // Validate enrichments
                if (externalChannel.Enrichments != null)
                {
                    List<ExternalRecommendationEngineEnrichment> availableEnrichments = CatalogDAL.GetAvailableEnrichments();

                    foreach (var currentEnrichment in externalChannel.Enrichments)
                    {
                        // If the used enrichment is not available
                        if (!availableEnrichments.Contains(currentEnrichment))
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.InactiveExternalChannelEnrichment,
                                string.Format("External channel enrichment {0} is inactive", currentEnrichment.ToString()));
                            return response;
                        }
                    }
                }

                externalChannel.GroupId = groupID;

                response.ExternalChannel = CatalogDAL.InsertExternalChannel(groupID, externalChannel);
                if (response.ExternalChannel != null && response.ExternalChannel.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "new external channel insert");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to insert new external channel");
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }
            return response;
        }

        public static ApiObjects.Response.Status DeleteExternalChannel(int groupID, int externalChannelId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {

                if (externalChannelId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ExternalChannelIdentifierRequired, EXTERNAL_CHANNEL_ID_REQUIRED);
                    return response;
                }

                //check external channel exist
                ExternalChannel originalExternalChannel = CatalogDAL.GetExternalChannelById(groupID, externalChannelId);
                if (originalExternalChannel == null || externalChannelId <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ExternalChannelNotExist, EXTERNAL_CHANNEL_NOT_EXIST);
                    return response;
                }

                bool isSet = CatalogDAL.DeleteExternalChannel(groupID, externalChannelId);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "external channel deleted");
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ExternalChannelNotExist, EXTERNAL_CHANNEL_NOT_EXIST);
                }

                string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                string[] keys = new string[1] 
                { 
                    string.Format("{0}_external_channel_{1}_{2}", version, groupID, externalChannelId)
                };

                QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, externalChannelId={1}", groupID, externalChannelId), ex);
            }
            return response;
        }

        public static ExternalChannelResponse SetExternalChannel(int groupID, ExternalChannel externalChannel)
        {
            ExternalChannelResponse response = new ExternalChannelResponse();

            try
            {
                if (externalChannel == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoExternalChannelToUpdate, NO_EXTERNAL_CHANNEL_TO_UPDTAE);
                    return response;
                }

                if (externalChannel.ID == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalChannelIdentifierRequired, EXTERNAL_CHANNEL_ID_REQUIRED);
                    return response;
                }
                if (string.IsNullOrEmpty(externalChannel.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(externalChannel.ExternalIdentifier))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, EXTERNAL_IDENTIFIER_REQUIRED);
                    return response;
                }

                //check external channel exist
                ExternalChannel originalExternalChannel = CatalogDAL.GetExternalChannelById(groupID, externalChannel.ID);
                if (originalExternalChannel == null || externalChannel.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalChannelNotExist, EXTERNAL_CHANNEL_NOT_EXIST);
                    return response;
                }

                //check External Identifier uniqueness 
                ExternalChannel returnExternalChannel = CatalogDAL.GetExternalChannelInternalID(groupID, externalChannel.ExternalIdentifier);

                if (returnExternalChannel != null && returnExternalChannel.ID > 0 && externalChannel.ID != returnExternalChannel.ID)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierMustBeUnique, ERROR_EXT_ID_ALREADY_IN_USE);
                    return response;
                }

                // Validate enrichments
                if (externalChannel.Enrichments != null)
                {
                    List<ExternalRecommendationEngineEnrichment> availableEnrichments = CatalogDAL.GetAvailableEnrichments();

                    foreach (var currentEnrichment in externalChannel.Enrichments)
                    {
                        // If the used enrichment is not available
                        if (!availableEnrichments.Contains(currentEnrichment))
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.InactiveExternalChannelEnrichment,
                                string.Format("External channel enrichment {0} is inactive", currentEnrichment.ToString()));
                            return response;
                        }
                    }
                }

                externalChannel.GroupId = groupID;

                response.ExternalChannel = CatalogDAL.SetExternalChannel(groupID, externalChannel);

                if (response.ExternalChannel != null && response.ExternalChannel.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "external channel set changes");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "external channel failed set changes");
                }

                string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                string[] keys = new string[1] 
                { 
                    string.Format("{0}_external_channel_{1}_{2}", version, groupID, externalChannel.ID)
                };

                QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }
            return response;
        }

        public static ExternalChannelResponseList GetExternalChannels(int groupID)
        {
            ExternalChannelResponseList response = new ExternalChannelResponseList();
            try
            {
                response.ExternalChannels = CatalogDAL.GetExternalChannel(groupID);
                if (response.ExternalChannels == null || response.ExternalChannels.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no external channels related to group");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new ExternalChannelResponseList();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }

        public static ExternalChannelResponseList ListExternalChannels(int groupID)
        {
            ExternalChannelResponseList response = new ExternalChannelResponseList();
            try
            {
                response.ExternalChannels = CatalogDAL.ListExternalChannel(groupID);
                if (response.ExternalChannels == null || response.ExternalChannels.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no external channels related to group");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new ExternalChannelResponseList();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }
        #endregion

        #region Bulk Export

        public static BulkExportTaskResponse AddBulkExportTask(int groupId, string externalKey, string name, eBulkExportDataType dataType, string filter, eBulkExportExportType exportType,
            long frequency, string notificationUrl, List<int> vodTypes, bool isActive)
        {
            BulkExportTaskResponse response = new BulkExportTaskResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            if (string.IsNullOrEmpty(externalKey))
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AliasRequired, ALIAS_REQUIRED);
                return response;
            }

            if (string.IsNullOrEmpty(notificationUrl))
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExportNotificationUrlRequired, EXPORT_NOTIFICATION_URL_REQUIRED);
                return response;
            }

            int frequencyMinValue = TVinciShared.WS_Utils.GetTcmIntValue("export.frequency_min_value");
            if (frequency < frequencyMinValue)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExportFrequencyMinValue, string.Format(EXPORT_FREQUENCY_MIN_VALUE_FORMAT, frequencyMinValue));
                return response;
            }

            try
            {
                var tasks = DAL.ApiDAL.GetBulkExportTasks(null, new List<string>() { externalKey }, groupId);
                if (tasks != null && tasks.Count > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AliasMustBeUnique, ALIAS_ALREADY_EXISTS);
                    return response;
                }

                // generate task version 
                string version = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow).ToString();

                // insert task
                response.Task = DAL.ApiDAL.InsertBulkExportTask(groupId, externalKey, name, dataType, filter, exportType, frequency, notificationUrl, vodTypes, version, isActive);

                if (response.Task != null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    // insert new message to tasks queue (for celery)
                    EnqueueExportTask(groupId, response.Task.Id, version);
                }
            }
            catch (Exception ex)
            {
                log.Error("error in AddBulkExportTask", ex);
            }
            return response;
        }

        public static BulkExportTaskResponse UpdateBulkExportTask(int groupId, long id, string externalKey, string name, eBulkExportDataType dataType, string filter, eBulkExportExportType exportType, long frequency,
            string notificationUrl, List<int> vodTypes, bool? isActive)
        {
            BulkExportTaskResponse response = new BulkExportTaskResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                // if update by ID - validate external key is unique
                if (id != 0)
                {
                    var tasks = DAL.ApiDAL.GetBulkExportTasks(null, new List<string>() { externalKey }, groupId);
                    if (tasks != null && tasks.Count > 0)
                    {
                        if (tasks.Count > 1 || tasks[0].Id != id)
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AliasMustBeUnique, SYSTEM_NAME_ALREADY_EXISTS);
                            return response;
                        }
                    }
                }

                if (string.IsNullOrEmpty(notificationUrl))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExportNotificationUrlRequired, EXPORT_NOTIFICATION_URL_REQUIRED);
                    return response;
                }

                int frequencyMinValue = TVinciShared.WS_Utils.GetTcmIntValue("export.frequency_min_value");
                if (frequency < frequencyMinValue)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExportFrequencyMinValue, string.Format(EXPORT_FREQUENCY_MIN_VALUE_FORMAT, frequencyMinValue));
                    return response;
                }
                // generate task version 
                string version = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow).ToString();

                response.Task = DAL.ApiDAL.UpdateBulkExportTask(groupId, id, externalKey, name, dataType, filter, exportType, frequency, notificationUrl, vodTypes, version, isActive);

                if (response.Task != null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    // insert new message to tasks queue (for celery)
                    EnqueueExportTask(groupId, response.Task.Id, version);
                }

            }
            catch (Exception ex)
            {
                log.Error("error in UpdateBulkExportTask", ex);
            }

            return response;
        }

        public static bool DeleteBulkExportTask(int groupId, long id, string externalKey)
        {
            try
            {
                return DAL.ApiDAL.DeleteBulkExportTask(groupId, id, externalKey);
            }
            catch (Exception ex)
            {
                log.Error("error in DeleteBulkExportTask", ex);
                return false;
            }
        }

        public static List<ApiObjects.BulkExport.BulkExportTask> GetBulkExportTasks(List<long> ids, List<string> externalKeys, int groupId, BulkExportTaskOrderBy orderBy)
        {
            List<ApiObjects.BulkExport.BulkExportTask> response = new List<ApiObjects.BulkExport.BulkExportTask>();
            try
            {
                response = DAL.ApiDAL.GetBulkExportTasks(ids, externalKeys, groupId, (int)orderBy);
            }
            catch (Exception ex)
            {
                log.Error("error in GetBulkExportTasks", ex);
                response = null;
            }

            return response;
        }

        public static bool Export(int groupId, long taskId, string version)
        {
            log.DebugFormat("Export: starting export process. task id = {0}, version = {1}", taskId, version);

            // get task
            ApiObjects.BulkExport.BulkExportTask task = null;
            List<ApiObjects.BulkExport.BulkExportTask> tasks = GetBulkExportTasks(new List<long>() { taskId }, null, groupId, BulkExportTaskOrderBy.CreateDateAsc);

            // task not found
            if (tasks == null || tasks.Count == 0)
            {
                log.ErrorFormat("Export: task was not found. task id = {0}", taskId);
                return false;
            }

            task = tasks[0];

            // validate task is active
            if (!task.IsActive)
            {
                log.ErrorFormat("Export: task is not active. task id = {0}, supplied version = {1}, task version = {2}", taskId, version, task.Version);
                return true;
            }

            // validate task
            // task is not deleted - will not be returned.
            // task version is the same as version
            if (task.Version != version)
            {
                log.ErrorFormat("Export: task version does not match supplied version. task id = {0}, supplied version = {1}, task version = {2}", taskId, version, task.Version);
                return true;
            }

            // insert new message to tasks queue (for celery)
            EnqueueExportTask(groupId, taskId, version, task.Frequency);

            // task is already in process
            if (task.InProcess)
            {
                log.ErrorFormat("Export: task already in process. task id = {0}", taskId);
                return false;
            }

            // start process - set task's in_process property to be true
            if (!DAL.ApiDAL.SetBulkExportTaskProcess(task.Id, true))
            {
                log.ErrorFormat("Export: failed to update task's process status. taskId = {0}, process status = true", task.Id);
                return false;
            }

            bool processSucceeded = false;
            string filename = null;
            try
            {
                processSucceeded = ExportLogic.Export(groupId, task, out filename);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Export: error in export process for task id = {0}", taskId), ex);
                processSucceeded = false;
            }

            // export process finished - set task's process status and update the last process date if process succeeded
            DateTime? processEndTime = null;
            if (processSucceeded)
            {
                processEndTime = DateTime.UtcNow;
            }

            if (!DAL.ApiDAL.SetBulkExportTaskProcess(task.Id, false, processEndTime))
            {
                log.ErrorFormat("Export: failed to set task's process status after process was finished. taskId = {0}", taskId);
            }

            // send notification - if notification url supplied
            if (!string.IsNullOrEmpty(task.NotificationUrl))
            {
                if (!ExportLogic.SendNotification(taskId, task.NotificationUrl, processSucceeded, filename))
                {
                    log.ErrorFormat("Export: failed to send notification for task id = {0}, to url = {1}, with filename = {2}, after {3} export",
                        taskId, task.NotificationUrl, filename, processSucceeded ? "successful" : "not successful");
                }
            }
            log.DebugFormat("Export: finished export process {0}. task id = {1}, version = {2}", processSucceeded ? "successfully" : "not successfully", taskId, version);

            return processSucceeded;
        }

        public static ApiObjects.Response.Status EnqueueExportTask(int groupId, long taskId)
        {
            // get task
            ApiObjects.BulkExport.BulkExportTask task = null;
            List<ApiObjects.BulkExport.BulkExportTask> tasks = GetBulkExportTasks(new List<long>() { taskId }, null, groupId, BulkExportTaskOrderBy.CreateDateAsc);

            // task not found
            if (tasks != null && tasks.Count > 0)
            {
                task = tasks[0];
            }
            else
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.ExportTaskNotFound, EXPORT_TASK_NOT_FOUND);
            }

            return EnqueueExportTask(groupId, taskId, task.Version);
        }

        public static ApiObjects.Response.Status EnqueueExportTask(int groupId, long taskId, string version, long taskFrequency = 0)
        {
            log.DebugFormat("EnqueueExportTask: inserting task to rabbit mq. task id = {0}, version = {1}, frequency {2}", taskId, version, taskFrequency);

            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                ExportTaskData data = null;

                // insert new message to tasks queue (for celery)
                ExportTasksQueue queue = new ExportTasksQueue();
                DateTime eta = DateTime.UtcNow.AddMinutes(taskFrequency);
                if (taskFrequency != 0)
                {
                    queue.storeForRecovery = true;
                    data = new ExportTaskData(groupId, taskId, version, eta);
                }
                else
                {
                    data = new ExportTaskData(groupId, taskId, version);
                }
                log.DebugFormat("EnqueueExportTask: inserting data to rabbit mq. data = ", data);

                if (queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_EXPORT, groupId)))
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    log.DebugFormat("EnqueueExportTask: successfully inserted task to rabbit mq. task id = {0}, version = {1}, frequency {2}", taskId, version, taskFrequency);
                }
                else
                {
                    log.ErrorFormat("Enqueue of new export task failed", data);
                }
            }
            catch (Exception ex)
            {
                log.Error("Enqueue of new export task failed", ex);
            }

            return status;
        }
        #endregion

        public static ApiObjects.Response.Status MessageRecovery(int groupId, long baseDateSec, List<string> messageDataTypes)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            string statusMessage = "Recovery count {0} , succeeded {1} ";
            int totalCount = 0;
            int succeeded = 0;

            StringBuilder sb = new StringBuilder();

            try
            {
                // get all requested messages
                List<MessageQueue> messages = DAL.ApiDAL.GetQueueMessages(groupId, baseDateSec, messageDataTypes);

                // send to queue
                if (messages != null && messages.Count > 0)
                {

                    log.DebugFormat("recovery is sending {0} new messages", messages.Count);

                    BaseQueue queue = null;
                    totalCount = messages.Count;

                    log.DebugFormat("recovery is sending {0} new messages", totalCount);

                    foreach (var message in messages)
                    {
                        // get queue type
                        queue = InitializationEnqueue(message.Type);

                        if (queue != null)
                        {
                            try
                            {
                                queue.RecoverMessages(groupId, message.MessageData, message.RoutingKey, message.Type);
                                succeeded++;
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat("Error reflecting class for recovery. group ID: {0}, message data: {1}. exc {2}", groupId, message.MessageData, ex);
                            }
                        }
                        else
                        {
                            log.ErrorFormat("Error reflecting class for recovery. group ID: {0}, message type: {1}",
                                groupId,
                                message.Type != null ? message.Type : string.Empty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while running message recovery", ex);
                response.Code = (int)eResponseStatus.Error;
                response.Message = "Error while running message recovery";
            }

            response.Message = string.Format(statusMessage, totalCount, succeeded);
            log.Debug(response.Message);
            return response;
        }

        private static BaseQueue InitializationEnqueue(string type)
        {
            try
            {
                // get assembly
                Assembly testAssembly = Assembly.Load(QUEUE_ASSEMBLY_NAME);

                // get type of class Calculator from just loaded assembly
                Type dataType = testAssembly.GetType(type);

                // return instance
                return (BaseQueue)Activator.CreateInstance(dataType);
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to reflect " + type, ex);
            }

            return null;
        }

        public static ApiObjects.Roles.RolesResponse GetRoles(int groupId, List<long> roleIds)
        {
            RolesResponse response = new RolesResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                response.Roles = DAL.ApiDAL.GetRoles(groupId, roleIds);
                if (response.Roles != null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while getting roles. group id = {0}", groupId), ex);
            }

            return response;
        }

        public static ApiObjects.Roles.PermissionsResponse GetPermissions(int groupId, List<long> permissionIds)
        {
            PermissionsResponse response = new PermissionsResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                response.Permissions = DAL.ApiDAL.GetPermissions(groupId, permissionIds);
                if (response.Permissions != null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while getting permissions. group id = {0}", groupId), ex);
            }

            return response;
        }

        public static PermissionResponse AddPermission(int groupId, string name, List<long> permissionItemsIds, ePermissionType type, string usersGroup, long updaterId)
        {
            PermissionResponse response = new PermissionResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                response.Permission = DAL.ApiDAL.InsertPermission(groupId, name, permissionItemsIds, type, usersGroup, updaterId);
                if (response.Permission != null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while adding permission. group id = {0}", groupId), ex);
            }

            return response;
        }

        public static ApiObjects.Response.Status AddPermissionToRole(int groupId, long roleId, long permissionId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                int rowCount = DAL.ApiDAL.InsertRolePermission(groupId, roleId, permissionId);
                if (rowCount > 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while adding permission to role. group id = {0}", groupId), ex);
            }

            return response;
        }

        public static ApiObjects.Response.Status AddPermissionItemToPermission(int groupId, long permissionId, long permissionItemId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                int rowCount = DAL.ApiDAL.InsertPermissionPermissionItem(groupId, permissionId, permissionItemId);
                if (rowCount > 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while getting permissions. group id = {0}", groupId), ex);
            }

            return response;
        }

        public static bool UpdateImageState(int groupId, long rowId, int version, eMediaType mediaType, eTableStatus state)
        {
            return ImageUtils.UpdateImageState(groupId, rowId, version, mediaType, state);
        }

        public static UnifiedSearchResult[] GetChannelAssets(int channelId, int groupId, int pageIndex, int pageSize)
        {
            UnifiedSearchResult[] assets = null;

            try
            {
                string catalogSignString = Guid.NewGuid().ToString();
                string catalogSignatureString = GetWSURL("CatalogSignatureKey");

                string catalogSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, catalogSignatureString);

                try
                {
                    InternalChannelRequest channelRequest = new InternalChannelRequest()
                    {
                        internalChannelID = channelId.ToString(),
                        m_nGroupID = groupId,
                        m_oFilter = new Filter()
                        {
                            m_bOnlyActiveMedia = false,
                            //m_bUseStartDate = fals
                        },
                        m_sSignature = catalogSignature,
                        m_sSignString = catalogSignString,
                        m_nPageIndex = pageIndex,
                        m_nPageSize = pageSize,
                        m_bIgnoreDeviceRuleID = true
                    };

                    BaseResponse response = channelRequest.GetResponse(channelRequest);
                    assets = ((UnifiedSearchResponse)response).searchResults.ToArray();
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("channel:{0}, msg:{1}", channelId, ex.Message), ex);
                }
            }
            catch (Exception ex)
            {
                log.Error("Configuration Reading - Couldn't read values from configuration ", ex);
            }

            return assets;
        }

        public static OSSAdapterEntitlementsResponse GetExternalEntitlements(int groupID, string userId)
        {
            OSSAdapterEntitlementsResponse response = new OSSAdapterEntitlementsResponse();

            try
            {
                int ossAdapterIdentifier = 0;
                var ossAdapterId = ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "OSS_ADAPTER", "GROUP_ID", "=", groupID, "BILLING_CONNECTION_STRING");

                if (ossAdapterId != null && ossAdapterId != DBNull.Value && int.TryParse(ossAdapterId.ToString(), out ossAdapterIdentifier))
                {
                    log.DebugFormat("GetOSSAdapterPaymentGateway ossAdapterIdentifier = {0}", ossAdapterIdentifier);
                }

                if (ossAdapterIdentifier == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                    return response;
                }

                // Get Oss Adapter Data                
                OSSAdapter ossAdapter = DAL.ApiDAL.GetOSSAdapter(groupID, ossAdapterIdentifier, 1);

                if (ossAdapter == null || ossAdapter.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                    return response;
                }

                // call oss Adapter
                if (string.IsNullOrEmpty(ossAdapter.AdapterUrl))
                {
                    response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "oss adapter has no adapter URL" };
                    return response;
                }

                APILogic.OSSAdapterService.EntitlementsResponse adapterResponse = AdaptersController.GetInstance(ossAdapter.ID).GetEntitlements(groupID, ossAdapter, userId);

                if (adapterResponse == null || adapterResponse.Status == null)
                {
                    log.ErrorFormat("GetEntitlements: OSS Adapter response is null. ossAdapterId = {0}, userId = {1}", ossAdapterId, userId);
                    response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "OSS Adapter response is null" };
                    return response;
                }

                if (adapterResponse.Status.Code != (int)OSSAdapterStatus.OK)
                {
                    log.ErrorFormat("GetEntitlements: OSS Adapter Response not ok. Message = {0}, Code = {1}",
                        adapterResponse.Status.Message != null ? adapterResponse.Status.Message : string.Empty, adapterResponse.Status.Code);

                    response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.AdapterAppFailure, Message = "OSS Adapter failed" };
                    return response;
                }

                // adapter response is ok - build local response
                response.Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK };

                if (adapterResponse.Entitlements != null)
                {
                    response.Entitlements = new List<ExternalEntitlement>();
                    foreach (var adapterEntitlement in adapterResponse.Entitlements)
                    {
                        if (adapterEntitlement != null)
                        {
                            response.Entitlements.Add(new ExternalEntitlement()
                            {
                                ProductCode = adapterEntitlement.Alias,
                                EntitlementType = (eTransactionType)adapterEntitlement.EntitlementType,
                                ContentId = adapterEntitlement.ContentId,
                                StartDateSeconds = adapterEntitlement.StartDateSeconds,
                                EndDateSeconds = adapterEntitlement.EndDateSeconds,
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetExternalEntitlements groupID = {0}, userId = {1}", groupID, userId), ex);
            }

            return response;
        }

        public static bool ModifyCB(string bucket, string key, eDbActionType action, string data, long ttlMinutes = 0)
        {
            return DalCB.General_Couchbase.ModifyCB(bucket, key, action, data, ttlMinutes);
        }

        public static RegistryResponse GetAllRegistry(int groupId)
        {
            RegistryResponse response = new RegistryResponse();
            try
            {
                response.registrySettings = DAL.ApiDAL.GetAllRegistry(groupId);
                if (response.registrySettings == null || (response.registrySettings != null && response.registrySettings.Count == 0))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "no registry settings found");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed GetAllRegistry groupID = {0}", groupId), ex);
            }

            return response;
        }

        public static bool InitializeFreeItemsUpdate(int groupId)
        {
            bool result = false;

            try
            {
                int lastMediaFileID = 0;
                List<KeyValuePair<int, DateTime>> freeItemsToInitialize = DAL.ApiDAL.GetFreeItemsToInitialize(groupId, ref lastMediaFileID);
                if (freeItemsToInitialize == null || freeItemsToInitialize.Count == 0)
                {
                    log.Debug("GetFreeItemsToInitialize didn't return any free items to initialize");
                    result = false;
                }

                int totalAssetsToInitialize = 0;
                int totalEnqueuedItems = 0;
                while (freeItemsToInitialize != null && freeItemsToInitialize.Count > 0)
                {
                    totalAssetsToInitialize += freeItemsToInitialize.Count;
                    foreach (KeyValuePair<int, DateTime> itemToUpdate in freeItemsToInitialize)
                    {
                        if (RabbitHelper.InsertFreeItemsIndexUpdate(groupId, eObjectType.Media, new List<int>() { itemToUpdate.Key }, itemToUpdate.Value))
                        {
                            totalEnqueuedItems++;
                        }
                    }

                    freeItemsToInitialize = DAL.ApiDAL.GetFreeItemsToInitialize(groupId, ref lastMediaFileID);
                }

                result = (totalAssetsToInitialize == totalEnqueuedItems);

                if (!result)
                {
                    log.Error(string.Format("InitializeFreeItemsUpdate failed initializing all Free items - Total free items to Initialize: {0} , Total enqueued items {1}",
                                totalAssetsToInitialize, totalEnqueuedItems));
                }
            }

            catch (Exception ex)
            {
                log.Error("InitializeFreeItemsUpdate - " + string.Format("Error in InitializeFreeItemsUpdate: groupId = {0} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        public static bool UpdateFreeFileTypeOfModule(int groupID, int moduleID)
        {
            bool result = false;

            try
            {
                result = ImporterImpl.UpdateFreeFileTypeOfModule(groupID, moduleID);
            }
            catch (Exception ex)
            {

                log.Error("InitializeFreeItemsUpdate - " + string.Format("Error in InitializeFreeItemsUpdate: groupID = {0}, moduleID = {1}, ex = {2}, ST = {3}", groupID, moduleID, ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        public static TimeShiftedTvPartnerSettingsResponse GetTimeShiftedTvPartnerSettings(int groupID)
        {
            TimeShiftedTvPartnerSettingsResponse response = new TimeShiftedTvPartnerSettingsResponse();

            try
            {
                DataRow dr = DAL.ApiDAL.GetTimeShiftedTvPartnerSettings(groupID);
                if (dr == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.TimeShiftedTvPartnerSettingsNotFound, eResponseStatus.TimeShiftedTvPartnerSettingsNotFound.ToString());
                }
                else
                {
                    int catchup = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_catch_up", 0);
                    int cdvr = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_cdvr", 0);
                    int startOver = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_start_over", 0);
                    int trickPlay = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_trick_play", 0);
                    long catchUpBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "catch_up_buffer", 7);
                    long trickPlayBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "trick_play_buffer", 1);
                    long recordingScheduleWindowBuffer = ODBCWrapper.Utils.GetLongSafeVal(dr, "recording_schedule_window_buffer", 0);
                    int recordingScheduleWindow = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_recording_schedule_window", -1);
                    long paddingAfterProgramEnds = ODBCWrapper.Utils.GetLongSafeVal(dr, "padding_after_program_ends", 0);
                    long paddingBeforeProgramStarts = ODBCWrapper.Utils.GetLongSafeVal(dr, "padding_before_program_starts", 0);
                    int protection = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_protection", 0);
                    int protectionPeriod = ODBCWrapper.Utils.GetIntSafeVal(dr, "protection_period", 90);
                    int protectionQuotaPercentage = ODBCWrapper.Utils.GetIntSafeVal(dr, "protection_quota_percentage", 25);
                    int recordingLifetimePeriod = ODBCWrapper.Utils.GetIntSafeVal(dr, "recording_lifetime_period", 182);
                    int cleanupNoticePeriod = ODBCWrapper.Utils.GetIntSafeVal(dr, "cleanup_notice_period", 7);
                    int series_recording = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_series_recording", 1); //Default = enabled
                    int enable_recording_playback_non_entitled = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_recording_playback_non_entitled", 0); // Default = disabled
                    int enable_recording_playback_non_existing = ODBCWrapper.Utils.GetIntSafeVal(dr, "enable_recording_playback_non_existing", 0); // Default = disabled
                    int quotaOveragePolicy = ODBCWrapper.Utils.GetIntSafeVal(dr, "quota_overage_policy", 0);
                    int protectionPolicy = ODBCWrapper.Utils.GetIntSafeVal(dr, "protection_policy", 0); 

                    if (recordingScheduleWindow > -1)
                    {
                        response.Settings = new TimeShiftedTvPartnerSettings(catchup == 1, cdvr == 1, startOver == 1, trickPlay == 1, recordingScheduleWindow == 1, catchUpBuffer,
                                                                    trickPlayBuffer, recordingScheduleWindowBuffer, paddingAfterProgramEnds, paddingBeforeProgramStarts,
                                                                    protection == 1, protectionPeriod, protectionQuotaPercentage, recordingLifetimePeriod, cleanupNoticePeriod,
                                                                    series_recording == 1, enable_recording_playback_non_entitled == 1, enable_recording_playback_non_existing == 1, quotaOveragePolicy, protectionPolicy);
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error("GetTimeShiftedTvPartnerSettings - " + string.Format("Error in GetTimeShiftedTvPartnerSettings: groupID = {0} ex = {1}", groupID, ex.Message, ex.StackTrace), ex);
            }

            return response;
        }

        public static ApiObjects.Response.Status UpdateTimeShiftedTvPartnerSettings(int groupID, TimeShiftedTvPartnerSettings settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                if (settings == null || (settings.IsCatchUpEnabled == null && settings.IsCdvrEnabled == null && settings.IsStartOverEnabled == null && settings.IsTrickPlayEnabled == null
                                         && settings.CatchUpBufferLength == null && settings.TrickPlayBufferLength == null && settings.IsRecordingScheduleWindowEnabled == null
                                         && settings.RecordingScheduleWindow == null && settings.PaddingBeforeProgramStarts == null && settings.PaddingAfterProgramEnds == null
                                         && settings.ProtectionPeriod == null && settings.ProtectionQuotaPercentage == null && settings.IsSeriesRecordingEnabled == null
                                         && settings.IsRecordingPlaybackNonEntitledChannelEnabled == null && settings.IsRecordingPlaybackNonExistingChannelEnabled == null
                                         && !settings.QuotaOveragePolicy.HasValue && !settings.ProtectionPolicy.HasValue ))
                {
                    response.Code = (int)ApiObjects.Response.eResponseStatus.TimeShiftedTvPartnerSettingsNotSent;
                    response.Message = ApiObjects.Response.eResponseStatus.TimeShiftedTvPartnerSettingsNotSent.ToString();
                }
                else if ((settings.CatchUpBufferLength.HasValue && settings.CatchUpBufferLength.Value < 0) || (settings.TrickPlayBufferLength.HasValue && settings.TrickPlayBufferLength.Value < 0))
                {
                    response.Code = (int)ApiObjects.Response.eResponseStatus.TimeShiftedTvPartnerSettingsNegativeBufferSent;
                    response.Message = ApiObjects.Response.eResponseStatus.TimeShiftedTvPartnerSettingsNegativeBufferSent.ToString();
                }
                else
                {
                    if (DAL.ApiDAL.UpdateTimeShiftedTvPartnerSettings(groupID, settings))
                    {
                        response = UpdateTimeShiftedTvEpgChannelsSettings(groupID, settings);
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error("UpdateTimeShiftedTvPartnerSettings - " + string.Format("Error in UpdateTimeShiftedTvPartnerSettings: groupID = {0}, settings = {1}, ex = {2}", groupID, ex.Message, settings.ToString(), ex.StackTrace), ex);
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }

            return response;
        }

        public static CDNAdapterListResponse GetCDNAdapters(int groupID)
        {
            CDNAdapterListResponse response = new CDNAdapterListResponse();
            try
            {
                response.Adapters = DAL.ApiDAL.GetCDNAdapters(groupID);
                if (response.Adapters == null || response.Adapters.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "No adapters found");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new CDNAdapterListResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }

        public static ApiObjects.Response.Status DeleteCDNAdapter(int groupID, int adapterId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {

                if (adapterId <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.AdapterIdentifierRequired, ADAPTER_ID_REQUIRED);
                    return response;
                }

                //check Adapter exists                
                CDNAdapter adapter = DAL.ApiDAL.GetCDNAdapter(adapterId, false);
                if (adapter == null || adapter.ID <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.AdapterNotExists, ADAPTER_NOT_EXIST);
                    return response;
                }

                bool isSet = DAL.ApiDAL.DeleteCDNAdapter(groupID, adapterId);
                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.AdapterNotExists, ADAPTER_NOT_EXIST);
                }

                string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                string[] keys = new string[1] 
                    { 
                        string.Format("{0}_cdn_adapter_{1}", version, adapterId)
                    };

                QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);

            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, adapterID={1}", groupID, adapterId), ex);
            }
            return response;
        }

        public static CDNAdapterResponse InsertCDNAdapter(int groupID, ApiObjects.CDNAdapter.CDNAdapter adapter)
        {
            CDNAdapterResponse response = new CDNAdapterResponse();

            try
            {
                if (adapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoAdapterToInsert, NO_ADAPTER_TO_INSERT);
                    return response;
                }

                if (string.IsNullOrEmpty(adapter.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(adapter.AdapterUrl))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(adapter.SystemName))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AliasRequired, SYSTEM_NAME_REQUIRED);
                    return response;
                }

                // Create Shared secret 
                adapter.SharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                //check alias uniqueness 
                CDNAdapter responseAdapter = DAL.ApiDAL.GetCDNAdapterByAlias(groupID, adapter.SystemName);

                if (responseAdapter != null && responseAdapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AliasMustBeUnique, SYSTEM_NAME_ALREADY_EXISTS);
                    return response;
                }

                // get regular groupId
                int regularGroupId = DAL.ApiDAL.GetCdnRegularGroupId(groupID);
                if (regularGroupId == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    log.ErrorFormat("Failed getting regularGroupId for group: {0}", groupID);
                    return response;
                }
                response.Adapter = DAL.ApiDAL.InsertCDNAdapter(regularGroupId, adapter);
                if (response.Adapter != null && response.Adapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    if (!SendConfigurationToCdnAdapter(groupID, response.Adapter))
                    {
                        log.ErrorFormat("InsertCDNAdapter - SendConfigurationToCdnAdapter failed : adapterID = {0}", response.Adapter.ID);
                    }

                    // remove adapter from cache
                    string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                    string[] keys = new string[1] 
                    { 
                        string.Format("{0}_cdn_adapter_{1}", version, adapter.ID)
                    };

                    QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "failed to insert new CDN Adapter");
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }
            return response;
        }

        public static CDNAdapterResponse GenerateCDNSharedSecret(int groupID, int adapterId)
        {
            CDNAdapterResponse response = new CDNAdapterResponse();

            try
            {
                if (adapterId <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterIdentifierRequired, ADAPTER_ID_REQUIRED);
                    return response;
                }

                //check CDN Adapter exists
                CDNAdapter adapter = DAL.ApiDAL.GetCDNAdapter(adapterId);
                if (adapter == null || adapter.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterNotExists, ADAPTER_NOT_EXIST);
                    return response;
                }

                // Create Shared secret 
                string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                response.Adapter = DAL.ApiDAL.SetCDNAdapterSharedSecret(groupID, adapterId, sharedSecret);

                if (response.Adapter != null && response.Adapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    bool isSendSucceeded = SendConfigurationToCdnAdapter(groupID, response.Adapter);
                    if (!isSendSucceeded)
                    {
                        log.DebugFormat("SetCDNAdapter - SendConfigurationToCdnAdapter failed : AdapterID = {0}", adapter.ID);
                    }

                    // remove adapter from cache
                    string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                    string[] keys = new string[1] 
                    { 
                        string.Format("{0}_cdn_adapter_{1}", version,adapterId)
                    };

                    QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, adapterId={1}", groupID, adapterId), ex);
            }
            return response;
        }

        public static CDNAdapterResponse SetCDNAdapter(int groupID, ApiObjects.CDNAdapter.CDNAdapter adapter, int adapterId)
        {
            CDNAdapterResponse response = new CDNAdapterResponse();

            try
            {
                if (adapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterIsRequired, NO_ADAPTER_TO_UPDATE);
                    return response;
                }

                if (adapterId <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterIdentifierRequired, ADAPTER_ID_REQUIRED);
                    return response;
                }
                if (string.Empty == adapter.Name)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.Empty == adapter.SystemName)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AliasRequired, SYSTEM_NAME_REQUIRED);
                    return response;
                }

                if (string.Empty == adapter.AdapterUrl)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                    return response;
                }

                // SharedSecret generated only at insert 
                // this value not relevant at update and should be ignored
                //--------------------------------------------------------
                adapter.SharedSecret = null;

                //check alias uniqueness 
                CDNAdapter responseAdpater = DAL.ApiDAL.GetCDNAdapterByAlias(groupID, adapter.SystemName);

                if (responseAdpater != null && responseAdpater.ID > 0 && responseAdpater.ID != adapterId)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AliasMustBeUnique, SYSTEM_NAME_ALREADY_EXISTS);
                    return response;
                }

                if (responseAdpater == null)
                {
                    //check adapter exists 
                    responseAdpater = DAL.ApiDAL.GetCDNAdapter(adapterId, false);
                    if (responseAdpater == null)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterNotExists, ADAPTER_NOT_EXIST);
                        return response;
                    }
                }

                if (adapter.Settings == null)
                {
                    adapter.Settings = responseAdpater.Settings;
                }
                response.Adapter = DAL.ApiDAL.SetCDNAdapter(groupID, adapterId, adapter);

                if (response.Adapter != null && response.Adapter.ID > 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    bool isSendSucceeded = SendConfigurationToCdnAdapter(groupID, response.Adapter);
                    if (!isSendSucceeded)
                    {
                        log.DebugFormat("SetCDNAdapter - SendConfigurationToCdnAdapter failed : AdapterID = {0}", adapterId);
                    }

                    // remove adapter from cache
                    string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                    string[] keys = new string[1] 
                    { 
                        string.Format("{0}_cdn_adapter_{1}", version, adapterId)
                    };

                    QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, adapterId={1}, name={2}, baseUrl={3}, isActive={4}",
                    groupID, adapterId, adapter.Name, adapter.BaseUrl, adapter.IsActive), ex);
            }
            return response;
        }

        public static CDNAdapterResponse SendCDNConfigurationToAdapter(int groupID, int adapterID)
        {
            CDNAdapterResponse response = new CDNAdapterResponse();

            try
            {
                if (adapterID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterIdentifierRequired, ADAPTER_ID_REQUIRED);
                    return response;
                }

                // get adapter from DB 
                //check alias uniqueness 
                CDNAdapter adapter = DAL.ApiDAL.GetCDNAdapter(adapterID);

                if (adapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterNotExists, ADAPTER_NOT_EXIST);
                    return response;
                }
                if (string.IsNullOrEmpty(adapter.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(adapter.SystemName))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AliasRequired, SYSTEM_NAME_REQUIRED);
                    return response;
                }

                if (string.IsNullOrEmpty(adapter.AdapterUrl))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterUrlRequired, ADAPTER_URL_REQUIRED);
                    return response;
                }

                bool isSetSucceeded = SendConfigurationToCdnAdapter(groupID, adapter);
                if (!isSetSucceeded)
                {
                    log.DebugFormat("SetCDNAdapter - SendConfigurationToCdnAdapter failed : AdapterID = {0}", adapter.ID);
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, adapterId={1}", groupID, adapterID), ex);
            }
            return response;
        }

        private static bool SendConfigurationToCdnAdapter(int groupId, CDNAdapter adapter)
        {
            try
            {
                CDNAdapterController cdnAdapter = CDNAdapterController.GetInstance();
                return cdnAdapter.SendConfiguration(adapter, groupId);
            }
            catch
            {
                log.ErrorFormat("SendConfigurationToCdnAdapter failed : groupID = {0}, AdapterID = {1}", groupId, adapter.ID);
                return false;
            }
        }

        public static CDNPartnerSettingsResponse GetCDNPartnerSettings(int groupId)
        {
            CDNPartnerSettingsResponse response = new CDNPartnerSettingsResponse();

            try
            {
                response.CDNPartnerSettings = DAL.ApiDAL.GetCdnSettings(groupId);
                if (response.CDNPartnerSettings == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.CDNPartnerSettingsNotFound, eResponseStatus.CDNPartnerSettingsNotFound.ToString());
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetCDNPartnerSettings: groupID = {0} ex = {1}", groupId, ex.Message), ex);
            }

            return response;
        }

        public static CDNPartnerSettingsResponse UpdateCDNPartnerSettings(int groupId, CDNPartnerSettings settings)
        {
            CDNPartnerSettingsResponse response = new CDNPartnerSettingsResponse();

            try
            {
                List<int> adaptersIds = new List<int>();

                if (settings.DefaultRecordingAdapter.HasValue && settings.DefaultRecordingAdapter.Value != 0)
                {
                    adaptersIds.Add(settings.DefaultRecordingAdapter.Value);
                }
                if (settings.DefaultAdapter.HasValue && settings.DefaultAdapter.Value != 0)
                {
                    adaptersIds.Add(settings.DefaultAdapter.Value);
                }

                var groupAdpters = DAL.ApiDAL.GetCDNAdapters(groupId);

                foreach (var adapterId in adaptersIds)
                {
                    if (groupAdpters.Where(a => a.ID == adapterId).FirstOrDefault() == null)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterNotExists, string.Format("Adapter ID = {0} does not exist", adapterId));
                        return response;
                    }
                }

                response.CDNPartnerSettings = DAL.ApiDAL.UpdateCdnSettings(groupId, settings.DefaultAdapter, settings.DefaultRecordingAdapter);
                if (response.CDNPartnerSettings == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.CDNPartnerSettingsNotFound, eResponseStatus.CDNPartnerSettingsNotFound.ToString());
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in UpdateCDNPartnerSettings: groupID = {0} ex = {1}", groupId, ex.Message), ex);
            }

            return response;
        }

        public static CDNAdapterResponse GetCdnAdapter(int groupId, int adapterId)
        {
            CDNAdapterResponse response = new CDNAdapterResponse();

            try
            {
                CDNAdapter adapter = CdnAdapterCache.Instance.GetCdnAdapter(groupId, adapterId);
                if (adapter == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterNotExists, eResponseStatus.AdapterNotExists.ToString());
                }
                else
                {
                    response.Adapter = adapter;
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetCdnAdapter: adapterId = {0} ex = {1}", adapterId, ex.Message), ex);
            }

            return response;
        }

        public static CDNAdapterResponse GetGroupDefaultCdnAdapter(int groupId, eAssetTypes assetType)
        {
            CDNAdapterResponse response = new CDNAdapterResponse();

            try
            {
                // get group cdn settings
                CDNPartnerSettings settings = CdnAdapterCache.Instance.GetCdnAdapterSettings(groupId);

                if (settings != null)
                {
                    int defaultAdapterId;

                    // get the id of the default adapter for the relevant asset type
                    switch (assetType)
                    {
                        case eAssetTypes.EPG:
                            defaultAdapterId = settings.DefaultAdapter.HasValue ? settings.DefaultAdapter.Value : 0;
                            break;
                        case eAssetTypes.NPVR:
                            defaultAdapterId = settings.DefaultRecordingAdapter.HasValue ? settings.DefaultRecordingAdapter.Value : 0;
                            break;
                        case eAssetTypes.MEDIA:
                            defaultAdapterId = settings.DefaultAdapter.HasValue ? settings.DefaultAdapter.Value : 0;
                            break;
                        default:
                            defaultAdapterId = 0;
                            break;
                    }

                    // if there is no default adapter configured return adapter not exists
                    if (defaultAdapterId == 0)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                        return response;
                    }

                    // get the adapter                   
                    CDNAdapter cdnAdapter = CdnAdapterCache.Instance.GetCdnAdapter(groupId, defaultAdapterId);
                    if (cdnAdapter == null)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AdapterNotExists, eResponseStatus.AdapterNotExists.ToString());
                    }
                    else
                    {
                        response.Adapter = cdnAdapter;
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetCdnAdapter: groupId = {0}, assetType = {1}, ex = {2}", groupId, assetType, ex.Message), ex);
            }

            return response;
        }

        public static ApiObjects.Response.Status UpdateTimeShiftedTvEpgChannelsSettings(int groupId, TimeShiftedTvPartnerSettings settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                if (settings == null || (settings.IsCatchUpEnabled == null && settings.IsCdvrEnabled == null && settings.IsStartOverEnabled == null
                        && settings.IsTrickPlayEnabled == null && settings.CatchUpBufferLength == null && settings.TrickPlayBufferLength == null))
                {
                    if (settings.IsSeriesRecordingEnabled != null || settings.IsRecordingPlaybackNonEntitledChannelEnabled != null || settings.IsRecordingPlaybackNonExistingChannelEnabled != null)
                    {
                        response.Code = (int)eResponseStatus.OK;
                        response.Message = eResponseStatus.OK.ToString();
                    }
                    else
                    {
                        response.Code = (int)ApiObjects.Response.eResponseStatus.TimeShiftedTvPartnerSettingsNotSent;
                        response.Message = ApiObjects.Response.eResponseStatus.TimeShiftedTvPartnerSettingsNotSent.ToString();
                    }
                }
                else if ((settings.CatchUpBufferLength.HasValue && settings.CatchUpBufferLength.Value < 0) ||
                    (settings.TrickPlayBufferLength.HasValue && settings.TrickPlayBufferLength.Value < 0))
                {
                    response.Code = (int)ApiObjects.Response.eResponseStatus.TimeShiftedTvPartnerSettingsNegativeBufferSent;
                    response.Message = ApiObjects.Response.eResponseStatus.TimeShiftedTvPartnerSettingsNegativeBufferSent.ToString();
                }
                else
                {
                    List<int> epgChannelsIds = DAL.ApiDAL.GetEpgChannelIdsWithNoCatchUp(groupId);
                    if (epgChannelsIds != null && epgChannelsIds.Count > 0)
                    {
                        bool res = Catalog.Module.UpdateEpgChannelIndex(epgChannelsIds, groupId, eAction.Update);
                        if (res)
                        {
                            response.Code = (int)eResponseStatus.OK;
                            response.Message = eResponseStatus.OK.ToString();
                        }
                    }
                    else
                    {
                        response.Code = (int)eResponseStatus.OK;
                        response.Message = eResponseStatus.OK.ToString();
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error("UpdateTimeShiftedTvPartnerSettingsTVM - " + string.Format("Error in UpdateTimeShiftedTvPartnerSettingsTVM: groupID = {0}, settings = {1}, ex = {2}", groupId, ex.Message, settings.ToString(), ex.StackTrace), ex);
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }

            return response;
        }

        public static bool MigrateStatistics(int groupId, DateTime? startDate)
        {
            bool result = false;

            var queue = new SetupTasksQueue();

            var dynamicData = new Dictionary<string, object>();

            if (startDate != null && startDate.HasValue)
            {
                dynamicData.Add("START_DATE", startDate.Value.ToString("yyyyMMddHHmmss"));
            }

            var queueObject = new CelerySetupTaskData(groupId, eSetupTask.MigrateStatistics, dynamicData);

            try
            {
                result = queue.Enqueue(queueObject, "MIGRATE_STATISTICS");
            }
            catch (Exception ex)
            {
                log.Error("MigrateStatistics - " +
                        string.Format("Error in MigrateStatistics: group = {0} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace),
                        ex);
            }

            return result;
        }

        public static ScheduledTaskLastRunDetails GetScheduledTaskLastRun(ScheduledTaskType scheduledTaskType)
        {
            ScheduledTaskLastRunDetails scheduledTask = GetScheduledTaskImplementationByType(scheduledTaskType);
            if (scheduledTask != null)
            {
                scheduledTask = scheduledTask.GetLastRunDetails();
                if (scheduledTask != null)
                {
                    scheduledTask.EstimatedNextRunDate = scheduledTask.LastRunDate.AddSeconds(scheduledTask.NextRunIntervalInSeconds);
                    return scheduledTask;
                }
            }

            return null;
        }

        public static bool UpdateScheduledTaskNextRunIntervalInSeconds(ScheduledTaskType scheduledTaskType, double nextRunIntervalInSeconds)
        {
            ScheduledTaskLastRunDetails scheduledTask = GetScheduledTaskImplementationByType(scheduledTaskType);
            if (scheduledTask != null)
            {
                return scheduledTask.SetNextRunIntervalInSeconds(nextRunIntervalInSeconds);
            }

            return false;
        }

        private static ScheduledTaskLastRunDetails GetScheduledTaskImplementationByType(ScheduledTaskType scheduledTaskType)
        {
            ScheduledTaskLastRunDetails scheduledTask = null;
            switch (scheduledTaskType)
            {
                case ScheduledTaskType.recordingsLifetime:
                case ScheduledTaskType.recordingsScheduledTasks:
                case ScheduledTaskType.recordingsCleanup:
                case ScheduledTaskType.notificationCleanup:
                case ScheduledTaskType.reminderCleanup:
                default:
                    scheduledTask = new BaseScheduledTaskLastRunDetails(scheduledTaskType);
                    break;
            }

            return scheduledTask;
        }

        public static UnifiedSearchResult[] SearchAssets(int groupID, string filter, int pageIndex, int pageSize, bool OnlyIsActive, int languageID, bool UseStartDate,
               string Udid, string UserIP, string SiteGuid, int DomainId, int ExectGroupId, bool IgnoreDeviceRule)
        {
            UnifiedSearchResult[] assets = null;

            try
            {
                string catalogSignString = Guid.NewGuid().ToString();
                string catalogSignatureString = GetWSURL("CatalogSignatureKey");

                string catalogSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, catalogSignatureString);

                try
                {
                    UnifiedSearchRequest assetRequest = new UnifiedSearchRequest()
                    {
                        m_nGroupID = groupID,
                        m_oFilter = new Filter()
                        {
                            m_bOnlyActiveMedia = OnlyIsActive,
                            m_nLanguage = languageID,
                            m_bUseStartDate = UseStartDate,
                            m_sDeviceId = Udid,
                        },
                        m_sSignature = catalogSignature,
                        m_sSignString = catalogSignString,
                        m_nPageIndex = 0,
                        m_nPageSize = pageSize,
                        m_sUserIP = UserIP,
                        filterQuery = filter,
                        exactGroupId = ExectGroupId,
                        shouldIgnoreDeviceRuleID = IgnoreDeviceRule,
                        order = new OrderObj()
                        {
                            m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID,
                            m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC
                        },
                        m_sSiteGuid = SiteGuid,
                        domainId = DomainId
                    };

                    BaseResponse response = assetRequest.GetResponse(assetRequest);
                    assets = ((UnifiedSearchResponse)response).searchResults.ToArray();
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("SearchAssets filter :{0}, languageID:{1}, Udid:{2}, UserIP:{3}, SiteGuid:{4}, DomainId:{5},OnlyIsActive:{6}, UseStartDate:{7},msg:{8}",
                        filter, languageID, Udid, UserIP, SiteGuid, DomainId, OnlyIsActive, UseStartDate, ex.Message), ex);
                }
            }
            catch (Exception ex)
            {
                log.Error("Configuration Reading - Couldn't read values from configuration ", ex);
            }

            return assets;
        }

        public static DeviceFamilyResponse GetDeviceFamilyList()
        {
            DeviceFamilyResponse result = new DeviceFamilyResponse();
            DataTable dt = DAL.ApiDAL.GetDeviceFamilies();
            if (dt != null && dt.Rows != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    if (dr != null)
                    {
                        int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID", 0);
                        string name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                        if (id > 0 && !string.IsNullOrEmpty(name))
                        {
                            DeviceFamily deviceFamily = new DeviceFamily(id, name);
                            result.DeviceFamilies.Add(deviceFamily);
                        }
                    }
                }

                result.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                result.TotalItems = dt.Rows.Count;
            }

            return result;
        }

        public static DeviceBrandResponse GetDeviceBrandList()
        {
            DeviceBrandResponse result = new DeviceBrandResponse();
            DataTable dt = DAL.ApiDAL.GetDeviceBrands();
            if (dt != null && dt.Rows != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    if (dr != null)
                    {
                        int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID", 0);
                        string name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                        int deviceFamilyId = ODBCWrapper.Utils.GetIntSafeVal(dr, "DEVICE_FAMILY_ID", 0);
                        if (id > 0 && !string.IsNullOrEmpty(name))
                        {
                            DeviceBrand deviceFamily = new DeviceBrand(id, name, deviceFamilyId);
                            result.DeviceBrands.Add(deviceFamily);
                        }
                    }
                }

                result.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                result.TotalItems = dt.Rows.Count;
            }

            return result;
        }

        public static ApiObjects.CountryResponse GetCountryList(List<int> countryIds)
        {
            ApiObjects.CountryResponse result = new ApiObjects.CountryResponse();
            DataTable dt = DAL.ApiDAL.GetCountries(countryIds);
            if (dt != null && dt.Rows != null)
            {
                ApiObjects.Country country;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    if (dr != null)
                    {
                        country = new ApiObjects.Country()
                        {
                            Id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID", 0),
                            Name = ODBCWrapper.Utils.GetSafeStr(dr, "COUNTRY_NAME"),
                            Code = ODBCWrapper.Utils.GetSafeStr(dr, "COUNTRY_CD2"),
                        };
                        if (country.Id > 0)
                        {
                            result.Countries.Add(country);
                        }
                    }
                }

                result.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return result;
        }

        public static MetaResponse GetGroupMetaList(int groupId, eAssetTypes assetType, ApiObjects.MetaType metaType, MetaFieldName fieldNameEqual, MetaFieldName fieldNameNotEqual)
        {
            MetaResponse response = new MetaResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                response.MetaList = new List<Meta>();

                if (assetType == eAssetTypes.EPG || assetType == eAssetTypes.NPVR || assetType == eAssetTypes.UNKNOWN)
                {
                    Dictionary<string, Meta> metaDict = new Dictionary<string, Meta>();

                    var mappings = CatalogDAL.GetAliasMappingFields(groupId);

                    // just the mapping
                    if (mappings != null && mappings.Count > 0)
                    {
                        Meta meta;
                        foreach (var map in mappings)
                        {
                            meta = new Meta()
                            {
                                AssetType = eAssetTypes.EPG,
                                Name = map.Name,
                                Type = map.FieldType == FieldTypes.Tag ? ApiObjects.MetaType.Tag : ApiObjects.MetaType.String
                            };

                            if (metaType == ApiObjects.MetaType.All || metaType == meta.Type)
                            {
                                switch (map.Alias)
                                {
                                    case "series_id":
                                        {
                                            if (fieldNameEqual == MetaFieldName.SeriesId || (fieldNameEqual == MetaFieldName.All && (fieldNameNotEqual != MetaFieldName.SeriesId
                                                || fieldNameNotEqual == MetaFieldName.None)) && fieldNameEqual != MetaFieldName.None)
                                            {
                                                meta.FieldName = MetaFieldName.SeriesId;
                                                metaDict.Add(meta.Name, meta);
                                            }
                                        }
                                        break;
                                    case "season_number":
                                        {
                                            if (fieldNameEqual == MetaFieldName.SeasonNumber || (fieldNameEqual == MetaFieldName.All && (fieldNameNotEqual != MetaFieldName.SeasonNumber
                                                || fieldNameNotEqual == MetaFieldName.None)) && fieldNameEqual != MetaFieldName.None)
                                            {
                                                meta.FieldName = MetaFieldName.SeasonNumber;
                                                metaDict.Add(meta.Name, meta);
                                            }
                                        }
                                        break;
                                    case "episode_number":
                                        {
                                            if (fieldNameEqual == MetaFieldName.EpisodeNumber || (fieldNameEqual == MetaFieldName.All && (fieldNameNotEqual != MetaFieldName.EpisodeNumber
                                                || fieldNameNotEqual == MetaFieldName.None)) && fieldNameEqual != MetaFieldName.None)
                                            {
                                                meta.FieldName = MetaFieldName.EpisodeNumber;
                                                metaDict.Add(meta.Name, meta);
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    if ((fieldNameEqual == MetaFieldName.All || fieldNameEqual == MetaFieldName.None) && fieldNameNotEqual != MetaFieldName.None &&
                        (metaType == ApiObjects.MetaType.String || metaType == ApiObjects.MetaType.Tag || metaType == ApiObjects.MetaType.All))
                    {
                        // all the rest
                        DataSet ds = CatalogDAL.Get_MetasByGroup(groupId, null);
                        if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && (metaType == ApiObjects.MetaType.String || metaType == ApiObjects.MetaType.All))
                        {
                            DataTable dt = ds.Tables[0];
                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {
                                response.MetaList = new List<Meta>();
                                Meta meta;
                                foreach (DataRow dr in dt.Rows)
                                {
                                    string name = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                                    if (!metaDict.ContainsKey(name))
                                    {
                                        meta = new Meta()
                                        {
                                            AssetType = eAssetTypes.EPG,
                                            FieldName = MetaFieldName.None,
                                            Name = name,
                                            Type = ApiObjects.MetaType.String
                                        };

                                        metaDict.Add(name, meta);
                                    }
                                }
                            }

                            if (ds.Tables.Count > 1 && (metaType == ApiObjects.MetaType.Tag || metaType == ApiObjects.MetaType.All))
                            {
                                dt = ds.Tables[1];
                                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                                {
                                    response.MetaList = new List<Meta>();
                                    Meta meta;
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        string name = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                                        if (!metaDict.ContainsKey(name))
                                        {
                                            meta = new Meta()
                                            {
                                                AssetType = eAssetTypes.EPG,
                                                FieldName = MetaFieldName.None,
                                                Name = name,
                                                Type = ApiObjects.MetaType.Tag
                                            };

                                            metaDict.Add(name, meta);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    response.MetaList = metaDict.Values.ToList();
                }
                if (assetType == eAssetTypes.MEDIA || assetType == eAssetTypes.UNKNOWN)
                {
                    GroupsCacheManager.Group group = GroupsCacheManager.GroupsCache.Instance().GetGroup(groupId);

                    if (group != null)
                    {
                        Meta meta;

                        if (metaType != ApiObjects.MetaType.Tag && group.m_oMetasValuesByGroupId != null)
                        {
                            foreach (var groupMetas in group.m_oMetasValuesByGroupId.Values)
                            {
                                foreach (var metaVal in groupMetas)
                                {
                                    meta = new Meta()
                                    {
                                        AssetType = eAssetTypes.MEDIA,
                                        FieldName = MetaFieldName.None,
                                        Name = metaVal.Value,
                                        Type = APILogic.Utils.GetMetaTypeByDbName(metaVal.Key)
                                    };

                                    if (meta.Type == metaType || metaType == ApiObjects.MetaType.All)
                                    {
                                        response.MetaList.Add(meta);
                                    }
                                }
                            }
                        }

                        if ((metaType == ApiObjects.MetaType.Tag || metaType == ApiObjects.MetaType.All) && group.m_oGroupTags != null)
                        {
                            foreach (var tagVal in group.m_oGroupTags)
                            {
                                meta = new Meta()
                                {
                                    AssetType = eAssetTypes.MEDIA,
                                    FieldName = MetaFieldName.None,
                                    Name = tagVal.Value,
                                    Type = ApiObjects.MetaType.Tag
                                };

                                response.MetaList.Add(meta);
                            }
                        }
                    }
                }

                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to get meta for group = {0}", groupId), ex);
            }
            return response;
        }

        public static string GetLayeredCacheGroupConfig(int groupId)
        {
            string groupConfigResult = string.Empty;

            try
            {
                LayeredCacheGroupConfig groupConfig = LayeredCache.Instance.GetLayeredCacheGroupConfig(groupId);
                if (groupConfig != null)
                {
                    groupConfigResult = groupConfig.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetLayeredCacheGroupConfig for groupId: {0}", groupId), ex);
            }

            return groupConfigResult;
        }

        public static bool UpdateLayeredCacheGroupConfig(int groupId, int? version, bool? disableLayeredCache, List<string> layeredCacheSettingsToExclude, bool? shouldOverrideExistingExludeSettings)
        {
            bool result = false;

            try
            {
                result = LayeredCache.Instance.SetLayeredCacheGroupConfig(groupId, version, disableLayeredCache, layeredCacheSettingsToExclude, shouldOverrideExistingExludeSettings);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateLayeredCacheGroupConfig for groupId: {0}", groupId), ex);
            }

            return result;
        }

        public static bool DoActionRules()
        {
            double alcrScheduledTaskIntervalSec = 0;
            bool shouldEnqueueFollowUp = false;            
            try
            {
                // try to get interval for next run take default
                BaseScheduledTaskLastRunDetails assetLifeCycleRuleScheduledTask = new BaseScheduledTaskLastRunDetails(ScheduledTaskType.assetLifeCycleRuleScheduledTasks);
                ScheduledTaskLastRunDetails lastRunDetails = assetLifeCycleRuleScheduledTask.GetLastRunDetails();
                assetLifeCycleRuleScheduledTask = lastRunDetails != null ? (BaseScheduledTaskLastRunDetails)lastRunDetails : null;
                if (assetLifeCycleRuleScheduledTask != null && assetLifeCycleRuleScheduledTask.Status.Code == (int)eResponseStatus.OK && assetLifeCycleRuleScheduledTask.NextRunIntervalInSeconds > 0)
                {
                    alcrScheduledTaskIntervalSec = assetLifeCycleRuleScheduledTask.NextRunIntervalInSeconds;
                    if (assetLifeCycleRuleScheduledTask.LastRunDate.AddSeconds(alcrScheduledTaskIntervalSec - MAX_SERVER_TIME_DIF) > DateTime.UtcNow)
                    {
                        return true;
                    }
                    else
                    {                        
                        shouldEnqueueFollowUp = true;
                    }
                }
                else
                {
                    shouldEnqueueFollowUp = true;
                    alcrScheduledTaskIntervalSec = HANDLE_ASSET_LIFE_CYCLE_RULE_SCHEDULED_TASKS_INTERVAL_SEC;
                }

                int impactedItems = AssetLifeCycleRuleManager.Instance.DoActionRules();
                if (impactedItems > 0)
                {
                    log.DebugFormat("Successfully applied asset life cycle rules on: {0} assets", impactedItems);
                }
                else
                {
                    log.DebugFormat("No assets were modified on DoActionRules");
                }

                assetLifeCycleRuleScheduledTask = new BaseScheduledTaskLastRunDetails(DateTime.UtcNow, impactedItems, alcrScheduledTaskIntervalSec, ScheduledTaskType.assetLifeCycleRuleScheduledTasks);
                if (!assetLifeCycleRuleScheduledTask.SetLastRunDetails())
                {
                    log.InfoFormat("Failed updating asset life cycle rules scheduled task last run details, AssetLifeCycleRuleScheduledTask: {0}", assetLifeCycleRuleScheduledTask.ToString());
                }
                else
                {
                    log.Debug("Successfully updated asset life cycle rules scheduled task last run date");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in DoActionRules", ex);
                shouldEnqueueFollowUp = true;
            }
            finally
            {               
                if (shouldEnqueueFollowUp)
                {
                    if (alcrScheduledTaskIntervalSec == 0)
                    {
                        alcrScheduledTaskIntervalSec = HANDLE_ASSET_LIFE_CYCLE_RULE_SCHEDULED_TASKS_INTERVAL_SEC;
                    }

                    DateTime nextExecutionDate = DateTime.UtcNow.AddSeconds(alcrScheduledTaskIntervalSec);
                    GenericCeleryQueue queue = new GenericCeleryQueue();
                    BaseCeleryData data = new BaseCeleryData(Guid.NewGuid().ToString(), ACTION_RULE_TASK, new List<object>(), nextExecutionDate);
                    bool enqueueResult = queue.Enqueue(data, ROUTING_KEY_RECORDINGS_ASSET_LIFE_CYCLE_RULE);
                }
            }

            return true;
        }

        public static bool DoActionRules(int groupId, List<long> ruleIds)
        {
            return AssetLifeCycleRuleManager.Instance.DoActionRules(groupId, ruleIds) > 0;
        }

        public static ApiObjects.AssetLifeCycleRules.FriendlyAssetLifeCycleRule GetFriendlyAssetLifeCycleRule(int groupId, long id)
        {
            FriendlyAssetLifeCycleRule result = null;
            try
            {
                Dictionary<int, List<AssetLifeCycleRule>> rules = AssetLifeCycleRuleManager.Instance.GetLifeCycleRules(groupId, new List<long>() { id });
                if (rules != null && rules.ContainsKey(groupId) && rules[groupId] != null && rules[groupId].Count == 1)
                {
                    result = new FriendlyAssetLifeCycleRule(rules[groupId].First());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetFriendlyAssetLifeCycleRule failed, groupId: {0}, id: {1}", groupId, id), ex);
            }

            return result;
        }

        public static string GetFriendlyAssetLifeCycleRuleKsqlFilter(int groupId, string tagType, List<string> tagValues, eCutType operand, string dateMeta, long dateValue)
        {
            string result = string.Empty;
            try
            {
                FriendlyAssetLifeCycleRule rule = new FriendlyAssetLifeCycleRule(groupId, tagType, tagValues, operand, dateMeta, dateValue);
                if (rule != null && !string.IsNullOrEmpty(rule.KsqlFilter))
                {
                    result = rule.KsqlFilter;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetFriendlyAssetLifeCycleRuleKsqlFilter, groupId: {0}, tagType: {1}, tagValues: {2}, operand: {3}, dateMeta: {4}, dateValue: {5}",
                    groupId, tagType, tagValues != null ? string.Join(",", tagValues) : string.Empty, operand.ToString(), dateMeta, dateValue), ex);
            }

            return result;
        }

    }
}