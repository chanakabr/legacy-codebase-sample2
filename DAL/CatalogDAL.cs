using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using ODBCWrapper;
using ApiObjects;
using ApiObjects.MediaMarks;
using CouchbaseManager;
using System.Threading;
using Newtonsoft.Json;
using DAL;
using ApiObjects.Epg;
using ApiObjects.SearchObjects;
using KLogMonitor;
using System.Reflection;
using ApiObjects.PlayCycle;

namespace Tvinci.Core.DAL
{
    public class CatalogDAL : BaseDal
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string CB_MEDIA_MARK_DESGIN = ODBCWrapper.Utils.GetTcmConfigValue("cb_media_mark_design");
        private static readonly string CB_EPG_DOCUMENT_EXPIRY_DAYS = ODBCWrapper.Utils.GetTcmConfigValue("epg_doc_expiry");
        private static readonly string CB_PLAYCYCLE_DOC_EXPIRY_MIN = ODBCWrapper.Utils.GetTcmConfigValue("playCycle_doc_expiry_min");

        /// <summary>
        /// 5
        /// </summary>
        private const int RETRY_LIMIT = 5;

        public static DataSet Get_MediaDetails(int nGroupID, int nMediaID, string sSiteGuid, bool bOnlyActiveMedia, int nLanguage, string sEndDate, bool bUseStartDate, List<int> lSubGroupTree)
        {
            ODBCWrapper.StoredProcedure spGet_MediaDetails = new ODBCWrapper.StoredProcedure("Get_MediaDetails");
            spGet_MediaDetails.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGet_MediaDetails.AddParameter("@MediaID", nMediaID);
            spGet_MediaDetails.AddParameter("@GroupID", nGroupID);
            spGet_MediaDetails.AddParameter("@SiteGuid", sSiteGuid);
            spGet_MediaDetails.AddParameter("@OnlyActiveMedia", bOnlyActiveMedia);
            spGet_MediaDetails.AddParameter("@Language", nLanguage);
            spGet_MediaDetails.AddParameter("@EndDate", sEndDate);
            spGet_MediaDetails.AddParameter("@UseStartDate", bUseStartDate);
            spGet_MediaDetails.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

            DataSet ds = spGet_MediaDetails.ExecuteDataSet();

            return ds;
        }

        /// <summary>
        /// For a given user and media, returns the last time the user watched the media
        /// </summary>
        /// <param name="p_nMedia"></param>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        public static DateTime? Get_MediaUserLastWatch(int mediaId, string siteGuid)
        {
            DateTime? result = null;

            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

            // get document of media mark
            object document = cbManager.Get<object>(UtilsDal.getUserMediaMarkDocKey(siteGuid, mediaId));

            if (document != null)
            {
                // Deserialize to known class - for comfortable access
                MediaMarkLog mediaMarkLog = JsonConvert.DeserializeObject<MediaMarkLog>(document.ToString());

                result = mediaMarkLog.LastMark.CreatedAt;
            }

            return result;
        }

        public static DataSet Build_MediaRelated(int nGroupID, int nMediaID, int nLanguage, List<int> lSubGroupTree)
        {
            ODBCWrapper.StoredProcedure spBuild_MediaRelated = new ODBCWrapper.StoredProcedure("Build_MediaRelated");
            spBuild_MediaRelated.SetConnectionKey("MAIN_CONNECTION_STRING");
            spBuild_MediaRelated.AddParameter("@MediaID", nMediaID);
            spBuild_MediaRelated.AddParameter("@GroupID", nGroupID);
            spBuild_MediaRelated.AddParameter("@Language", nLanguage);
            spBuild_MediaRelated.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

            DataSet ds = spBuild_MediaRelated.ExecuteDataSet();

            return ds;
        }

        public static DataTable Get_ChannelBaseData(int nGroupID, int nChannelID)
        {
            ODBCWrapper.StoredProcedure spChannelBaseData = new ODBCWrapper.StoredProcedure("Get_ChannelBaseData");
            spChannelBaseData.SetConnectionKey("MAIN_CONNECTION_STRING");
            spChannelBaseData.AddParameter("@ChannelID", nChannelID);
            spChannelBaseData.AddParameter("@GroupID", nGroupID);

            DataSet ds = spChannelBaseData.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataSet Get_ChannelsListByCategory(int nCategoryID, int nGroupID, int nLanguage, List<int> lSubGroupTree)
        {
            ODBCWrapper.StoredProcedure spCategoryChannels = new ODBCWrapper.StoredProcedure("Get_ChannelsListByCategory");
            spCategoryChannels.SetConnectionKey("MAIN_CONNECTION_STRING");
            spCategoryChannels.AddParameter("@CategoryID", nCategoryID);
            spCategoryChannels.AddParameter("@GroupID", nGroupID);
            spCategoryChannels.AddParameter("@LanguageID", nLanguage);
            spCategoryChannels.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

            DataSet ds = spCategoryChannels.ExecuteDataSet();

            return ds;
        }

        public static DataSet Get_CommentsList(int nMediaID, int nGroupID, int nLanguage)
        {
            ODBCWrapper.StoredProcedure spPersonalRecommended = new ODBCWrapper.StoredProcedure("Get_CommentsList");
            spPersonalRecommended.SetConnectionKey("MAIN_CONNECTION_STRING");

            spPersonalRecommended.AddParameter("@GroupID", nGroupID);
            spPersonalRecommended.AddParameter("@MediaID", nMediaID);
            spPersonalRecommended.AddParameter("@LangID", nLanguage);

            DataSet ds = spPersonalRecommended.ExecuteDataSet();

            return ds;
        }

        public static DataTable Get_PersonalLastWatched(int nGroupID, string sSiteGuid)
        {
            bool bGetDBData = TCMClient.Settings.Instance.GetValue<bool>("getDBData");
            DataTable dt = null;

            int nSiteGuid = 0;
            int.TryParse(sSiteGuid, out nSiteGuid);
            List<UserMediaMark> mediaMarksList = GetMediaMarksLastDateByUsers(new List<int> { nSiteGuid });
            List<int> nMediaIDs = mediaMarksList.Where(x => x.CreatedAt >= DateTime.UtcNow.AddDays(-8)).Select(x => x.MediaID).ToList();
            bool bContunueWithCB = (nMediaIDs != null && nMediaIDs.Count > 0) ? true : false;

            if (bContunueWithCB)
            {
                //Complete details from db
                dt = Get_MediaUpdateDate(nMediaIDs);
            }
            else if (bGetDBData)
            {
                ODBCWrapper.StoredProcedure spPersonalLastWatched = new ODBCWrapper.StoredProcedure("Get_PersonalLastWatched");
                spPersonalLastWatched.SetConnectionKey("MAIN_CONNECTION_STRING");
                spPersonalLastWatched.AddParameter("@GroupID", nGroupID);
                spPersonalLastWatched.AddParameter("@SiteGuid", sSiteGuid);
                DataSet ds = spPersonalLastWatched.ExecuteDataSet();
                if (ds != null)
                    dt = ds.Tables[0];
            }

            return dt;

        }

        public static List<UserMediaMark> Get_PersonalLastDevice(List<int> nMediaIDs, string sSiteGuid)
        {
            List<MediaMarkLog> mediaMarkLogList = new List<MediaMarkLog>();
            List<UserMediaMark> lRes = new List<UserMediaMark>();
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            List<string> docKeysList = new List<string>();

            int nUserID = 0;
            int.TryParse(sSiteGuid, out nUserID);

            foreach (int nMediaID in nMediaIDs)
            {
                docKeysList.Add(UtilsDal.getUserMediaMarkDocKey(nUserID, nMediaID));
            }

            IDictionary<string, object> res = cbManager.GetValues<object>(docKeysList, true);

            foreach (string sKey in res.Keys)
            {
                mediaMarkLogList.Add(JsonConvert.DeserializeObject<MediaMarkLog>(res[sKey].ToString()));
            }

            List<MediaMarkLog> sortedMediaMarksList = mediaMarkLogList.OrderByDescending(x => x.LastMark.CreatedAt).ToList();
            lRes = sortedMediaMarksList.Select(x => x.LastMark).ToList();
            return lRes;
        }

        public static DataTable Get_MediaUpdateDate(List<int> nMediaIDs)
        {
            ODBCWrapper.StoredProcedure spGet_MediaUpdateDate = new ODBCWrapper.StoredProcedure("Get_MediaUpdateDate");
            spGet_MediaUpdateDate.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGet_MediaUpdateDate.AddIDListParameter("@MediaID", nMediaIDs, "Id");
            DataSet ds = spGet_MediaUpdateDate.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetActiveMedia(List<int> mediaIds)
        {
            ODBCWrapper.StoredProcedure spGet_MediaUpdateDate = new ODBCWrapper.StoredProcedure("Get_ActiveMedia");
            spGet_MediaUpdateDate.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGet_MediaUpdateDate.AddIDListParameter("@MediaID", mediaIds, "Id");
            DataSet ds = spGet_MediaUpdateDate.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_UserSocialMedias(string sSiteGuid, int nSocialPlatform, int nSocialAction)
        {
            ODBCWrapper.StoredProcedure spUserSocial = new ODBCWrapper.StoredProcedure("Get_UserSocialMedias");
            spUserSocial.SetConnectionKey("MAIN_CONNECTION_STRING");

            spUserSocial.AddParameter("@SiteGuid", sSiteGuid);
            spUserSocial.AddParameter("@SocialPlatform", nSocialPlatform);
            spUserSocial.AddParameter("@SocialAction", nSocialAction);

            DataSet ds = spUserSocial.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_PersonalRecommended(int nGroupID, string sSiteGuid, int Top, List<int> lSubGroupTree)
        {
            bool bGetDBData = TCMClient.Settings.Instance.GetValue<bool>("getDBData");
            DataSet ds = null;
            int nSiteGuid = 0;
            int.TryParse(sSiteGuid, out nSiteGuid);

            List<UserMediaMark> mediaMarksList = GetMediaMarksLastDateByUsers(new List<int> { nSiteGuid });
            List<int> nMediaIDs = mediaMarksList.OrderByDescending(x => x.CreatedAt).Select(x => x.MediaID).ToList();

            if (nMediaIDs != null && nMediaIDs.Count > 0)
            {
                int nMediaID = 0;
                int.TryParse(nMediaIDs[0].ToString(), out nMediaID);

                ODBCWrapper.StoredProcedure spPersonalRecommended = new ODBCWrapper.StoredProcedure("Get_PersonalRecommended_C");
                spPersonalRecommended.SetConnectionKey("MAIN_CONNECTION_STRING");
                spPersonalRecommended.AddParameter("@GroupID", nGroupID);
                spPersonalRecommended.AddParameter("@MediaId", nMediaID);
                spPersonalRecommended.AddParameter("@Top", Top);
                spPersonalRecommended.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

                ds = spPersonalRecommended.ExecuteDataSet();
            }
            else if (bGetDBData)
            {
                ODBCWrapper.StoredProcedure spPersonalRecommended = new ODBCWrapper.StoredProcedure("Get_PersonalRecommended");
                spPersonalRecommended.SetConnectionKey("MAIN_CONNECTION_STRING");
                spPersonalRecommended.AddParameter("@GroupID", nGroupID);
                spPersonalRecommended.AddParameter("@SiteGuid", sSiteGuid);
                spPersonalRecommended.AddParameter("@Top", Top);
                ds = spPersonalRecommended.ExecuteDataSet();
            }
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_PWLALProtocol(int nGroupID, int nMediaID, string sSiteGuid, int nSocialAction, int nSocialPlatform, int nMediaFileID,
                            int nCountryID, int nLanguage, string sEndDate, int nDeviceId)
        {
            ODBCWrapper.StoredProcedure spPWLAL = new ODBCWrapper.StoredProcedure("Get_PWLALProtocol");
            spPWLAL.SetConnectionKey("MAIN_CONNECTION_STRING");

            spPWLAL.AddParameter("@GroupID", nGroupID);
            spPWLAL.AddParameter("@MediaID", nMediaID);
            spPWLAL.AddParameter("@SiteGUID", sSiteGuid);
            spPWLAL.AddParameter("@SocialAction", nSocialAction);
            spPWLAL.AddParameter("@SocialPlatform", nSocialPlatform);
            spPWLAL.AddParameter("@MediaFileID", nMediaFileID);
            spPWLAL.AddParameter("@CountryID", nCountryID);
            spPWLAL.AddParameter("@LanguageID", nLanguage);
            spPWLAL.AddParameter("@EndDateField", sEndDate);
            spPWLAL.AddParameter("@DeviceID", nDeviceId);

            DataSet ds = spPWLAL.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_PWWAWProtocol(int nGroupID, int nMediaID, string sSiteGuid, int nCountryID, int nLanguage, string sEndDate, int nDeviceId)
        {

            bool bGetDBData = TCMClient.Settings.Instance.GetValue<bool>("getDBData");
            DataSet ds = null;
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

            //int nNumOfUsers = 30;
            int nNumOfMedias = 8;
            int nSiteGuid = 0;
            int.TryParse(sSiteGuid, out nSiteGuid);

            List<UserMediaMark> mediaMarksList = CatalogDAL.GetMediaMarksLastDateByMedias(new List<int> { nMediaID });
            List<UserMediaMark> sortedMediaMarksList = mediaMarksList.OrderByDescending(x => x.CreatedAt).ToList(); //.Take(nNumOfUsers).ToList();

            bool bContunueWithCB = (sortedMediaMarksList != null && sortedMediaMarksList.Count > 0) ? true : false;
            if (bContunueWithCB)
            {
                Dictionary<int, int> dictMediaWatchersCount = sortedMediaMarksList.Where(x => x.UserID != nSiteGuid).GroupBy(g => g.MediaID).ToDictionary(k => k.Key, k => k.Count());
                List<int> otherMediasList = dictMediaWatchersCount.OrderByDescending(x => x.Value).Take(nNumOfMedias).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();

                ODBCWrapper.StoredProcedure spPWWAWProtocol = new ODBCWrapper.StoredProcedure("Get_PWWAWProtocol_C");
                spPWWAWProtocol.SetConnectionKey("MAIN_CONNECTION_STRING");

                spPWWAWProtocol.AddIDListParameter<int>("@mediasList", otherMediasList, "Id");
                spPWWAWProtocol.AddParameter("@GroupID", nGroupID);
                spPWWAWProtocol.AddParameter("@Language", nLanguage);
                spPWWAWProtocol.AddParameter("@CountryID", nCountryID);
                spPWWAWProtocol.AddParameter("@EndDateField", sEndDate);
                spPWWAWProtocol.AddParameter("@DeviceID", nDeviceId);
                ds = spPWWAWProtocol.ExecuteDataSet();
            }
            else if (bGetDBData)
            {
                ODBCWrapper.StoredProcedure spPWWAWProtocol = new ODBCWrapper.StoredProcedure("Get_PWWAWProtocol");
                spPWWAWProtocol.SetConnectionKey("MAIN_CONNECTION_STRING");
                spPWWAWProtocol.AddParameter("@MediaID", nMediaID);
                spPWWAWProtocol.AddParameter("@GroupID", nGroupID);
                spPWWAWProtocol.AddParameter("@Language", nLanguage);
                spPWWAWProtocol.AddParameter("@CountryID", nCountryID);
                spPWWAWProtocol.AddParameter("@EndDateField", sEndDate);
                spPWWAWProtocol.AddParameter("@DeviceID", nDeviceId);
                spPWWAWProtocol.AddParameter("@SiteGuid", sSiteGuid);
                ds = spPWWAWProtocol.ExecuteDataSet();
            }

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_ChannelsBySubscription(int nGroupID, int nSubscriptionID)
        {
            ODBCWrapper.StoredProcedure spCatalog = new ODBCWrapper.StoredProcedure("Get_ChannelsBySubscription");
            spCatalog.SetConnectionKey("MAIN_CONNECTION_STRING");

            spCatalog.AddParameter("@GroupID", nGroupID);
            spCatalog.AddParameter("@SubscriptionID", nSubscriptionID);

            DataSet ds = spCatalog.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_ChannelsByCollection(int nGroupID, int nCollectionID)
        {
            ODBCWrapper.StoredProcedure spCatalog = new ODBCWrapper.StoredProcedure("Get_ChannelsByCollection");
            spCatalog.SetConnectionKey("MAIN_CONNECTION_STRING");

            spCatalog.AddParameter("@GroupID", nGroupID);
            spCatalog.AddParameter("@CollectionID", nCollectionID);

            DataSet ds = spCatalog.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_PicProtocol(int nGroupID, List<int> nPicIDs, List<int> lSubGroupTree)
        {
            ODBCWrapper.StoredProcedure spPicProtocol = new ODBCWrapper.StoredProcedure("Get_PicProtocol");
            spPicProtocol.SetConnectionKey("MAIN_CONNECTION_STRING");

            spPicProtocol.AddIDListParameter("@PicIdList", nPicIDs, "Id");
            spPicProtocol.AddParameter("@GroupID", nGroupID);
            spPicProtocol.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

            DataSet ds = spPicProtocol.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_LuceneUrl(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spLuceneUrl = new ODBCWrapper.StoredProcedure("Get_LuceneUrl");
            spLuceneUrl.SetConnectionKey("MAIN_CONNECTION_STRING");
            spLuceneUrl.AddParameter("@GroupID", nGroupID);

            DataSet ds = spLuceneUrl.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_MediaPlayData(int nMediaID, int nMediaFileID)
        {
            ODBCWrapper.StoredProcedure spGetMediaPlayData = new ODBCWrapper.StoredProcedure("GetMediaPlayData");
            spGetMediaPlayData.SetConnectionKey("MAIN_CONNECTION_STRING");

            spGetMediaPlayData.AddParameter("@MediaID", nMediaID);
            spGetMediaPlayData.AddParameter("@MediaFileID", nMediaFileID);

            DataSet ds = spGetMediaPlayData.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_ActionValues(string sAction)
        {
            ODBCWrapper.StoredProcedure spGetActionValues = new ODBCWrapper.StoredProcedure("GetActionValues");
            spGetActionValues.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetActionValues.AddParameter("@ApiVal", sAction);
            DataSet ds = spGetActionValues.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static int Get_BillingTypeID(string apiVal)
        {
            ODBCWrapper.StoredProcedure spGetBillingTypeID = new ODBCWrapper.StoredProcedure("GetBillingTypeID");
            spGetBillingTypeID.SetConnectionKey("MAIN_CONNECTION_STRING");

            spGetBillingTypeID.AddParameter("@ApiVal", apiVal);
            int result = spGetBillingTypeID.ExecuteReturnValue<int>();
            return result;
        }

        public static string Get_LastPlayCycleKey(string sSiteGUID, int nMediaID, int nMediaFileID, string sUDID, int nPlatform)
        {
            string result = string.Empty;
            ODBCWrapper.StoredProcedure spGetLastPlayCycleKey = new ODBCWrapper.StoredProcedure("GetLastPlayCycleKey");
            spGetLastPlayCycleKey.SetConnectionKey("MAIN_CONNECTION_STRING");

            spGetLastPlayCycleKey.AddParameter("@SiteGuid", sSiteGUID);
            spGetLastPlayCycleKey.AddParameter("@MediaID", nMediaID);
            spGetLastPlayCycleKey.AddParameter("@MediaFileID", nMediaFileID);
            spGetLastPlayCycleKey.AddParameter("@DeviceUDID", sUDID);
            spGetLastPlayCycleKey.AddParameter("@Platform", nPlatform);

            DataSet ds = spGetLastPlayCycleKey.ExecuteDataSet();

            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                result = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "play_cycle_key");
            }
            return result;
        }

        public static void Insert_NewWatcherMediaAction(int nWatcherID, string sSessionID, int nBillingTypeID, int nOwnerGroupID, int nQualityID, int nFormatID, int nMediaID, int nMediaFileID,
                                                        int nGroupID, int nCDNID, int nActionID, int nCountryID, int nPlayerID, int nLoc, int nBrowser, int nPlatform, string sSiteGUID, string sUDID)
        {

            ODBCWrapper.StoredProcedure spNewWatcherMediaAction = new ODBCWrapper.StoredProcedure("Insert_NewWatcherMediaAction");
            spNewWatcherMediaAction.SetConnectionKey("MAIN_CONNECTION_STRING");

            spNewWatcherMediaAction.AddParameter("@WatcherID", nWatcherID);
            spNewWatcherMediaAction.AddParameter("@SessionID", sSessionID);
            spNewWatcherMediaAction.AddParameter("@BillingTypeID", nBillingTypeID);
            spNewWatcherMediaAction.AddParameter("@OwnerGroupID", nOwnerGroupID);
            spNewWatcherMediaAction.AddParameter("@FileQualityID", nQualityID);
            spNewWatcherMediaAction.AddParameter("@FileFormatID", nFormatID);
            spNewWatcherMediaAction.AddParameter("@MediaID", nMediaID);
            spNewWatcherMediaAction.AddParameter("@MediaFileID", nMediaFileID);
            spNewWatcherMediaAction.AddParameter("@GroupID", nGroupID);
            spNewWatcherMediaAction.AddParameter("@CdnID", nCDNID);
            spNewWatcherMediaAction.AddParameter("@ActionID", nActionID);
            spNewWatcherMediaAction.AddParameter("@CountryID", nCountryID);
            spNewWatcherMediaAction.AddParameter("@PlayerID", nPlayerID);
            spNewWatcherMediaAction.AddParameter("@LocationSec", nLoc);
            spNewWatcherMediaAction.AddParameter("@Browser", nBrowser);
            spNewWatcherMediaAction.AddParameter("@Platform", nPlatform);
            spNewWatcherMediaAction.AddParameter("@SiteGuid", sSiteGUID);
            spNewWatcherMediaAction.AddParameter("@DeviceUdID", sUDID);

            spNewWatcherMediaAction.ExecuteNonQuery();
        }

        public static void Insert_NewMediaEoh(int nWatcherID, string sSessionID, int nGroupID, int nOwnerGroupID, int nMediaID, int nMediaFileID, int nBillingTypeID, int nCDNID, int nDuration, int nCountryID, int nPLayerID,
                                              int nFirstPlayCounter, int nPlayCounter, int nLoadCounter, int nPauseCounter, int nStopCounter, int nFullScreenCounter, int nExitFullScreenCounter, int nSendToFriendCounter,
                                              int nPlayTimeCounter, int nFileQualityID, int nFileFormatID, DateTime dStartHourDate, int nUpdaterID, int nBrowser, int nPlatform, string sSiteGuid, string sDeviceUdID, string sPlayCycleID, int nSwooshCounter
                                             )
        {

            ODBCWrapper.StoredProcedure spNewMediaEoh = new ODBCWrapper.StoredProcedure("Insert_NewMediaEoh");
            spNewMediaEoh.SetConnectionKey("MAIN_CONNECTION_STRING");

            spNewMediaEoh.AddParameter("@WatcherID", nWatcherID);
            spNewMediaEoh.AddParameter("@SessionID", sSessionID);
            spNewMediaEoh.AddParameter("@GroupID", nGroupID);
            spNewMediaEoh.AddParameter("@OwnerGroupID", nOwnerGroupID);
            spNewMediaEoh.AddParameter("@MediaID", nMediaID);
            spNewMediaEoh.AddParameter("@MediaFileID", nMediaFileID);
            spNewMediaEoh.AddParameter("@BillingTypeID", nBillingTypeID);
            spNewMediaEoh.AddParameter("@CdnID", nCDNID);
            spNewMediaEoh.AddParameter("@Duration", nDuration);
            spNewMediaEoh.AddParameter("@CountryID", nCountryID);
            spNewMediaEoh.AddParameter("@PlayerID", nPLayerID);
            spNewMediaEoh.AddParameter("@FirstPlayCounter", nFirstPlayCounter);
            spNewMediaEoh.AddParameter("@PlayCounter", nPlayCounter);
            spNewMediaEoh.AddParameter("@LoadCounter", nLoadCounter);
            spNewMediaEoh.AddParameter("@PauseCounter", nPauseCounter);
            spNewMediaEoh.AddParameter("@StopCounter", nStopCounter);
            spNewMediaEoh.AddParameter("@FullScreenCounter", nFullScreenCounter);
            spNewMediaEoh.AddParameter("@ExitFullScreenCounter", nExitFullScreenCounter);
            spNewMediaEoh.AddParameter("@SendToFriendCounter", nSendToFriendCounter);
            spNewMediaEoh.AddParameter("@PlayTimeCounter", nPlayTimeCounter);
            spNewMediaEoh.AddParameter("@FileQualityID", nFileQualityID);
            spNewMediaEoh.AddParameter("@FileFormatID", nFileFormatID);
            spNewMediaEoh.AddParameter("@StartHourDate", dStartHourDate);
            spNewMediaEoh.AddParameter("@UpdaterID", nUpdaterID);
            spNewMediaEoh.AddParameter("@Browser", nBrowser);
            spNewMediaEoh.AddParameter("@Platform", nPlatform);
            spNewMediaEoh.AddParameter("@SiteGuid", sSiteGuid);
            spNewMediaEoh.AddParameter("@DeviceUdID", sDeviceUdID);
            spNewMediaEoh.AddParameter("@PlayCycleID", sPlayCycleID);
            spNewMediaEoh.AddParameter("@SwooshCounter", nSwooshCounter);

            spNewMediaEoh.ExecuteNonQuery();
        }

        public static void Insert_NewMediaFileVideoQuality(int nWatcherID, int nUserSiteGuid, string sSessionID, int nMediaID, int nMediaFileID, int nAvgMaxBitRate, int nBitRateIndex,
                                                           int nTotalBitRatesNum, int nLoactionSec, int nBrowser, int nPlatform, int nCountryID, int nStatus, int nGroupID)
        {
            ODBCWrapper.StoredProcedure spInsertNewMediaFileVideoQuality = new ODBCWrapper.StoredProcedure("Insert_NewMediaFileVideoQuality");
            spInsertNewMediaFileVideoQuality.SetConnectionKey("MAIN_CONNECTION_STRING");

            spInsertNewMediaFileVideoQuality.AddParameter("@WatcherID", nWatcherID);
            spInsertNewMediaFileVideoQuality.AddParameter("@UserSiteGuid", nUserSiteGuid);
            spInsertNewMediaFileVideoQuality.AddParameter("@SessionID", sSessionID);
            spInsertNewMediaFileVideoQuality.AddParameter("@MediaID", nMediaID);
            spInsertNewMediaFileVideoQuality.AddParameter("@MediaFileID", nMediaFileID);
            spInsertNewMediaFileVideoQuality.AddParameter("@AvgMaxBitRate", nAvgMaxBitRate);
            spInsertNewMediaFileVideoQuality.AddParameter("@BitRateIndex", nBitRateIndex);
            spInsertNewMediaFileVideoQuality.AddParameter("@TotalBitRatesNum", nTotalBitRatesNum);
            spInsertNewMediaFileVideoQuality.AddParameter("@LocationSec", nLoactionSec);
            spInsertNewMediaFileVideoQuality.AddParameter("@Browser", nBrowser);
            spInsertNewMediaFileVideoQuality.AddParameter("@Platform", nPlatform);
            spInsertNewMediaFileVideoQuality.AddParameter("@CountryID", nCountryID);
            spInsertNewMediaFileVideoQuality.AddParameter("@Status", nStatus);
            spInsertNewMediaFileVideoQuality.AddParameter("@GroupID", nGroupID);

            spInsertNewMediaFileVideoQuality.ExecuteNonQuery();
        }

        public static void Insert_NewPlayerErrorMessage(int nGroupID, int nMediaID, int nMediaFileID, int nLoactionSec, int nPlatform, int nSiteUserGuid, string sUDID, string sErrorCode, string sErrorMessage)
        {
            ODBCWrapper.StoredProcedure spInsertNewPlayerError = new ODBCWrapper.StoredProcedure("Insert_NewPlayerError");
            spInsertNewPlayerError.SetConnectionKey("MAIN_CONNECTION_STRING");

            spInsertNewPlayerError.AddParameter("@GroupID", nGroupID);
            spInsertNewPlayerError.AddParameter("@MediaID ", nMediaID);
            spInsertNewPlayerError.AddParameter("@MediaFileID", nMediaFileID);
            spInsertNewPlayerError.AddParameter("@PlayTimeCounter", nLoactionSec);
            spInsertNewPlayerError.AddParameter("@Platform", nPlatform);
            spInsertNewPlayerError.AddParameter("@SiteGuid", nSiteUserGuid);
            spInsertNewPlayerError.AddParameter("@DeviceUdID", sUDID);
            spInsertNewPlayerError.AddParameter("@ErrorCode", sErrorCode);
            spInsertNewPlayerError.AddParameter("@ErrorMessage", sErrorMessage);

            spInsertNewPlayerError.ExecuteNonQuery();
        }

        public static void UpdateOrInsert_UsersMediaMark(int nDomainID, int nSiteUserGuid, string sUDID, int nMediaID,
            int nGroupID, int nLoactionSec, int fileDuration, string action, int mediaTypeId, bool isFirstPlay,
            bool isLinearChannel = false, int finishedPercentThreshold = 95)
        {
            var mediaMarksManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            var mediaHitsManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIA_HITS);
            var domainMarksManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.DOMAIN_CONCURRENCY);

            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            DateTime currentDate = DateTime.UtcNow;

            string docKey = UtilsDal.getDomainMediaMarksDocKey(nDomainID);
            UserMediaMark dev = new UserMediaMark()
            {
                Location = nLoactionSec,
                UDID = sUDID,
                MediaID = nMediaID,
                UserID = nSiteUserGuid,
                CreatedAt = currentDate,
                CreatedAtEpoch = Utils.DateTimeToUnixTimestamp(currentDate),
                playType = ePlayType.MEDIA.ToString(),
                FileDuration = fileDuration,
                AssetAction = action,
                AssetTypeId = mediaTypeId
            };

            while (limitRetries >= 0)
            {
                bool res = UpdateDomainConcurrency(sUDID, domainMarksManager, docKey, dev);

                if (!res)
                {
                    Thread.Sleep(r.Next(50));
                    limitRetries--;
                }
                else
                    break;
            }

            string mmKey = UtilsDal.getUserMediaMarkDocKey(nSiteUserGuid, nMediaID);

            //Now storing this by the mediaID
            limitRetries = RETRY_LIMIT;
            bool success = false;

            bool shouldUpdateLocation = false;

            // media hits interest us only on media that are not linear channel - if it is a linear channel, we are not interested in the location
            // because it is a live, constant, "endless" stream
            if (!isLinearChannel || (isLinearChannel && isFirstPlay))
            {
                while (limitRetries >= 0 && !success)
                {
                    shouldUpdateLocation = UpdateOrInsert_UsersMediaMarkOrHit(mediaHitsManager, sUDID, ref limitRetries, r, mmKey, ref success, dev, finishedPercentThreshold);
                }
            }

            // media marks interest us only if location status is changed (in progress / done) or if it is first play
            if (isFirstPlay || shouldUpdateLocation)
            {
                //Now storing this by the mediaID
                limitRetries = RETRY_LIMIT;
                success = false;

                while (limitRetries >= 0 && !success)
                {
                    UpdateOrInsert_UsersMediaMarkOrHit(mediaMarksManager, sUDID, ref limitRetries, r, mmKey, ref success, dev, 0);
                }
            }
        }

        private static bool UpdateDomainConcurrency(string udid,
            CouchbaseManager.CouchbaseManager couchbase, string documentKey, UserMediaMark userMediaMark)
        {
            ulong version;
            var data = couchbase.GetWithVersion<string>(documentKey, out version);

            DomainMediaMark domainMediaMark = new DomainMediaMark();

            //Create new if doesn't exist
            if (data == null)
            {
                domainMediaMark.devices = new List<UserMediaMark>();
                domainMediaMark.devices.Add(userMediaMark);
            }
            else
            {
                domainMediaMark = JsonConvert.DeserializeObject<DomainMediaMark>(data);
                UserMediaMark existdev = domainMediaMark.devices.Where(x => x.UDID == udid).FirstOrDefault();

                if (existdev != null)
                    domainMediaMark.devices.Remove(existdev);

                domainMediaMark.devices.Add(userMediaMark);
            }
            bool res = couchbase.SetWithVersion(documentKey, JsonConvert.SerializeObject(domainMediaMark, Formatting.None), version);
            return res;
        }

        private static bool UpdateOrInsert_UsersMediaMarkOrHit(CouchbaseManager.CouchbaseManager couchbaseManager, string udid,
            ref int limitRetries, Random r, string mmKey, ref bool success, UserMediaMark userMediaMark, int finishedPercent = 95)
        {
            bool locationStatusChanged = false;
            int previousLocation = 0;

            ulong version;
            var mediaHitData = couchbaseManager.GetWithVersion<string>(mmKey, out version);

            MediaMarkLog umm = new MediaMarkLog();

            if (mediaHitData != null)
            {
                if (finishedPercent > 0)
                {
                    umm = JsonConvert.DeserializeObject<MediaMarkLog>(mediaHitData);

                    previousLocation = umm.LastMark.Location;
                    int duration = umm.LastMark.FileDuration;

                    bool wasFinishedBefore = false;
                    bool isFinishedNow = false;

                    if ((duration != 0) && (((float)previousLocation / (float)duration * 100) >= finishedPercent))
                    {
                        wasFinishedBefore = true;
                    }

                    if ((userMediaMark.FileDuration != 0) && (((float)userMediaMark.Location / (float)userMediaMark.FileDuration * 100) >= finishedPercent))
                    {
                        isFinishedNow = true;
                    }

                    // if there is a difference in the location statuses (wasn't finished but now it is, or vice versa) - mark it
                    if (wasFinishedBefore != isFinishedNow)
                    {
                        locationStatusChanged = true;
                    }
                }
            }

            umm.LastMark = userMediaMark;

            bool result = couchbaseManager.SetWithVersion(mmKey, JsonConvert.SerializeObject(umm, Formatting.None), version);

            if (!result)
            {
                Thread.Sleep(r.Next(50));
                limitRetries--;
            }
            else
            {
                success = true;
            }

            return locationStatusChanged;
        }

        public static DataTable Get_GroupByChannel(int channelID)
        {
            ODBCWrapper.StoredProcedure spLuceneUr = new ODBCWrapper.StoredProcedure("Get_GroupByChannel");
            spLuceneUr.SetConnectionKey("MAIN_CONNECTION_STRING");
            spLuceneUr.AddParameter("@ChannelID", channelID);

            DataSet ds = spLuceneUr.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataSet Get_MetasByGroup(int groupID, List<int> lSubGroupTree)
        {
            ODBCWrapper.StoredProcedure spMetas = new ODBCWrapper.StoredProcedure("Get_MetasByGroup");
            spMetas.SetConnectionKey("MAIN_CONNECTION_STRING");
            spMetas.AddParameter("@GroupId", groupID);
            spMetas.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

            DataSet ds = spMetas.ExecuteDataSet();

            if (ds != null)
                return ds;
            return null;
        }

        public static DataTable GetMappedMetasByGroupId(int nParentGroupId, List<int> lSubGroupTree)
        {
            DataTable returnedDataTable = null;

            try
            {
                ODBCWrapper.StoredProcedure spGroupsMappedMetaColumns = new ODBCWrapper.StoredProcedure("Get_MetasByGroup");
                spGroupsMappedMetaColumns.SetConnectionKey("MAIN_CONNECTION_STRING");
                spGroupsMappedMetaColumns.AddParameter("@GroupId", nParentGroupId);
                spGroupsMappedMetaColumns.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

                DataSet mappedMetasDataSet = spGroupsMappedMetaColumns.ExecuteDataSet();

                if (mappedMetasDataSet != null && mappedMetasDataSet.Tables.Count > 0)
                {
                    returnedDataTable = mappedMetasDataSet.Tables[0];
                }
            }
            catch
            {
                returnedDataTable = null;
            }

            return returnedDataTable;
        }


        public static DataSet GetChannelDetails(List<int> nChannelId, bool alsoInactive = false)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetChannelDetails");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@ChannelsID", nChannelId, "Id");
            sp.AddParameter("alsoInactive", Convert.ToInt32(alsoInactive));

            DataSet ds = sp.ExecuteDataSet();
            return ds;
        }

        public static DataTable GetChannelByChannelId(int nChannelId)
        {
            DataTable returnedDataTable = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            try
            {
                selectQuery += "select * from channels (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nChannelId);
                selectQuery.SetCachedSec(0);

                returnedDataTable = selectQuery.Execute("query", true);

            }
            catch
            {
                returnedDataTable = null;
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }

            return returnedDataTable;
        }

        public static DataTable GetAllGroupTree(int parentGroupId)
        {
            DataTable returnedDataTable = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            try
            {
                selectQuery += "select id from TVinci.dbo.F_Get_GroupsTree(" + parentGroupId.ToString() + ")";
                returnedDataTable = selectQuery.Execute("query", true);
            }
            catch
            {
                returnedDataTable = null;
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }

            return returnedDataTable;
        }

        public static DataTable GetMediaTagsTypesByGroupIds(string sAllGroups)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id, name from media_tags_types where status=1 and group_id in (" + sAllGroups + ") order by id";
            selectQuery.SetCachedSec(0);
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            DataTable mediaTagsTypeIds = null;

            try
            {
                mediaTagsTypeIds = selectQuery.Execute("query", true);
            }
            catch
            {
                mediaTagsTypeIds = null;
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }

            return mediaTagsTypeIds;
        }

        public static DataTable GetTagsValuesByTagTypeIds(int channelId)
        {
            DataTable returnedDataTable = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            try
            {
                selectQuery += "select t.tag_type_id, t.value from tags t, channel_tags ct where ct.status=1 and t.status=1 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ct.channel_id", "=", channelId);
                selectQuery += "and ct.tag_id=t.id";
                selectQuery.SetCachedSec(0);
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                returnedDataTable = selectQuery.Execute("query", true);
            }
            catch
            {
                returnedDataTable = null;
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }

            return returnedDataTable;
        }

        public static DataTable GetMediaIdsByChannelId(int nChannelId)
        {
            DataTable returnedDataTable = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            try
            {
                selectQuery += "select distinct media_id,order_num from channels_media (nolock) where status=1 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("channel_id", "=", nChannelId);
                selectQuery.SetCachedSec(0);
                returnedDataTable = selectQuery.Execute("query", true);
            }
            catch
            {
                returnedDataTable = null;
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }

            return returnedDataTable;
        }

        public static DataTable GetPermittedWatchRulesByGroupId(int nGroupId, List<int> lSubGroupTree)
        {
            DataTable permittedWatchRules = null;

            try
            {
                ODBCWrapper.StoredProcedure spPermittedWatchRulesID = new ODBCWrapper.StoredProcedure("Get_PermittedWatchRulesID");
                spPermittedWatchRulesID.SetConnectionKey("MAIN_CONNECTION_STRING");
                spPermittedWatchRulesID.AddParameter("@GroupID", nGroupId);
                spPermittedWatchRulesID.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

                DataSet ds = spPermittedWatchRulesID.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    permittedWatchRules = ds.Tables[0];
                }
            }
            catch
            {
                permittedWatchRules = null;
            }

            return permittedWatchRules;
        }

        public static DataTable GetMediaParentGroupId(int nMediaId)
        {
            DataTable mediaParentGroup = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");

            try
            {
                selectQuery += "select g.id, g.parent_group_id from media m, groups g (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", nMediaId);
                selectQuery += "and g.id=m.group_id";
                mediaParentGroup = selectQuery.Execute("query", true);
            }
            catch
            {
                mediaParentGroup = null;
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }

            return mediaParentGroup;
        }

        public static DataTable GetChanneslByChannelIds(List<int> lChannelIds)
        {
            ODBCWrapper.StoredProcedure spGet_ChannelsFullData = new ODBCWrapper.StoredProcedure("Get_ChannelsFullData");
            spGet_ChannelsFullData.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGet_ChannelsFullData.AddIDListParameter("@ChannelsID", lChannelIds, "Id");
            DataSet ds = spGet_ChannelsFullData.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetOrderedMediasByStats(List<int> ids, int nOrderType, int nOrderDirection)
        {
            DataTable dt = null;

            ODBCWrapper.StoredProcedure spGet_OrderedMediaIdList = new ODBCWrapper.StoredProcedure("Get_OrderedMediaIdList");
            spGet_OrderedMediaIdList.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGet_OrderedMediaIdList.AddIDListParameter("@MediaIds", ids, "Id");
            spGet_OrderedMediaIdList.AddParameter("@OrderingType", nOrderType);
            spGet_OrderedMediaIdList.AddParameter("@OrderingDirection", nOrderDirection);

            DataSet ds = spGet_OrderedMediaIdList.ExecuteDataSet();

            if (ds != null)
                dt = ds.Tables[0];

            return dt;
        }

        public static int GetCommentType(int nGroupID, string sCommentType)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CommentType");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            sp.AddParameter("@CommentName", sCommentType);
            sp.AddParameter("@GroupID", nGroupID);

            int result = sp.ExecuteReturnValue<int>();
            return result;
        }

        public static bool InsertEpgComment(int nEpgProgramID, int nLanguage, string sWriter, int nGroupID, string sCommentIp,
            string sHeader, string sSubHeader, string sContentText, string sSiteGuid, string sUDID, string sCountry, int nIsActive)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_EpgComment");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            sp.AddParameter("@EpgProgramId", nEpgProgramID);
            sp.AddParameter("@LanguageID", nLanguage);
            sp.AddParameter("@Writer", sWriter);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@CommentIP", sCommentIp);
            sp.AddParameter("@Header", sHeader);
            sp.AddParameter("@SubHeader", sSubHeader);
            sp.AddParameter("@ContentText", sContentText);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@DeviceUdid", sUDID);
            sp.AddParameter("@Country", sCountry);
            sp.AddParameter("@IsActive", nIsActive);
            bool result = sp.ExecuteReturnValue<bool>();
            return result;
        }

        public static DataSet Get_EPGCommentsList(int nEpgProgramID, int nGroupID, int nLanguage, List<int> lSubGroupTree)
        {
            ODBCWrapper.StoredProcedure spPersonalRecommended = new ODBCWrapper.StoredProcedure("Get_EpgCommentsList");
            spPersonalRecommended.SetConnectionKey("MAIN_CONNECTION_STRING");

            spPersonalRecommended.AddParameter("@GroupID", nGroupID);
            spPersonalRecommended.AddParameter("@EpgProgramID", nEpgProgramID);
            spPersonalRecommended.AddParameter("@LangID", nLanguage);
            spPersonalRecommended.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

            DataSet ds = spPersonalRecommended.ExecuteDataSet();

            return ds;
        }

        public static DataTable Get_EpgProgramUpdateDate(List<int> lPrograms)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EpgProgramUpdateDate");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter("@ProgramID", lPrograms, "Id");
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static bool InsertMediaComment(string sSiteGUID, int nMediaID, string sWriter, int nGroupID, int nAutoActive, int nWatcherID,
                                              string sCommentIP, string sHeader, string sSubHeader, string sContent, int nLangID, string sUDID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_MediaComment");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            sp.AddParameter("@SiteGUID", sSiteGUID);
            sp.AddParameter("@MediaID", nMediaID);
            sp.AddParameter("@Writer", sWriter);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@AutoActive", nAutoActive);
            sp.AddParameter("@WatcherID", nWatcherID);
            sp.AddParameter("@GroupGUID", sSiteGUID);
            sp.AddParameter("@CommentIP", sCommentIP);
            sp.AddParameter("@Header", sHeader);
            sp.AddParameter("@SubHeader", sSubHeader);
            sp.AddParameter("@Content", sContent);
            sp.AddParameter("@LangID", nLangID);
            sp.AddParameter("@UDID", sUDID);
            bool result = sp.ExecuteReturnValue<bool>();
            return result;
        }

        public static DataTable Get_UserMediaMark(int groupID, int mediaID, string siteGUID, bool PCFlag, string sUDID)
        {
            DataTable dt = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UserMediaMark");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@mediaID", mediaID);
            sp.AddParameter("@SiteGUID", siteGUID);
            sp.AddParameter("@PCFlag", PCFlag);
            sp.AddParameter("@sUDID", sUDID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                dt = ds.Tables[0];

            return dt;
        }

        public static DataTable Get_GroupChannels(int nGroupID, List<int> lSubGroupTree)
        {
            ODBCWrapper.StoredProcedure spPersonalRecommended = new ODBCWrapper.StoredProcedure("Get_GroupChannels");
            spPersonalRecommended.SetConnectionKey("MAIN_CONNECTION_STRING");

            spPersonalRecommended.AddParameter("@GroupID", nGroupID);
            spPersonalRecommended.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

            DataSet ds = spPersonalRecommended.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public static DataTable Get_Media_By_SlidingWindow(string spName, List<int> mediaIds, bool isDesc, int pageSize, int pageIndex, DateTime windowTime)
        {
            ODBCWrapper.StoredProcedure sp = new StoredProcedure(spName);

            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter("@MediaIds", mediaIds, "Id");
            sp.AddParameter("@WindowFrame", windowTime);
            sp.AddParameter("@OrderDir", isDesc ? 1 : 0);
            sp.AddParameter("@PageSize", pageSize);
            sp.AddParameter("@PageIndex", pageIndex);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public static DataTable Get_OrderedMediaIdList(List<int> medias, int nOrderType, int nOrderDirection)
        {
            DataTable dt = null;

            ODBCWrapper.StoredProcedure spGet_OrderedMediaIdList = new ODBCWrapper.StoredProcedure("Get_OrderedMediaIdList");
            spGet_OrderedMediaIdList.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGet_OrderedMediaIdList.AddIDListParameter("@MediaIds", medias, "Id");
            spGet_OrderedMediaIdList.AddParameter("@OrderingType", nOrderType);
            spGet_OrderedMediaIdList.AddParameter("@OrderingDirection", nOrderDirection);

            DataSet ds = spGet_OrderedMediaIdList.ExecuteDataSet();

            if (ds != null)
                dt = ds.Tables[0];
            return dt;
        }

        public static DataSet Get_GroupMedias(int m_nGroupID, int nMediaID, List<int> lSubGroupTree)
        {
            ODBCWrapper.StoredProcedure GroupMedias = new ODBCWrapper.StoredProcedure("Get_GroupMedias");
            GroupMedias.SetConnectionKey("MAIN_CONNECTION_STRING");

            GroupMedias.AddParameter("@GroupID", m_nGroupID);
            GroupMedias.AddParameter("@MediaID", nMediaID);
            GroupMedias.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

            DataSet ds = GroupMedias.ExecuteDataSet();

            return ds;
        }

        public static DataSet GetMediasStats(int nGroupID, List<int> mediaIDs, DateTime? dStartDate, DateTime? dEndDate, List<int> lSubGroupTree)
        {
            ODBCWrapper.StoredProcedure MediaStats = new ODBCWrapper.StoredProcedure("Get_MediasStats");
            MediaStats.SetConnectionKey("MAIN_CONNECTION_STRING");

            MediaStats.AddParameter("@groupID", nGroupID);
            MediaStats.AddParameter("@startDate", dStartDate);
            MediaStats.AddParameter("@endDate", dEndDate);
            MediaStats.AddIDListParameter("@MediasIDs", mediaIDs, "Id");
            MediaStats.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

            DataSet ds = MediaStats.ExecuteDataSet();
            return ds;
        }

        public static DataSet GetEpgStats(int nGroupID, List<int> epgIDs, DateTime? dStartDate, DateTime? dEndDate, List<int> lSubGroupTree)
        {
            ODBCWrapper.StoredProcedure MediaStats = new ODBCWrapper.StoredProcedure("Get_EpgStats");
            MediaStats.SetConnectionKey("MAIN_CONNECTION_STRING");

            MediaStats.AddParameter("@groupID", nGroupID);
            MediaStats.AddParameter("@startDate", dStartDate);
            MediaStats.AddParameter("@endDate", dEndDate);
            MediaStats.AddIDListParameter("@EpgIDs", epgIDs, "Id");
            MediaStats.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");


            DataSet ds = MediaStats.ExecuteDataSet();
            return ds;
        }

        public static DataSet Get_DataForEPGIPNOSearcherRequest(int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DataForEPGIPNOSearcherRequest");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);

            return sp.ExecuteDataSet();
        }

        public static DataTable Get_EPGChannelsIDsByMediaIDs(List<long> epgChannelsMediaIDs)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EPGChannelIDsByMediaIDs");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<long>("@EPGChannelsMediaIDs", epgChannelsMediaIDs, "id");

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_IPWWAWProtocol(int nGroupID, int nMediaID, string sSiteGuid, int nCountryID, int nLanguage, string sEndDate,
                                                      int nDeviceId, int nOperatorID)
        {
            bool bGetDBData = TCMClient.Settings.Instance.GetValue<bool>("getDBData");
            DataSet ds = null;
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            int nNumOfUsers = 30;
            int nNumOfMedias = 8;
            int nSiteGuid = 0;
            int.TryParse(sSiteGuid, out nSiteGuid);

            List<UserMediaMark> mediaMarksList = CatalogDAL.GetMediaMarksLastDateByMedias(new List<int> { nMediaID });
            List<UserMediaMark> sortedMediaMarksList = mediaMarksList.OrderByDescending(x => x.CreatedAt).Take(nNumOfUsers).ToList();

            bool bContunueWithCB = (sortedMediaMarksList != null && sortedMediaMarksList.Count > 0) ? true : false;
            if (bContunueWithCB)
            {
                List<int> mediaUsersList = sortedMediaMarksList.Select(x => x.UserID).ToList();
                List<int> operatorUsersList = DomainDal.GetOperatorUsers(nOperatorID, mediaUsersList);
                Dictionary<int, int> dictMediaWatchersCount = sortedMediaMarksList.Where(x => x.UserID != nSiteGuid && operatorUsersList.Contains(x.UserID)).GroupBy(g => g.MediaID).ToDictionary(k => k.Key, k => k.Count());
                List<int> otherMediasList = dictMediaWatchersCount.OrderByDescending(x => x.Value).Take(nNumOfMedias).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();

                if (otherMediasList != null && otherMediasList.Count > 0)
                {

                    ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_IPWWAWProtocol_C");
                    sp.SetConnectionKey("MAIN_CONNECTION_STRING");

                    sp.AddIDListParameter<int>("@mediasList", otherMediasList, "Id");
                    sp.AddParameter("@GroupID", nGroupID);
                    sp.AddParameter("@CountryID", nCountryID);
                    sp.AddParameter("@Language", nLanguage);
                    sp.AddParameter("@EndDateField", sEndDate);
                    sp.AddParameter("@DeviceID", nDeviceId);

                    ds = sp.ExecuteDataSet();
                }
            }
            else if (bGetDBData)
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_IPWWAWProtocol");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@MediaID", nMediaID);
                sp.AddParameter("@GroupID", nGroupID);
                sp.AddParameter("@Language", nLanguage);
                sp.AddParameter("@CountryID", nCountryID);
                sp.AddParameter("@EndDateField", sEndDate);
                sp.AddParameter("@DeviceID", nDeviceId);
                sp.AddParameter("@SiteGuid", sSiteGuid);
                sp.AddParameter("@OperatorID", nOperatorID);

                ds = sp.ExecuteDataSet();

            }
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }


        public static DataTable Get_IPWLALProtocol(int nGroupID, int nMediaID, string sSiteGuid, int nSocialAction, int nSocialPlatform, int nMediaFileID,
                    int nCountryID, int nLanguage, string sEndDate, int nDeviceId, int nOperatorID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_IPWLALProtocol");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@MediaID", nMediaID);
            sp.AddParameter("@SiteGUID", sSiteGuid);
            sp.AddParameter("@SocialAction", nSocialAction);
            sp.AddParameter("@SocialPlatform", nSocialPlatform);
            sp.AddParameter("@MediaFileID", nMediaFileID);
            sp.AddParameter("@CountryID", nCountryID);
            sp.AddParameter("@LanguageID", nLanguage);
            sp.AddParameter("@EndDateField", sEndDate);
            sp.AddParameter("@DeviceID", nDeviceId);
            sp.AddParameter("@OperatorID", nOperatorID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static List<int> Get_GroupOperatorIDs(int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_GroupOperatorIDs");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    int length = dt.Rows.Count;
                    List<int> res = new List<int>(length);

                    for (int i = 0; i < length; i++)
                    {
                        int nOperatorID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["id"]);
                        if (nOperatorID > 0)
                            res.Add(nOperatorID);
                    }

                    return res;
                }
            }

            return new List<int>(0);
        }

        public static List<int> Get_OperatorsOwningSubscription(int nGroupID, int nSubscriptionID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_OperatorsOwningSubscription");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SubscriptionID", nSubscriptionID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    int length = dt.Rows.Count;
                    List<int> res = new List<int>(length);

                    for (int i = 0; i < length; i++)
                    {
                        int operatorID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["id"]);
                        if (operatorID > 0)
                            res.Add(operatorID);

                    }

                    return res;
                }

            }

            return new List<int>(0);
        }

        public static List<LanguageObj> GetGroupLanguages(int nGroupID)
        {
            List<LanguageObj> lLanguages = null;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_GroupLanguages");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupID", nGroupID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                lLanguages = new List<LanguageObj>();

                DataTable dt = ds.Tables[0];
                LanguageObj tempLang;

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    tempLang = getLanguageFromRow(dt.Rows[0]);

                    if (tempLang != null)
                    {
                        //language from groups table is counted as default
                        tempLang.IsDefault = true;
                        lLanguages.Add(tempLang);
                    }
                }

                if (ds.Tables.Count > 1)
                {
                    dt = ds.Tables[1];

                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            tempLang = getLanguageFromRow(row);

                            if (tempLang != null)
                            {
                                //languages from group_extra_languages are set as non-default
                                tempLang.IsDefault = false;
                                lLanguages.Add(tempLang);
                            }
                        }
                    }
                }
            }

            return lLanguages;
        }

        public static DataTable Get_IPersonalRecommended(int nGroupID, string sSiteGuid, int nTop, int nOperatorID, List<int> lSubGroupTree)
        {
            StoredProcedure sp = new StoredProcedure("Get_IPersonalRecommended");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@Top", nTop);
            sp.AddParameter("@OperatorID", nOperatorID);
            sp.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");

            sp.AddParameter("@groupID", nGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null)
                return ds.Tables[0];
            return null;


        }

        private static LanguageObj getLanguageFromRow(DataRow row)
        {
            LanguageObj language = null;

            try
            {
                if (row != null)
                {
                    int id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                    string name = ODBCWrapper.Utils.GetSafeStr(row, "NAME");
                    string code = ODBCWrapper.Utils.GetSafeStr(row, "CODE3");
                    string direction = ODBCWrapper.Utils.GetSafeStr(row, "DIRECTION");

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code))
                    {
                        language = new LanguageObj()
                        {
                            ID = id,
                            Name = name,
                            Code = code,
                            Direction = direction
                        };
                    }
                }
            }
            catch
            {
                language = null;
            }

            return language;
        }

        public static DataTable Get_IPersonalRecommended(string sSiteGuid, int nTop, int nOperatorID)
        {
            int nSiteGuid = 0;
            int.TryParse(sSiteGuid, out nSiteGuid);

            List<UserMediaMark> mediaMarksList = GetMediaMarksLastDateByUsers(new List<int> { nSiteGuid });
            List<UserMediaMark> sortedMediaMarksList = mediaMarksList.ToList().OrderByDescending(x => x.CreatedAt).Take(nTop).ToList();


            if (sortedMediaMarksList != null && sortedMediaMarksList.Count > 0)
            {
                List<int> mediasList = sortedMediaMarksList.Select(x => x.MediaID).ToList();
                List<int> mediaIds = null;

                if (mediasList.Count > 1)
                {
                    List<int> operatorUsersList = DomainDal.GetOperatorUsers(nOperatorID, sortedMediaMarksList.Select(x => x.UserID).ToList());
                    mediaIds = sortedMediaMarksList.Where(x => operatorUsersList.Contains(x.UserID)).Select(x => x.MediaID).ToList();
                }
                else
                {
                    mediaIds = mediasList.ToList();
                }

                StoredProcedure sp = new StoredProcedure("Get_IPersonalRecommended_C");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddIDListParameter("@mediaIds", mediaIds, "Id");
                sp.AddParameter("@Top", nTop);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                    return ds.Tables[0];
            }
            return null;
        }

        public static int Get_MediaTypeIdByMediaId(int nMediaID)
        {
            int result = 0;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_MediaTypeIdByMediaId");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@MediaId", nMediaID);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                result = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_TYPE_ID");
            }

            return result;
        }

        public static int GetLastPosition(int mediaID, int userID)
        {
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIA_HITS);
            string key = UtilsDal.getUserMediaMarkDocKey(userID, mediaID);
            var data = cbManager.Get<string>(key);
            if (data == null)
                return 0;
            var umm = JsonConvert.DeserializeObject<MediaMarkLog>(data);
            return umm.LastMark.Location;
        }

        public static List<UserMediaMark> GetDomainLastPositions(int nDomainID, int ttl, ePlayType ePlay = ePlayType.MEDIA)
        {
            var domainMarksManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.DOMAIN_CONCURRENCY);

            string docKey = UtilsDal.getDomainMediaMarksDocKey(nDomainID);
            var data = domainMarksManager.Get<string>(docKey);

            if (data == null)
                return null;

            Random r = new Random();
            List<string> playActions = new List<string>() { MediaPlayActions.FINISH.ToString().ToLower(), MediaPlayActions.STOP.ToString().ToLower() };

            DomainMediaMark domainMarks = JsonConvert.DeserializeObject<DomainMediaMark>(data);
            domainMarks.devices = domainMarks.devices.Where(x => x.CreatedAt.AddMilliseconds(ttl) > DateTime.UtcNow && x.playType == ePlay.ToString() &&
                !playActions.Contains(x.AssetAction.ToLower())).ToList();

            //Cleaning old ones...
            int limitRetries = RETRY_LIMIT;
            while (limitRetries >= 0)
            {
                ulong version;
                var marks = domainMarksManager.GetWithVersion<string>(docKey, out version);

                DomainMediaMark dm = JsonConvert.DeserializeObject<DomainMediaMark>(marks);
                switch (ePlay)
                {
                    case ePlayType.MEDIA:
                    case ePlayType.NPVR:
                        dm.devices = dm.devices.Where(x => x.CreatedAt.AddMilliseconds(ttl) > DateTime.UtcNow && x.playType == ePlay.ToString()).ToList();
                        break;
                    case ePlayType.ALL:
                        dm.devices = dm.devices.Where(x => x.CreatedAt.AddMilliseconds(ttl) > DateTime.UtcNow).ToList();
                        break;
                    default:
                        break;
                }

                bool res = domainMarksManager.SetWithVersion(docKey, JsonConvert.SerializeObject(dm, Formatting.None), version);

                if (!res)
                {
                    Thread.Sleep(r.Next(50));
                    limitRetries--;
                }
                else
                    break;
            }

            return domainMarks.devices;
        }

        public static Dictionary<string, int> GetMediaMarkUserCount(List<int> usersList)
        {
            Dictionary<string, int> dictMediaUsersCount = new Dictionary<string, int>(); // key: media id , value: users count

            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            ViewManager viewManager = new ViewManager(CB_MEDIA_MARK_DESGIN, "users_watch_history")
            {
                startKey = new object[] { usersList, 0 },
                endKey = new object[] { usersList, string.Empty },
                staleState = CouchbaseManager.ViewStaleState.False,
                asJson = true
            };

            List<WatchHistory> lastWatchViews = cbManager.View<WatchHistory>(viewManager);

            foreach (var view in lastWatchViews)
            {
                if (!dictMediaUsersCount.ContainsKey(view.AssetId))
                    dictMediaUsersCount.Add(view.AssetId, 1);
                else
                    dictMediaUsersCount[view.AssetId]++;
            }

            return dictMediaUsersCount;
        }

        public static List<UserMediaMark> GetMediaMarksLastDateByUsers(List<int> usersList)
        {
            List<UserMediaMark> mediasMarksList = new List<UserMediaMark>();

            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

            // get views
            ViewManager viewManager = new ViewManager(CB_MEDIA_MARK_DESGIN, "users_medias_lastdate")
            {
                keys = usersList,
                asJson = true
            };

            var res = cbManager.ViewKeyValuePairs<object[]>(viewManager);

            foreach (var row in res)
            {
                int nUserID = 0;
                int nMediaID = 0;
                DateTime lastDate;

                if (row.Key != null && row.Value != null)
                {
                    object objUserID = row.Key;
                    int.TryParse(objUserID.ToString(), out nUserID);

                    object[] arrMediasDates = row.Value;
                    int.TryParse(arrMediasDates[0].ToString(), out nMediaID);
                    DateTime.TryParse(arrMediasDates[1].ToString(), out lastDate);

                    UserMediaMark objUserMediaMark = new UserMediaMark
                    {
                        MediaID = nMediaID,
                        UserID = nUserID,
                        CreatedAt = lastDate
                    };
                    mediasMarksList.Add(objUserMediaMark);
                }
            }

            return mediasMarksList;
        }

        public static List<UserMediaMark> GetMediaMarksLastDateByMedias(List<int> mediasList)
        {
            List<UserMediaMark> mediasMarksList = new List<UserMediaMark>();
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

            // get views
            ViewManager viewManager = new ViewManager(CB_MEDIA_MARK_DESGIN, "media_users_lastdate")
            {
                keys = mediasList,
                limit = 30,
                asJson = true
            };

            var res = cbManager.ViewKeyValuePairs<object[]>(viewManager);
            int nUserID = 0;

            if (res != null)
            {
                List<int> userList = new List<int>();

                foreach (var row in res)
                {
                    if (row.Key != null && row.Value != null)
                    {
                        object[] arUserDates = row.Value;
                        int.TryParse(arUserDates[0].ToString(), out nUserID);
                        userList.Add(nUserID);
                    }
                }

                if (userList != null && userList.Count > 0)
                {
                    // get views
                    ViewManager mediaViewManager = new ViewManager(CB_MEDIA_MARK_DESGIN, "users_medias_lastdate")
                    {
                        keys = userList
                    };

                    var resMedia = cbManager.ViewKeyValuePairs<object[]>(mediaViewManager);

                    foreach (var row in resMedia)
                    {
                        int nMediaID = 0;
                        nUserID = 0;

                        DateTime lastDate;
                        if (row.Key != null && row.Value != null)
                        {
                            // key = user
                            object objUserID = row.Key;
                            object[] arUserDates = row.Value;
                            int.TryParse(arUserDates[0].ToString(), out nMediaID);

                            if (!mediasList.Contains(nMediaID))
                            {
                                int.TryParse(objUserID.ToString(), out nUserID);
                                DateTime.TryParse(arUserDates[1].ToString(), out lastDate);
                                UserMediaMark objUserMediaMark = new UserMediaMark
                                {
                                    MediaID = nMediaID,
                                    UserID = nUserID,
                                    CreatedAt = lastDate
                                };
                                mediasMarksList.Add(objUserMediaMark);
                            }
                        }
                    }
                }
            }
            return mediasMarksList;

        }

        public static bool GetPicEpgURL(int groupID, ref string baseUrl, ref string width, ref string height)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("GetPicEpgURL");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = true;
                    baseUrl = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "baseURL");
                    if (baseUrl.Length > 0 && baseUrl[baseUrl.Length - 1] != '/')
                        baseUrl = String.Concat(baseUrl, '/');
                    width = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "WIDTH");
                    height = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "HEIGHT");
                }
            }

            return res;
        }

        public static Dictionary<int, List<string>> Get_GroupTreePicEpgUrl(int parentGroupID)
        {
            Dictionary<int, List<string>> res = null;
            StoredProcedure sp = new StoredProcedure("Get_GroupTreePicEpgUrl");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@ParentGroupID", parentGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = new Dictionary<int, List<string>>(dt.Rows.Count);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int groupID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["GROUP_ID"]);
                        if (groupID > 0 && !res.ContainsKey(groupID))
                        {
                            string baseUrl = string.Empty;
                            string width = string.Empty;
                            string height = string.Empty;
                            baseUrl = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["baseURL"]);
                            if (baseUrl.Length > 0 && baseUrl[baseUrl.Length - 1] != '/')
                            {
                                baseUrl = String.Concat(baseUrl, '/');
                            }
                            width = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["WIDTH"]);
                            height = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["HEIGHT"]);
                            res.Add(groupID, new List<string>(3) { baseUrl, width, height });
                        }
                    }
                }
                else
                {
                    res = new Dictionary<int, List<string>>(0);
                }
            }
            else
            {
                res = new Dictionary<int, List<string>>(0);
            }

            return res;
        }

        public static Dictionary<int, List<EpgPicture>> GetGroupTreePicEpgUrl(int parentGroupID)
        {
            Dictionary<int, List<EpgPicture>> res = null;
            StoredProcedure sp = new StoredProcedure("Get_GroupTreePicEpgUrl");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@ParentGroupID", parentGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = new Dictionary<int, List<EpgPicture>>();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int groupID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["GROUP_ID"]);
                        if (groupID > 0)
                        {
                            string baseUrl = string.Empty;
                            int width = 0;
                            int height = 0;
                            string ration = string.Empty;
                            baseUrl = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["baseURL"]);
                            if (baseUrl.Length > 0 && baseUrl[baseUrl.Length - 1] != '/')
                            {
                                baseUrl = String.Concat(baseUrl, '/');
                            }
                            width = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i], "WIDTH");
                            height = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i], "HEIGHT");
                            ration = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i], "ratio");

                            EpgPicture picture = new EpgPicture();
                            picture.Initialize(width, height, ration, baseUrl);
                            if (!res.ContainsKey(groupID))
                            {
                                res.Add(groupID, new List<EpgPicture>() { picture });
                            }
                            else
                            {
                                res[groupID].Add(picture);
                            }
                        }
                    }
                }
                else
                {
                    res = new Dictionary<int, List<EpgPicture>>(0);
                }
            }
            else
            {
                res = new Dictionary<int, List<EpgPicture>>(0);
            }

            return res;
        }

        public static DataTable GetPicEpgURL(int groupID)
        {
            StoredProcedure sp = new StoredProcedure("GetPicEpgURL");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static bool Get_ChannelsByBundles(int nGroupID, List<int> lstSubIDs, List<int> lstColIDs,
            ref Dictionary<int, List<int>> channelsToSubsMapping, ref Dictionary<int, List<int>> channelsToColsMapping)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_ChannelsByBundles");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            if (lstSubIDs != null)
                sp.AddIDListParameter("@Subscriptions", lstSubIDs, "ID");
            else
                sp.AddIDListParameter("@Subscriptions", new List<int>(0), "ID");
            if (lstColIDs != null)
                sp.AddIDListParameter("@Collections", lstColIDs, "ID");
            else
                sp.AddIDListParameter("@Collections", new List<int>(0), "ID");

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                res = true;
                DataTable dtSubChannels = ds.Tables[0];
                channelsToSubsMapping = new Dictionary<int, List<int>>();
                if (dtSubChannels != null && dtSubChannels.Rows != null && dtSubChannels.Rows.Count > 0)
                {
                    for (int i = 0; i < dtSubChannels.Rows.Count; i++)
                    {
                        int channelID = ODBCWrapper.Utils.GetIntSafeVal(dtSubChannels.Rows[i]["CHANNEL_ID"]);
                        int subID = ODBCWrapper.Utils.GetIntSafeVal(dtSubChannels.Rows[i]["SUBSCRIPTION_ID"]);
                        if (channelsToSubsMapping.ContainsKey(channelID))
                        {
                            channelsToSubsMapping[channelID].Add(subID);
                        }
                        else
                        {
                            channelsToSubsMapping.Add(channelID, new List<int>() { subID });
                        }
                    }
                }

                DataTable dtColChannels = ds.Tables[1];
                channelsToColsMapping = new Dictionary<int, List<int>>();
                if (dtColChannels != null && dtColChannels.Rows != null && dtColChannels.Rows.Count > 0)
                {
                    for (int i = 0; i < dtColChannels.Rows.Count; i++)
                    {
                        int channelID = ODBCWrapper.Utils.GetIntSafeVal(dtColChannels.Rows[i]["CHANNEL_ID"]);
                        int colID = ODBCWrapper.Utils.GetIntSafeVal(dtColChannels.Rows[i]["COLLECTION_ID"]);
                        if (channelsToColsMapping.ContainsKey(channelID))
                        {
                            channelsToColsMapping[channelID].Add(colID);
                        }
                        else
                        {
                            channelsToColsMapping.Add(channelID, new List<int>() { colID });
                        }
                    }
                }
            }
            else
            {
                channelsToSubsMapping = new Dictionary<int, List<int>>();
                channelsToColsMapping = new Dictionary<int, List<int>>();
            }

            return res;
        }

        public static DataTable Get_MediaFilesDetails(List<int> groupTree, List<int> mediaFileIDs, string mediaFileCoGuid)
        {
            StoredProcedure sp = new StoredProcedure("Get_MediaFilesDetails");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter("@GroupTree", groupTree, "ID");
            sp.AddIDListParameter("@MediaFileIDs", mediaFileIDs, "ID");
            sp.AddParameter("@CoGuid", mediaFileCoGuid);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static int Get_MediaIDByMediaFileID(int nMediaFileID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_MediaIDByMediaFileID");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@MediaFileID", nMediaFileID);
            int mediaID = sp.ExecuteReturnValue<int>();
            return mediaID;

        }

        public static int GetRuleIDPlayCycleKey(string sSiteGuid, int nMediaID, int nMediaFileID, string sUDID, int nPlatform)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetRuleIDPlayCycleKey");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@MediaID", nMediaID);
            sp.AddParameter("@MediaFileID", nMediaFileID);
            sp.AddParameter("@DeviceUDID", sUDID);
            sp.AddParameter("@Platform", nPlatform);

            return sp.ExecuteReturnValue<int>();
        }

        public static DataSet GetGroupCategoriesAndChannels(int nGroupID, int nLangID = 0)
        {
            // SELECT ID, CATEGORY_NAME, PARENT_CATEGORY_ID, PIC_ID, ORDER_NUM, CO_GUID FROM CATEGORIES 
            // SELECT CH.ID,CH.PIC_ID,CH.NAME,CH.DESCRIPTION,CH.EDITOR_REMARKS  

            StoredProcedure sp = new StoredProcedure("Get_GroupCategoriesAndChannels");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupID", nGroupID);

            if (nLangID > 0)
            {
                sp.AddParameter("@LanguageID", nLangID);
            }

            DataSet ds = sp.ExecuteDataSet();

            return ds;
        }

        public static Dictionary<int, int[]> Get_MediaStatistics(DateTime? startDate, DateTime? endDate, int parentGroupID, List<int> mediaIDs)
        {
            Dictionary<int, int[]> mediaToViewsCountMapping = null;
            StoredProcedure sp = new StoredProcedure("Get_MediaStatistics");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@StartDate", startDate);
            sp.AddParameter("@EndDate", endDate);
            sp.AddParameter("@GroupID", parentGroupID);
            sp.AddIDListParameter("@MediaIDs", mediaIDs, "ID");

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    bool isWithSocialActions = !startDate.HasValue || !endDate.HasValue;
                    mediaToViewsCountMapping = new Dictionary<int, int[]>(dt.Rows.Count);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int mediaId = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["media_id"]);
                        int viewsCount = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["views"]);
                        int votes = 0;
                        int likes = 0;
                        int votesSum = 0;
                        if (isWithSocialActions)
                        {
                            votes = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["votes_count"]);
                            votesSum = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["votes_sum"]);
                            likes = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["like_counter"]);
                        }
                        if (mediaId > 0 && !mediaToViewsCountMapping.ContainsKey(mediaId))
                        {
                            mediaToViewsCountMapping.Add(mediaId, new int[4] { viewsCount, votes, votesSum, likes });
                        }
                    }
                }
                else
                {
                    mediaToViewsCountMapping = new Dictionary<int, int[]>(0);
                }
            }
            else
            {
                mediaToViewsCountMapping = new Dictionary<int, int[]>(0);
            }

            return mediaToViewsCountMapping;
        }

        public static bool Get_MediaMarkHitInitialData(int mediaID, int mediaFileID, long ipVal, ref int countryID,
            ref int ownerGroupID, ref int cdnID, ref int qualityID, ref int formatID, ref int mediaTypeID,
            ref int billingTypeID, ref int fileDuration)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_MediaMarkHitInitialData");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@MediaID", mediaID);
            sp.AddParameter("@MediaFileID", mediaFileID);
            sp.AddParameter("@IPVal", ipVal);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                res = true;
                DataTable ipTable = ds.Tables[0];
                if (ipTable != null && ipTable.Rows != null && ipTable.Rows.Count > 0)
                {
                    countryID = ODBCWrapper.Utils.GetIntSafeVal(ipTable.Rows[0]["COUNTRY_ID"]);
                }

                DataTable mpData = ds.Tables[1];
                if (mpData != null && mpData.Rows != null && mpData.Rows.Count > 0)
                {
                    ownerGroupID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["group_id"]);
                    cdnID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["streaming_suplier_id"]);
                    qualityID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["media_quality_id"]);
                    formatID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["media_file_type_id"]);
                    mediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["media_type_id"]);
                    billingTypeID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["billing_type_id"]);
                    fileDuration = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["media_file_duration"]);
                }
            }

            return res;
        }

        public static bool Get_UMMsToCB(int parentGroupID, int fromUserIndexInclusive, int toUserIndexInclusive,
            DateTime fromDate, DateTime toDate,
            ref Dictionary<int, List<UserMediaMark>> domainIdToUserMediaMarksMapping,
            ref Dictionary<UserMediaKey, List<UserMediaMark>> userMediaToMediaMarksMapping,
            ref List<int> userIDsWithNoDomain)
        {
            bool res = false;
            domainIdToUserMediaMarksMapping = new Dictionary<int, List<UserMediaMark>>();
            userMediaToMediaMarksMapping = new Dictionary<UserMediaKey, List<UserMediaMark>>();
            userIDsWithNoDomain = new List<int>();
            StoredProcedure sp = new StoredProcedure("Get_UMMsToCB");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@FromIndex", fromUserIndexInclusive);
            sp.AddParameter("@ToIndex", toUserIndexInclusive);
            sp.AddParameter("@FromDate", fromDate);
            sp.AddParameter("@ToDate", toDate);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                res = true;
                Dictionary<int, int> userIDToDomainIDMapping = new Dictionary<int, int>();
                DataTable usersDomains = ds.Tables[0];
                if (usersDomains != null && usersDomains.Rows != null && usersDomains.Rows.Count > 0)
                {
                    for (int i = 0; i < usersDomains.Rows.Count; i++)
                    {
                        int userID = ODBCWrapper.Utils.GetIntSafeVal(usersDomains.Rows[i]["user_id"]);
                        int domainID = ODBCWrapper.Utils.GetIntSafeVal(usersDomains.Rows[i]["domain_id"]);
                        if (userID > 0 && domainID > 0 && !userIDToDomainIDMapping.ContainsKey(userID))
                        {
                            userIDToDomainIDMapping.Add(userID, domainID);
                        }
                    }
                }
                DataTable mediaMarks = ds.Tables[1];
                if (mediaMarks != null && mediaMarks.Rows != null && mediaMarks.Rows.Count > 0)
                {
                    for (int i = 0; i < mediaMarks.Rows.Count; i++)
                    {
                        int siteGuid = ODBCWrapper.Utils.GetIntSafeVal(mediaMarks.Rows[i]["site_user_guid"]);
                        string udid = ODBCWrapper.Utils.GetSafeStr(mediaMarks.Rows[i]["device_udid"]);
                        int mediaID = ODBCWrapper.Utils.GetIntSafeVal(mediaMarks.Rows[i]["media_id"]);
                        DateTime createdAt = ODBCWrapper.Utils.GetDateSafeVal(mediaMarks.Rows[i]["update_Date"]);
                        int locationSec = ODBCWrapper.Utils.GetIntSafeVal(mediaMarks.Rows[i]["location_sec"]);
                        if (siteGuid > 0 && mediaID > 0 && !createdAt.Equals(ODBCWrapper.Utils.FICTIVE_DATE))
                        {
                            UserMediaMark umm = new UserMediaMark()
                                {
                                    UserID = siteGuid,
                                    UDID = udid,
                                    MediaID = mediaID,
                                    CreatedAt = createdAt,
                                    Location = locationSec
                                };
                            int currDomainID = 0;
                            if (userIDToDomainIDMapping.ContainsKey(siteGuid))
                            {
                                currDomainID = userIDToDomainIDMapping[siteGuid];
                            }
                            if (currDomainID > 0) // domain id exists
                            {
                                if (!domainIdToUserMediaMarksMapping.ContainsKey(currDomainID))
                                {
                                    domainIdToUserMediaMarksMapping.Add(currDomainID, new List<UserMediaMark>());
                                }
                                domainIdToUserMediaMarksMapping[currDomainID].Add(umm);
                            }
                            else
                            {
                                userIDsWithNoDomain.Add(siteGuid);
                            }

                            UserMediaKey umk = new UserMediaKey(siteGuid, mediaID);
                            if (!userMediaToMediaMarksMapping.ContainsKey(umk))
                            {
                                userMediaToMediaMarksMapping.Add(umk, new List<UserMediaMark>());
                            }
                            userMediaToMediaMarksMapping[umk].Add(umm);
                        }
                    } // for
                }
            }
            IComparer<UserMediaMark> ummComparer = new UserMediaMark.UMMDateComparerDesc();
            foreach (KeyValuePair<UserMediaKey, List<UserMediaMark>> kvp in userMediaToMediaMarksMapping)
            {
                kvp.Value.Sort(ummComparer);
            }

            return res;
        }

        public static string GetOrInsert_PlayCycleKey(string siteGuid, long mediaID, long mediaFileID, string udid, long platform,
            long countryID, int mcRuleID, int groupID, bool justInsert)
        {
            string res = string.Empty;
            StoredProcedure sp = new StoredProcedure("GetOrInsert_PlayCycleKey");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", siteGuid ?? string.Empty);
            sp.AddParameter("@MediaID", mediaID);
            sp.AddParameter("@MediaFileID", mediaFileID);
            sp.AddParameter("@DeviceUDID", udid);
            sp.AddParameter("@Platform", platform);
            sp.AddParameter("@CountryID", countryID);
            sp.AddParameter("@RuleID", mcRuleID);
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@JustInsert", justInsert);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["PLAY_CYCLE_KEY"]);
                }
            }

            return res;
        }

        public static void InsertPlayCycleKey(string siteGuid, long mediaID, long mediaFileID, string udid, long platform, long countryID, int mcRuleID, int groupID, string playCycleKey)
        {
            StoredProcedure sp = new StoredProcedure("Insert_PlayCycleKey");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", siteGuid);
            sp.AddParameter("@MediaID", mediaID);
            sp.AddParameter("@MediaFileID", mediaFileID);
            sp.AddParameter("@DeviceUDID", udid);
            sp.AddParameter("@Platform", platform);
            sp.AddParameter("@CountryID", countryID);
            sp.AddParameter("@RuleID", mcRuleID);
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@PlayCycleKey", playCycleKey);
            sp.ExecuteNonQuery();
        }

        public static void Insert_NewPlayCycleKey(int nGroupID, int nMediaID, int nMediaFileID, string sSiteGuid, int nPlatform, string sUDID, int nCountryID, string sPlayCycleKey, int nRuleID = 0)
        {
            GetOrInsert_PlayCycleKey(sSiteGuid, nMediaID, nMediaFileID, sUDID, nPlatform, nCountryID, nRuleID, nGroupID, true);
        }

        public static void Insert_MediaMarkHitActionData(long watcherID, string sessionID, long groupID, long ownerGroupID,
            long mediaID, long mediaFileID, long billingTypeID, long cdnID, long duration, long countryID, long playerID,
            long firstPlayCounter, long playCounter, long loadCounter, long pauseCounter, long stopCounter, long fullScreenCounter,
            long exitFullScreenCounter, long sendToFriendCounter, long playTimeCounter, long fileQualityID, long fileFormatID,
            DateTime startHourDate, long updaterID, long browser, long platform, string siteGuid, string udid,
            long swooshCounter, int mcRuleID)
        {
            StoredProcedure sp = new StoredProcedure("Insert_MediaMarkHitActionData");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@WatcherID", watcherID);
            sp.AddParameter("@SessionID", sessionID);
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@OwnerGroupID", ownerGroupID);
            sp.AddParameter("@MediaID", mediaID);
            sp.AddParameter("@MediaFileID", mediaFileID);
            sp.AddParameter("@BillingTypeID", billingTypeID);
            sp.AddParameter("@CdnID", cdnID);
            sp.AddParameter("@Duration", duration);
            sp.AddParameter("@CountryID", countryID);
            sp.AddParameter("@PlayerID", playerID);
            sp.AddParameter("@FirstPlayCounter", firstPlayCounter);
            sp.AddParameter("@PlayCounter", playCounter);
            sp.AddParameter("@LoadCounter", loadCounter);
            sp.AddParameter("@PauseCounter", pauseCounter);
            sp.AddParameter("@StopCounter", stopCounter);
            sp.AddParameter("@FullScreenCounter", fullScreenCounter);
            sp.AddParameter("@ExitFullScreenCounter", exitFullScreenCounter);
            sp.AddParameter("@SendToFriendCounter", sendToFriendCounter);
            sp.AddParameter("@PlayTimeCounter", playTimeCounter);
            sp.AddParameter("@FileQualityID", fileQualityID);
            sp.AddParameter("@FileFormatID", fileFormatID);
            sp.AddParameter("@StartHourDate", startHourDate);
            sp.AddParameter("@UpdaterID", updaterID);
            sp.AddParameter("@Browser", browser);
            sp.AddParameter("@Platform", platform);
            sp.AddParameter("@SiteGuid", siteGuid);
            sp.AddParameter("@DeviceUDID", udid);
            sp.AddParameter("@SwooshCounter", swooshCounter);
            sp.AddParameter("@RuleID", mcRuleID);

            sp.ExecuteNonQuery();
        }

        public static int Create_SiteGuidsTableForUMMMigration(DateTime from, DateTime to, long groupID)
        {
            StoredProcedure sp = new StoredProcedure("Create_SiteGuidsTableForUMMMigration");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@From", from);
            sp.AddParameter("@To", to);
            sp.AddParameter("@GroupID", groupID);

            return sp.ExecuteReturnValue<int>();
        }

        public static bool Drop_SiteGuidsTableForUMMMigration()
        {
            StoredProcedure sp = new StoredProcedure("Drop_SiteGuidsTableForUMMMigration");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            return sp.ExecuteReturnValue<bool>();
        }

        public static void UpdateOrInsert_UsersNpvrMark(int nDomainID, int nSiteUserGuid, string sUDID, string sAssetID, int nGroupID, int nLoactionSec,
            int fileDuration, string action, bool isFirstPlay = false, int finishedPercent = 95)
        {
            var mediaMarkManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            var mediaHitManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIA_HITS);
            var domainMarksManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.DOMAIN_CONCURRENCY);

            int limitRetries = RETRY_LIMIT;
            Random r = new Random();

            DateTime currentDate = DateTime.UtcNow;

            string docKey = UtilsDal.getDomainMediaMarksDocKey(nDomainID);

            var dev = new UserMediaMark()
            {
                Location = nLoactionSec,
                UDID = sUDID,
                MediaID = 0,
                UserID = nSiteUserGuid,
                CreatedAt = currentDate,
                CreatedAtEpoch = Utils.DateTimeToUnixTimestamp(currentDate),
                playType = ApiObjects.ePlayType.NPVR.ToString(),
                NpvrID = sAssetID,
                AssetAction = action,
                FileDuration = fileDuration,
                AssetTypeId = (int)eAssetTypes.NPVR
            };

            while (limitRetries >= 0)
            {
                bool res = UpdateDomainConcurrency(sUDID, domainMarksManager, docKey, dev);

                if (!res)
                {
                    Thread.Sleep(r.Next(50));
                    limitRetries--;
                }
                else
                    break;
            }

            string mmKey = UtilsDal.getUserNpvrMarkDocKey(nSiteUserGuid, sAssetID);
            var userMediaMark = new UserMediaMark()
            {
                Location = nLoactionSec,
                UDID = sUDID,
                MediaID = 0,
                UserID = nSiteUserGuid,
                CreatedAt = currentDate,
                CreatedAtEpoch = Utils.DateTimeToUnixTimestamp(currentDate),
                playType = ApiObjects.ePlayType.NPVR.ToString(),
                NpvrID = sAssetID,
                AssetAction = action,
                FileDuration = fileDuration,
                AssetTypeId = (int)eAssetTypes.NPVR
            };

            limitRetries = RETRY_LIMIT;

            bool success = false;

            if (isFirstPlay)
            {
                while (limitRetries >= 0 && !success)
                {
                    UpdateOrInsert_UsersMediaMarkOrHit(mediaMarkManager, sUDID, ref limitRetries, r, mmKey, ref success, userMediaMark);
                }
            }

            success = false;
            while (limitRetries >= 0 && !success)
            {
                UpdateOrInsert_UsersMediaMarkOrHit(mediaHitManager, sUDID, ref limitRetries, r, mmKey, ref success, userMediaMark);
            }
        }

        public static void UpdateOrInsert_UsersEpgMark(int nDomainID, int nSiteUserGuid, string sUDID, int nAssetID, int nGroupID, int nLoactionSec,
            int fileDuration, string action, bool isFirstPlay)
        {
            var mediaMarksManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            var mediaHitsManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIA_HITS);
            var domainMarksManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.DOMAIN_CONCURRENCY);

            int limitRetries = RETRY_LIMIT;
            Random r = new Random();

            DateTime currentDate = DateTime.UtcNow;
            string docKey = UtilsDal.getDomainMediaMarksDocKey(nDomainID);

            var dev = new UserMediaMark()
            {
                Location = nLoactionSec,
                UDID = sUDID,
                MediaID = nAssetID,
                UserID = nSiteUserGuid,
                CreatedAt = currentDate,
                CreatedAtEpoch = Utils.DateTimeToUnixTimestamp(currentDate),
                playType = ApiObjects.ePlayType.EPG.ToString(),
                AssetAction = action,
                FileDuration = fileDuration,
                AssetTypeId = (int)eAssetTypes.EPG
            };

            while (limitRetries >= 0)
            {
                bool res = UpdateDomainConcurrency(sUDID, domainMarksManager, docKey, dev);

                if (!res)
                {
                    Thread.Sleep(r.Next(50));
                    limitRetries--;
                }
                else
                    break;
            }

            string mmKey = UtilsDal.getUserEpgMarkDocKey(nSiteUserGuid, nAssetID.ToString());

            if (isFirstPlay)
            {
                UpdateOrInsert_UserEpgMarkOrHit(mediaMarksManager, r, dev, mmKey);
            }

            UpdateOrInsert_UserEpgMarkOrHit(mediaHitsManager, r, dev, mmKey);
        }

        private static bool UpdateOrInsert_UserEpgMarkOrHit(CouchbaseManager.CouchbaseManager manager, Random r, UserMediaMark dev, string mmKey)
        {
            bool success = false;
            int limitRetries = RETRY_LIMIT;

            while (limitRetries >= 0)
            {
                ulong version;
                var data = manager.GetWithVersion<string>(mmKey, out version);

                MediaMarkLog umm = new MediaMarkLog();

                //For quick last position access
                umm.LastMark = dev;

                TimeSpan? epgDocExpiry = null;

                uint cbEpgDocumentExpiryDays;
                uint.TryParse(CB_EPG_DOCUMENT_EXPIRY_DAYS, out cbEpgDocumentExpiryDays);

                bool res = (epgDocExpiry.HasValue) ?
                    manager.SetWithVersion(mmKey, JsonConvert.SerializeObject(umm, Formatting.None), version, cbEpgDocumentExpiryDays * 24 * 60 * 60)
                    : manager.SetWithVersion(mmKey, JsonConvert.SerializeObject(umm, Formatting.None), version);

                if (!res)
                {
                    Thread.Sleep(r.Next(50));
                    limitRetries--;
                }
                else
                {
                    success = true;
                    break;
                }
            }

            return success;
        }

        public static int GetLastPosition(string NpvrID, int userID)
        {
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIA_HITS);
            string key = UtilsDal.getUserNpvrMarkDocKey(userID, NpvrID);
            var data = cbManager.Get<string>(key);
            if (data == null)
                return 0;
            var umm = JsonConvert.DeserializeObject<MediaMarkLog>(data);
            return umm.LastMark.Location;
        }

        // get all devices last position in domain by media_id 
        public static DomainMediaMark GetDomainLastPosition(int mediaID, int userID, int domainID)
        {
            var domainMarksManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.DOMAIN_CONCURRENCY);

            // get domain document 
            string key = UtilsDal.getDomainMediaMarksDocKey(domainID);
            var data = domainMarksManager.Get<string>(key);
            if (data == null)
                return null;

            // get all last position order by desc              
            var dmm = JsonConvert.DeserializeObject<DomainMediaMark>(data);
            dmm.devices = dmm.devices.Where(x => x.MediaID == mediaID).ToList();
            return dmm;
        }

        public static void Get_IPCountryCode(long ipVal, ref int countryID)
        {
            StoredProcedure sp = new StoredProcedure("Get_IPCountryCode");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@IPVal", ipVal);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count == 1)
            {
                DataTable ipTable = ds.Tables[0];
                if (ipTable != null && ipTable.Rows != null && ipTable.Rows.Count > 0)
                {
                    countryID = ODBCWrapper.Utils.GetIntSafeVal(ipTable.Rows[0]["COUNTRY_ID"]);
                }
            }
        }

        public static bool GetMediaPlayData(int mediaID, int mediaFileID, ref int ownerGroupID, ref int cdnID, ref int qualityID, ref int formatID, ref int mediaTypeID, ref int billingTypeID, ref int fileDuration)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("GetMediaPlayData");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@MediaID", mediaID);
            sp.AddParameter("@MediaFileID", mediaFileID);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count == 1)
            {
                res = true;
                DataTable mpData = ds.Tables[0];
                if (mpData != null && mpData.Rows != null && mpData.Rows.Count > 0)
                {
                    ownerGroupID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["group_id"]);
                    cdnID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["streaming_suplier_id"]);
                    qualityID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["media_quality_id"]);
                    formatID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["media_file_type_id"]);
                    mediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["media_type_id"]);
                    billingTypeID = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["billing_type_id"]);
                    fileDuration = ODBCWrapper.Utils.GetIntSafeVal(mpData.Rows[0]["media_file_duration"]);
                }
            }
            return res;
        }

        public static List<int> Get_LinearMediaType(int parentGroupID)
        {
            List<int> res = new List<int>();

            StoredProcedure sp = new StoredProcedure("Get_LinearMediaType");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", parentGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res.Add(ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["ID"]));
                    }
                }
            }

            return res;
        }

        public static DomainMediaMark GetAssetLastPosition(string assetID, eAssetTypes assetType, List<int> users)
        {
            DomainMediaMark dmmResponse = new DomainMediaMark();

            //Create users keys according to asset type
            List<string> userKeys = new List<string>();
            string userDocKey = string.Empty;
            switch (assetType)
            {
                case eAssetTypes.EPG:
                    foreach (int userID in users)
                    {
                        userDocKey = UtilsDal.getUserEpgMarkDocKey(userID, assetID);
                        userKeys.Add(userDocKey);
                    }
                    break;
                case eAssetTypes.NPVR:
                    foreach (int userID in users)
                    {
                        userDocKey = UtilsDal.getUserNpvrMarkDocKey(userID, assetID);
                        userKeys.Add(userDocKey);
                    }
                    break;
                case eAssetTypes.MEDIA:
                    int mediaID;
                    if (int.TryParse(assetID, out mediaID))
                    {
                        foreach (int userID in users)
                        {
                            userDocKey = UtilsDal.getUserMediaMarkDocKey(userID, mediaID);
                            userKeys.Add(userDocKey);
                        }
                    }
                    break;
                default:
                    break;
            }

            // get all documents from CB
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIA_HITS);
            IDictionary<string, MediaMarkLog> usersData = cbManager.GetValues<MediaMarkLog>(userKeys, true, true);
            List<UserMediaMark> usersMediaMark = new List<UserMediaMark>();

            if (usersData == null)
                return null;

            if (usersData != null && usersData.Count > 0)
            {
                foreach (KeyValuePair<string, MediaMarkLog> userData in usersData)
                {
                    if (userData.Value != null)
                    {
                        MediaMarkLog mediaMarkLog = userData.Value;
                        if (mediaMarkLog != null && mediaMarkLog.LastMark != null)
                        {
                            usersMediaMark.Add(mediaMarkLog.LastMark);
                        }
                    }
                }
            }

            dmmResponse.devices = usersMediaMark;
            return dmmResponse;
        }

        public static DomainMediaMark GetDomainLastPosition(int media_id, List<int> usersKey, int domain_id)
        {
            DomainMediaMark dmm = new DomainMediaMark();
            dmm.domainID = domain_id;

            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            // create Keys 
            List<string> keys = new List<string>();
            string docKey = string.Empty;
            foreach (int user in usersKey)
            {
                docKey = UtilsDal.getUserMediaMarkDocKey(user, media_id);
                keys.Add(docKey);
            }
            // get all documents from CB
            IDictionary<string, MediaMarkLog> data = cbManager.GetValues<MediaMarkLog>(keys, true, true);

            List<UserMediaMark> oRes = new List<UserMediaMark>();

            if (data == null)
                return null;

            if (data != null && data.Count > 0)
            {
                foreach (KeyValuePair<string, MediaMarkLog> item in data)
                {
                    if (item.Value != null)
                    {
                        MediaMarkLog mml = item.Value;
                        if (mml != null && mml.LastMark != null)
                        {
                            oRes.Add(mml.LastMark);
                        }
                    }
                }
            }

            dmm.devices = oRes;
            return dmm;
        }

        public static bool UpdateOrInsert_EPGDeafultsValues(Dictionary<int, List<string>> dMetasDefaults, Dictionary<int, List<string>> dTagsDefaults, int nEpgChannelID)
        {
            StoredProcedure sp = new StoredProcedure("UpdateOrInsert_EPGDeafultsValues");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@EpgChannelID", nEpgChannelID);
            sp.AddKeyValueListParameter<int, string>("@MetasDefaults", dMetasDefaults, "key", "value");
            sp.AddKeyValueListParameter<int, string>("@TagsDefaults", dTagsDefaults, "key", "value");

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool UpdateOrInsert_EPGTagTypeWithDeafultsValues(Dictionary<int, List<string>> dTagsDefaults, int nEpgTagTypelID, int groupID, int isActive,
            int? orderNum, string TagName, int tagTypeFlag)
        {
            StoredProcedure sp = new StoredProcedure("UpdateOrInsert_EPGTagTypeWithDeafultsValues");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@EpgTagTypelID", nEpgTagTypelID);
            sp.AddParameter("@TagName", TagName);
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@isActive", isActive);
            sp.AddParameter("@orderNum", orderNum);
            sp.AddParameter("@tagTypeFlag", tagTypeFlag);
            sp.AddKeyValueListParameter<int, string>("@dTagsDefaults", dTagsDefaults, "key", "value");

            return sp.ExecuteReturnValue<bool>();
        }

        public static Dictionary<string, string> GetMinPeriods()
        {
            Dictionary<string, string> dicMinPeriods = new Dictionary<string, string>();
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_MinPeriods");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        object oId = dt.Rows[i]["ID"];
                        object oDescription = dt.Rows[i]["Description"];

                        if (oId != null && oId != DBNull.Value &&
                            oDescription != null && oDescription != DBNull.Value)
                        {
                            string sId = Convert.ToString(oId);
                            string sDescription = Convert.ToString(oDescription);

                            if (!dicMinPeriods.ContainsKey(sId))
                            {
                                dicMinPeriods.Add(sId, sDescription);
                            }
                        }
                    }
                }
            }

            return dicMinPeriods;
        }

        public static List<int> GetGroupServices(int groupID, int? serviceID = null)
        {
            List<int> lServices = new List<int>();
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetGroupServices");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);
            if (serviceID != null)
                sp.AddParameter("@ServiceID", serviceID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        lServices.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "SERVICE_ID"));
                    }
                }
            }

            return lServices;
        }

        /// <summary>
        /// For a given group, gets the media types mappings of Id to name and vice versa
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="idToName"></param>
        /// <param name="nameToId"></param>
        public static void GetMediaTypes(int groupId, out Dictionary<int, string> idToName,
            out Dictionary<string, int> nameToId, out Dictionary<int, int> parentMediaTypes,
            out List<int> linearChannelMediaTypes)
        {
            idToName = new Dictionary<int, string>();
            nameToId = new Dictionary<string, int>();
            parentMediaTypes = new Dictionary<int, int>();
            linearChannelMediaTypes = new List<int>();

            DataTable mediaTypes = GetMediaTypesTable(groupId);

            if (mediaTypes != null)
            {
                foreach (DataRow mediaType in mediaTypes.Rows)
                {
                    int id = ODBCWrapper.Utils.ExtractInteger(mediaType, "ID");
                    string name = ODBCWrapper.Utils.ExtractString(mediaType, "NAME");
                    int parent = ODBCWrapper.Utils.ExtractInteger(mediaType, "PARENT_TYPE_ID");
                    int isLinear = ODBCWrapper.Utils.ExtractInteger(mediaType, "IS_LINEAR");

                    idToName.Add(id, name);
                    nameToId.Add(name, id);
                    parentMediaTypes.Add(id, parent);

                    if (isLinear == 1)
                    {
                        linearChannelMediaTypes.Add(id);
                    }
                }
            }
        }

        /// For a given group, gets the media types data table
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="idToName"></param>
        /// <param name="nameToId"></param>
        public static DataTable GetMediaTypesTable(int groupId)
        {
            DataTable table = null;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_GroupMediaTypes");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupId", groupId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                DataTable mediaTypes = dataSet.Tables[0];

                if (mediaTypes != null && mediaTypes.Rows != null && mediaTypes.Rows.Count > 0)
                {
                    table = mediaTypes;
                }
            }

            return table;
        }

        ///
        /// Builds the list of the regions of a given group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="isRegionalizationEnabled"></param>
        /// <param name="defaultRegion"></param>
        /// <returns></returns>
        public static void GetRegionalizationSettings(int groupId, out bool isRegionalizationEnabled, out int defaultRegion)
        {
            isRegionalizationEnabled = false;
            defaultRegion = 0;

            // Call stored procedure that checks if this group has regionalization or not
            ODBCWrapper.StoredProcedure storedProcedureDefaultRegion = new ODBCWrapper.StoredProcedure("Get_GroupDefaultRegion");
            storedProcedureDefaultRegion.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedureDefaultRegion.AddParameter("@GroupID", groupId);

            DataSet groupDataSet = storedProcedureDefaultRegion.ExecuteDataSet();

            if (groupDataSet != null && groupDataSet.Tables != null && groupDataSet.Tables.Count == 1)
            {
                DataTable groupTable = groupDataSet.Tables[0];

                if (groupTable != null && groupTable.Rows != null && groupTable.Rows.Count > 0 && groupTable.Rows[0] != null)
                {
                    DataRow groupRow = groupTable.Rows[0];

                    isRegionalizationEnabled = ODBCWrapper.Utils.ExtractBoolean(groupRow, "is_regionalization_enabled");
                    defaultRegion = ODBCWrapper.Utils.ExtractInteger(groupRow, "default_region");
                }
            }
        }

        /// <summary>
        /// Builds a region object based on a data row
        /// </summary>
        /// <param name="regionRow"></param>
        /// <returns></returns>
        private static Region BuildRegion(DataRow regionRow)
        {
            Region region = new Region();

            region.id = ODBCWrapper.Utils.ExtractInteger(regionRow, "ID");
            region.name = ODBCWrapper.Utils.ExtractString(regionRow, "NAME");
            region.externalId = ODBCWrapper.Utils.ExtractString(regionRow, "EXTERNAL_ID");
            region.groupId = ODBCWrapper.Utils.ExtractInteger(regionRow, "GROUP_ID");

            return (region);
        }

        public static DataSet GetMediaByEpgChannelIds(int groupId, List<string> epgChannelIds)
        {
            StoredProcedure storedProcedure = new StoredProcedure("[Get_MediasByEpgIds]");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddIDListParameter("@epgIdentifiers", epgChannelIds, "ID");
            storedProcedure.AddParameter("@GroupID", groupId);

            return storedProcedure.ExecuteDataSet();
        }

        public static int DeleteAllEpgDetailsByChannelDates(DateTime fromDate, DateTime toDate, int channelID, int updateStatus, int currentStatus)
        {
            StoredProcedure sp = new StoredProcedure("DeleteAllEpgDetailsByChannelDates");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@From", fromDate);
            sp.AddParameter("@To", toDate);
            sp.AddParameter("@channelID", channelID);
            sp.AddParameter("@updateStatus", updateStatus);
            sp.AddParameter("@currentStatus", currentStatus);

            return sp.ExecuteReturnValue<int>();
        }

        /// <summary>
        /// For a given group and regions, get all linear channels EPG_IDENTIFIER that match them
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="regionIds"></param>
        /// <returns></returns>
        public static List<long> Get_EpgIdentifier_ByRegion(int groupId, List<int> regionIds)
        {
            List<long> epgIdentifiers = new List<long>();

            // SP does a join between media and media_regions, 
            // thus finding the ID of the channels from 'media' table,
            // while also filtering by REGION_ID from 'media_regions' table

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EpgIdentifier_ByRegion");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@RegionID", regionIds, "id");
            sp.AddParameter("@GroupID", groupId);

            DataSet ds = sp.ExecuteDataSet();

            // Simple null checks
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 &&
                ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];

                // Extract all long values from the data tables only column
                foreach (DataRow dr in dt.Rows)
                {
                    epgIdentifiers.Add(
                        ODBCWrapper.Utils.ExtractValue<long>(dr, "ID"));
                }
            }

            return epgIdentifiers;
        }

        public static Dictionary<int, List<EpgPicture>> GetGroupTreeMultiPicEpgUrl(int parentGroupID, ref List<Ratio> epgRatios)
        {
            Dictionary<int, List<EpgPicture>> res = null;

            StoredProcedure sp = new StoredProcedure("GetGroupTreeMultiPicEpgUrl");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@ParentGroupID", parentGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable groupRatioTable = ds.Tables[0];
                string baseUrl = string.Empty;
                int width = 0;
                int height = 0;
                string ratio = string.Empty;
                int ratioId = 0;

                if (groupRatioTable != null && groupRatioTable.Rows != null && groupRatioTable.Rows.Count > 0)
                {
                    res = new Dictionary<int, List<EpgPicture>>();
                    foreach (DataRow dr in groupRatioTable.Rows)
                    {
                        int groupID = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_ID");
                        if (groupID > 0)
                        {
                            baseUrl = ODBCWrapper.Utils.GetSafeStr(dr, "baseURL");
                            if (baseUrl.Length > 0 && baseUrl[baseUrl.Length - 1] != '/')
                            {
                                baseUrl = String.Concat(baseUrl, '/');
                            }
                            width = ODBCWrapper.Utils.GetIntSafeVal(dr, "WIDTH");
                            height = ODBCWrapper.Utils.GetIntSafeVal(dr, "HEIGHT");
                            ratio = ODBCWrapper.Utils.GetSafeStr(dr, "ratio");
                            ratioId = ODBCWrapper.Utils.GetIntSafeVal(dr, "ratio_id");

                            EpgPicture picture = new EpgPicture();
                            picture.Initialize(width, height, ratio, baseUrl);
                            picture.RatioId = ratioId;
                            if (!res.ContainsKey(groupID))
                            {
                                res.Add(groupID, new List<EpgPicture>() { picture });
                            }
                            else
                            {
                                res[groupID].Add(picture);
                            }
                        }
                    }
                }
                if (ds.Tables.Count > 1)
                {
                    DataTable groupEpgRatioTable = ds.Tables[1];
                    width = 0;
                    height = 0;
                    ratio = string.Empty;
                    if (groupEpgRatioTable != null && groupEpgRatioTable.Rows != null && groupEpgRatioTable.Rows.Count > 0)
                    {
                        if (res == null)
                        {
                            res = new Dictionary<int, List<EpgPicture>>();
                        }

                        foreach (DataRow dr in groupEpgRatioTable.Rows)
                        {
                            int groupID = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_ID");
                            if (groupID > 0)
                            {
                                width = ODBCWrapper.Utils.GetIntSafeVal(dr, "WIDTH");
                                height = ODBCWrapper.Utils.GetIntSafeVal(dr, "HEIGHT");
                                ratio = ODBCWrapper.Utils.GetSafeStr(dr, "ratio");
                                ratioId = ODBCWrapper.Utils.GetIntSafeVal(dr, "ratio_id");

                                EpgPicture picture = new EpgPicture();
                                picture.Initialize(width, height, ratio, baseUrl);
                                picture.RatioId = ratioId;
                                if (!res.ContainsKey(groupID))
                                {
                                    res.Add(groupID, new List<EpgPicture>() { picture });
                                }
                                else
                                {
                                    if (!res[groupID].Exists(x => x.Ratio == ratio && x.PicHeight == height && x.PicWidth == width))
                                    {
                                        res[groupID].Add(picture);
                                    }
                                }
                            }
                        }
                    }

                    if (ds.Tables.Count > 2)
                    {
                        epgRatios = new List<Ratio>();

                        // epg ratio table
                        DataTable epgRatioTable = ds.Tables[2];
                        foreach (DataRow dr in epgRatioTable.Rows)
                        {
                            epgRatios.Add(new Ratio()
                            {
                                Id = ODBCWrapper.Utils.GetIntSafeVal(dr, "id"),
                                Name = ODBCWrapper.Utils.GetSafeStr(dr, "ratio")
                            });
                        }
                    }
                }
                if (res == null)
                {
                    res = new Dictionary<int, List<EpgPicture>>(0);
                }
            }
            else
            {
                res = new Dictionary<int, List<EpgPicture>>(0);
            }

            return res;
        }

        public static DataSet Get_FileAndMediaBasicDetails(int[] mediaFiles)
        {
            DataSet ds = null;
            try
            {
                StoredProcedure sp = new StoredProcedure("Get_FileAndMediaBasicDetails");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddIDListParameter<int>("@mediaFiles", mediaFiles.ToList(), "id");

                ds = sp.ExecuteDataSet();
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
            }
            return ds;
        }

        public static RecommendationEngine GetRecommendationEngine(int groupID, int engineId, int? isActive = null, int status = 1)
        {
            RecommendationEngine result = null;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_RecommendationEngine");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupID);
            storedProcedure.AddParameter("@RecommendationEngineId", engineId);
            storedProcedure.AddParameter("@Status", status);
            if (isActive.HasValue)
            {
                storedProcedure.AddParameter("@IsActive", isActive.Value);
            }

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 1)
            {
                if (dataSet.Tables[0] != null && dataSet.Tables[0].Rows.Count > 0)
                {
                    DataRow row = dataSet.Tables[0].Rows[0];

                    if (row != null)
                    {
                        result = new RecommendationEngine()
                        {
                            AdapterUrl = ODBCWrapper.Utils.ExtractString(row, "adapter_url"),
                            ExternalIdentifier = ODBCWrapper.Utils.ExtractString(row, "external_identifier"),
                            ID = engineId,
                            IsActive = ODBCWrapper.Utils.ExtractBoolean(row, "is_active"),
                            Name = ODBCWrapper.Utils.ExtractString(row, "name"),
                            Settings = new List<RecommendationEngineSettings>(),
                            SharedSecret = ODBCWrapper.Utils.ExtractString(row, "shared_secret"),
                            Status = ODBCWrapper.Utils.ExtractInteger(row, "status"),
                            IsDefault = ODBCWrapper.Utils.ExtractBoolean(row, "is_default"),
                        };
                    }

                    if (dataSet.Tables[1] != null && dataSet.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow setting in dataSet.Tables[1].Rows)
                        {
                            result.Settings.Add(new RecommendationEngineSettings()
                            {
                                key = ODBCWrapper.Utils.ExtractString(setting, "keyName"),
                                value = ODBCWrapper.Utils.ExtractString(setting, "value"),
                            });
                        }
                    }
                }
            }

            return result;
        }

        public static RecommendationEngine GetRecommendationEngineInternalID(int groupID, string externalIdentifier)
        {
            RecommendationEngine result = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_RecommendationEngineByExternalD");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@external_identifier", externalIdentifier);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                result = CreateRecommendationEngine(ds.Tables[0].Rows[0]);
            }

            return result;
        }

        public static RecommendationEngine InsertRecommendationEngine(int groupID, RecommendationEngine recommendationEngine)
        {
            RecommendationEngine result = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_RecommendationEngine");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@name", recommendationEngine.Name);
            sp.AddParameter("@adapter_url", recommendationEngine.AdapterUrl);
            sp.AddParameter("@external_identifier", recommendationEngine.ExternalIdentifier);
            sp.AddParameter("@shared_secret", recommendationEngine.SharedSecret);
            sp.AddParameter("@isActive", recommendationEngine.IsActive);

            DataTable dt = CreateDataTable(recommendationEngine.Settings);
            sp.AddDataTableParameter("@KeyValueList", dt);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                result = CreateRecommendationEngine(ds.Tables[0].Rows[0]);
            }

            return result;
        }

        private static RecommendationEngine CreateRecommendationEngine(DataRow dr)
        {
            RecommendationEngine result = null;

            //if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            if (dr != null)
            {
                result = new RecommendationEngine();
                result.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(dr, "adapter_url");
                result.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(dr, "external_identifier");
                result.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                int is_Active = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active");
                result.IsActive = is_Active == 1 ? true : false;
                result.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                result.SharedSecret = ODBCWrapper.Utils.GetSafeStr(dr, "shared_secret");
            }

            return result;

        }

        private static DataTable CreateDataTable(List<RecommendationEngineSettings> recommendationEngineSettings)
        {
            DataTable resultTable = new DataTable("resultTable");
            ;
            try
            {
                resultTable.Columns.Add("idkey", typeof(string));
                resultTable.Columns.Add("value", typeof(string));

                foreach (RecommendationEngineSettings item in recommendationEngineSettings)
                {
                    DataRow row = resultTable.NewRow();
                    row["idkey"] = item.key;
                    row["value"] = item.value;
                    resultTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return null;
            }

            return resultTable;
        }

        public static bool DeleteRecommendationEngine(int groupID, int recommendationEngineId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_RecommendationEngine");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", recommendationEngineId);
                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static RecommendationEngine SetRecommendationEngine(int groupID, RecommendationEngine recommendationEngine)
        {
            RecommendationEngine ossAdapterRes = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_RecommendationEngine");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", recommendationEngine.ID);
                sp.AddParameter("@name", recommendationEngine.Name);
                sp.AddParameter("@external_identifier", recommendationEngine.ExternalIdentifier);
                sp.AddParameter("@shared_secret", recommendationEngine.SharedSecret);
                sp.AddParameter("@adapter_url", recommendationEngine.AdapterUrl);
                sp.AddParameter("@isActive", recommendationEngine.IsActive);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    ossAdapterRes = new RecommendationEngine();
                    ossAdapterRes.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "adapter_url");
                    ossAdapterRes.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "external_identifier");
                    ossAdapterRes.ID = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                    int is_Active = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "is_active");
                    ossAdapterRes.IsActive = is_Active == 1 ? true : false;
                    ossAdapterRes.Name = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "name");
                    ossAdapterRes.SharedSecret = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "shared_secret");

                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[1].Rows)
                        {
                            string key = ODBCWrapper.Utils.GetSafeStr(dr, "key");
                            string value = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                            if (ossAdapterRes.Settings == null)
                            {
                                ossAdapterRes.Settings = new List<RecommendationEngineSettings>();
                            }
                            ossAdapterRes.Settings.Add(new RecommendationEngineSettings(key, value));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
            }

            return ossAdapterRes;
        }

        public static List<RecommendationEngine> GetRecommendationEngineList(int groupID, int status = 1, int isActive = 1)
        {
            List<RecommendationEngine> res = new List<RecommendationEngine>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_RecommendationEngineList");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@status", status);
                DataSet ds = sp.ExecuteDataSetWithListParam();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtResult = ds.Tables[0];
                    if (dtResult != null && dtResult.Rows != null && dtResult.Rows.Count > 0)
                    {
                        RecommendationEngine recommendationEngine = null;
                        foreach (DataRow dr in dtResult.Rows)
                        {
                            recommendationEngine = CreateRecommendationEngine(dr);
                            if (recommendationEngine != null)
                            {
                                res.Add(recommendationEngine);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                res = new List<RecommendationEngine>();
            }
            return res;
        }

        public static bool InsertRecommendationEngineSettings(int groupID, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_RecommendationEngineSettings");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", recommendationEngineId);

                DataTable dt = CreateDataTable(settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                bool isInsert = sp.ExecuteReturnValue<bool>();
                return isInsert;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static bool SetRecommendationEngineSettings(int groupID, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_RecommendationEngineSettings");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", recommendationEngineId);

                DataTable dt = CreateDataTable(settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                bool isSet = sp.ExecuteReturnValue<bool>();
                return isSet;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static bool DeleteRecommendationEngineSettings(int groupID, int recommendationEngineId, List<RecommendationEngineSettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_RecommendationEngineSettings");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", recommendationEngineId);
                DataTable dt = CreateDataTable(settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static List<RecommendationEngine> GetRecommendationEngineSettingsList(int groupID, int recommendationEngineId = 0, int status = 1, int isActive = 1)
        {
            List<RecommendationEngine> res = new List<RecommendationEngine>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetRecommendationEngineSettingsList");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@recommendationEngineId", recommendationEngineId);
                sp.AddParameter("@status", status);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtPG = ds.Tables[0];
                    DataTable dtConfig = ds.Tables[1];
                    if (dtPG != null && dtPG.Rows != null && dtPG.Rows.Count > 0)
                    {
                        RecommendationEngine recommendationEngine = null;
                        foreach (DataRow dr in dtPG.Rows)
                        {
                            recommendationEngine = new RecommendationEngine();
                            recommendationEngine.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                            recommendationEngine.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                            recommendationEngine.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(dr, "external_identifier");
                            recommendationEngine.SharedSecret = ODBCWrapper.Utils.GetSafeStr(dr, "shared_secret");
                            recommendationEngine.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(dr, "adapter_url");
                            int is_Active = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active");
                            recommendationEngine.IsActive = is_Active == 1 ? true : false;

                            if (dtConfig != null)
                            {
                                DataRow[] drpc = dtConfig.Select("recommendation_engine_id =" + recommendationEngine.ID);

                                foreach (DataRow drp in drpc)
                                {
                                    string key = ODBCWrapper.Utils.GetSafeStr(drp, "key");
                                    string value = ODBCWrapper.Utils.GetSafeStr(drp, "value");
                                    if (recommendationEngine.Settings == null)
                                    {
                                        recommendationEngine.Settings = new List<RecommendationEngineSettings>();
                                    }
                                    recommendationEngine.Settings.Add(new RecommendationEngineSettings(key, value));
                                }
                            }
                            res.Add(recommendationEngine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                res = new List<RecommendationEngine>();
            }
            return res;
        }

        public static RecommendationEngine SetRecommendationEngineSharedSecret(int groupID, int recommendationEngineId, string sharedSecret)
        {
            RecommendationEngine result = null;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_RecommendationEngineSharedSecret");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupID);
            sp.AddParameter("@id", recommendationEngineId);
            sp.AddParameter("@sharedSecret", sharedSecret);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                result = CreateRecommendationEngine(ds.Tables[0].Rows[0]);
            }

            return result;
        }

        public static List<ExternalChannel> GetExternalChannel(int groupID, int status = 1, int isActive = 1)
        {
            List<ExternalChannel> res = new List<ExternalChannel>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_ExternalChannelList");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@status", status);
                DataSet ds = sp.ExecuteDataSetWithListParam();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtResult = ds.Tables[0];
                    if (dtResult != null && dtResult.Rows != null && dtResult.Rows.Count > 0)
                    {
                        ExternalChannel externalChannelBase = null;
                        foreach (DataRow dr in dtResult.Rows)
                        {
                            externalChannelBase = SetExternalChannel(dr);
                            res.Add(externalChannelBase);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                res = new List<ExternalChannel>();
            }
            return res;
        }

        public static ExternalChannel GetExternalChannelById(int groupID, int channelId, int status = 1, int? isActive = null)
        {
            ExternalChannel result = null;

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_ExternalChannel");
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedure.AddParameter("@GroupID", groupID);
            storedProcedure.AddParameter("@id", channelId);
            storedProcedure.AddParameter("@Status", status);
            if (isActive.HasValue)
            {
                storedProcedure.AddParameter("@IsActive", isActive.Value);
            }

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                result = SetExternalChannel(dataSet);
            }

            return result;
        }

        public static ExternalChannel GetExternalChannel(string channelId)
        {
            ExternalChannel result = null;

            DataRow row = ODBCWrapper.Utils.GetTableSingleRow("external_channels", channelId, "", 0);

            if (row != null)
            {
                // Make sure external channel is active before creating it
                int status = ODBCWrapper.Utils.ExtractInteger(row, "status");
                int isActive = ODBCWrapper.Utils.ExtractInteger(row, "is_active");

                if (status == 1 && isActive == 1)
                {
                    result = SetExternalChannel(row);

                    ExternalRecommendationEngineEnrichment enrichments = (ExternalRecommendationEngineEnrichment)ODBCWrapper.Utils.ExtractInteger(row, "enrichments");

                    foreach (var currentValue in Enum.GetValues(typeof(ExternalRecommendationEngineEnrichment)))
                    {
                        if ((enrichments & (ExternalRecommendationEngineEnrichment)currentValue) > 0)
                        {
                            result.Enrichments.Add((ExternalRecommendationEngineEnrichment)currentValue);
                        }
                    }
                }
            }

            return result;
        }

        public static int GetExternalChannelIdByExternalIdentifier(int groupId, string externalIdentifier)
        {
            int result = 0;

            object resultObject = ODBCWrapper.Utils.GetTableSingleVal("external_channels", "ID", "external_identifier", "=", externalIdentifier);

            if (resultObject != DBNull.Value)
            {
                result = Convert.ToInt32(resultObject);
            }

            return result;
        }

        public static ExternalChannel GetExternalChannelInternalID(int groupID, string externalIdentifier)
        {
            ExternalChannel result = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_ExternalChannelByExternalD");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@external_identifier", externalIdentifier);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                result = SetExternalChannel(ds.Tables[0].Rows[0]);
            }

            return result;
        }

        public static ExternalChannel InsertExternalChannel(int groupID, ExternalChannel externalChannel)
        {
            ExternalChannel externalChannelRes = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_ExternalChannel");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupID);
            sp.AddParameter("@name", externalChannel.Name);
            sp.AddParameter("@isActive", externalChannel.IsActive);
            sp.AddParameter("@externalIdentifier", externalChannel.ExternalIdentifier);
            sp.AddParameter("@recommendationEngineId", externalChannel.RecommendationEngineId);
            sp.AddParameter("@filterExpression", externalChannel.FilterExpression);
            int enrichments = GetEnrichments(externalChannel.Enrichments);
            sp.AddParameter("@enrichments", enrichments);

            DataSet ds = sp.ExecuteDataSet();

            externalChannelRes = SetExternalChannel(ds);

            return externalChannelRes;
        }

        public static bool DeleteExternalChannel(int groupID, int externalChannelId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_ExternalChannel");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", externalChannelId);
                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static ExternalChannel SetExternalChannel(int groupID, ExternalChannel externalChannel)
        {
            ExternalChannel externalChannelRes = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_ExternalChannel");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@ID", externalChannel.ID);
            sp.AddParameter("@groupId", groupID);
            sp.AddParameter("@name", externalChannel.Name);
            sp.AddParameter("@isActive", externalChannel.IsActive);
            sp.AddParameter("@externalIdentifier", externalChannel.ExternalIdentifier);
            sp.AddParameter("@recommendationEngineId", externalChannel.RecommendationEngineId);
            sp.AddParameter("@filterExpression", externalChannel.FilterExpression);
            int enrichments = GetEnrichments(externalChannel.Enrichments);
            sp.AddParameter("@enrichments", enrichments);

            DataSet ds = sp.ExecuteDataSet();

            externalChannelRes = SetExternalChannel(ds);

            return externalChannelRes;
        }

        private static ExternalChannel SetExternalChannel(DataSet ds)
        {
            ExternalChannel externalChannelRes = null;

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                externalChannelRes = SetExternalChannel(ds.Tables[0].Rows[0]);
            }

            return externalChannelRes;
        }

        private static ExternalChannel SetExternalChannel(DataRow dr)
        {
            ExternalChannel result = new ExternalChannel();
            result.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            result.Name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
            result.GroupId = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_ID");
            result.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(dr, "EXTERNAL_IDENTIFIER");
            result.FilterExpression = ODBCWrapper.Utils.GetSafeStr(dr, "FILTER_EXPRESSION");
            result.RecommendationEngineId = ODBCWrapper.Utils.GetIntSafeVal(dr, "RECOMMENDATION_ENGINE_ID");
            int isActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active");
            result.IsActive = isActive == 1 ? true : false;
            int enrichmentsVal = ODBCWrapper.Utils.GetIntSafeVal(dr, "ENRICHMENTS");
            result.Enrichments = SetEnrichments(enrichmentsVal);
            return result;
        }

        public static List<ExternalRecommendationEngineEnrichment> GetAvailableEnrichments()
        {
            List<ExternalRecommendationEngineEnrichment> result = new List<ExternalRecommendationEngineEnrichment>();

            DataTable enrichmentsTable = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            try
            {
                // Get from DB all enrichments that are active
                selectQuery += "select * from external_channels_enrichments (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                selectQuery.SetCachedSec(1200);

                enrichmentsTable = selectQuery.Execute("query", true);

            }
            catch
            {
                enrichmentsTable = null;
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }

            if (enrichmentsTable != null && enrichmentsTable.Rows != null)
            {
                // Run on table and convert values to enums
                foreach (DataRow row in enrichmentsTable.Rows)
                {
                    int value = ODBCWrapper.Utils.ExtractInteger(row, "value");

                    if (value > 0)
                    {
                        result.Add((ExternalRecommendationEngineEnrichment)value);
                    }
                }
            }

            return result;
        }

        private static int GetEnrichments(List<ExternalRecommendationEngineEnrichment> list)
        {
            int enrichmentListValue = 0;
            foreach (ExternalRecommendationEngineEnrichment ExternalRecommendationEngineEnrichment in list)
            {
                enrichmentListValue += (int)ExternalRecommendationEngineEnrichment;
            }

            return enrichmentListValue;
        }

        private static List<ExternalRecommendationEngineEnrichment> SetEnrichments(int enrichmentListValue)
        {
            List<ExternalRecommendationEngineEnrichment> list = new List<ExternalRecommendationEngineEnrichment>();

            foreach (var value in Enum.GetValues(typeof(ExternalRecommendationEngineEnrichment)))
            {
                if (((int)value & enrichmentListValue) == (int)value)
                {
                    list.Add((ExternalRecommendationEngineEnrichment)value);
                }
            }

            return list;
        }

        public static void GetGroupDefaultParameters(int groupId, out bool isRegionalizationEnabled, out int defaultRegion, out int defaultRecommendationEngine,
                                                     out int RelatedRecommendationEngine, out int SearchRecommendationEngine,
                                                     out int RelatedRecommendationEngineEnrichments, out int SearchRecommendationEngineEnrichments)
        {
            isRegionalizationEnabled = false;
            defaultRegion = 0;
            defaultRecommendationEngine = 0;
            RelatedRecommendationEngine = 0;
            RelatedRecommendationEngineEnrichments = 0;
            SearchRecommendationEngine = 0;
            SearchRecommendationEngineEnrichments = 0;

            // Call stored procedure that checks if this group has regionalization or not
            ODBCWrapper.StoredProcedure storedProcedureDefaultRegion = new ODBCWrapper.StoredProcedure("Get_GroupDefaultParameters");
            storedProcedureDefaultRegion.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedureDefaultRegion.AddParameter("@GroupID", groupId);

            DataSet groupDataSet = storedProcedureDefaultRegion.ExecuteDataSet();

            if (groupDataSet != null && groupDataSet.Tables != null && groupDataSet.Tables.Count == 1)
            {
                DataTable groupTable = groupDataSet.Tables[0];

                if (groupTable != null && groupTable.Rows != null && groupTable.Rows.Count > 0 && groupTable.Rows[0] != null)
                {
                    DataRow groupRow = groupTable.Rows[0];

                    isRegionalizationEnabled = ODBCWrapper.Utils.ExtractBoolean(groupRow, "is_regionalization_enabled");
                    defaultRegion = ODBCWrapper.Utils.ExtractInteger(groupRow, "default_region");
                    defaultRecommendationEngine = ODBCWrapper.Utils.ExtractInteger(groupRow, "SELECTED_RECOMMENDATION_ENGINE");
                    RelatedRecommendationEngine = ODBCWrapper.Utils.ExtractInteger(groupRow, "RELATED_RECOMMENDATION_ENGINE");
                    RelatedRecommendationEngineEnrichments = ODBCWrapper.Utils.ExtractInteger(groupRow, "RELATED_RECOMMENDATION_ENGINE_ENRICHMENTS");
                    SearchRecommendationEngine = ODBCWrapper.Utils.ExtractInteger(groupRow, "SEARCH_RECOMMENDATION_ENGINE");
                    SearchRecommendationEngineEnrichments = ODBCWrapper.Utils.ExtractInteger(groupRow, "SEARCH_RECOMMENDATION_ENGINE_ENRICHMENTS");
                }
            }
        }

        /// <summary>
        /// Performs an update query that deletes a given KSQL channel
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public static bool DeleteKSQLChannel(int groupID, int channelId)
        {
            bool result = false;

            try
            {
                UpdateQuery updateQuery = new UpdateQuery("CHANNELS");

                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", 2);
                updateQuery += " WHERE ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", channelId);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);

                result = updateQuery.Execute();
                updateQuery.Finish();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed deleting KSQL Channel {0}", channelId, ex);
            }

            return result;
        }

        public static List<KSQLChannel> GetKSQLChannels(int groupID)
        {
            throw new NotImplementedException();
        }

        public static KSQLChannel GetKSQLChannelById(int groupID, int channelId, Dictionary<string, string> metas)
        {
            KSQLChannel result = null;

            var dataSet = GetChannelDetails(new List<int>() { channelId }, true);

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 1)
            {
                DataTable channelsTable = dataSet.Tables[0];
                DataTable assetTypesTable = dataSet.Tables[1];

                // If there is any row returned
                if (channelsTable != null && channelsTable.Rows != null && channelsTable.Rows.Count > 0)
                {
                    result = CreateKSQLChannelByDataRow(assetTypesTable, channelsTable.Rows[0], metas);
                }
            }

            return result;
        }

        private static KSQLChannel CreateKSQLChannelByDataRow(DataTable assetTypesTable, DataRow rowData, Dictionary<string, string> metas = null)
        {
            KSQLChannel channel = new KSQLChannel();
            channel.AssetTypes = new List<int>();

            channel.ID = ODBCWrapper.Utils.GetIntSafeVal(rowData["Id"]);

            int channelGroupId = ODBCWrapper.Utils.GetIntSafeVal(rowData["group_id"]);
            int isActive = ODBCWrapper.Utils.GetIntSafeVal(rowData["is_active"]);
            int status = ODBCWrapper.Utils.GetIntSafeVal(rowData["status"]);
            int channelType = ODBCWrapper.Utils.GetIntSafeVal(rowData["channel_type"]);

            // If the channel is in correct status
            if ((status == 1) && (channelType == 4))
            {
                channel.IsActive = isActive;
                channel.Status = status;
                channel.GroupID = channelGroupId;
                channel.Name = ODBCWrapper.Utils.ExtractString(rowData, "name");
                channel.Description = ODBCWrapper.Utils.ExtractString(rowData, "description");

                #region Asset Types

                if (assetTypesTable != null)
                {
                    List<DataRow> mediaTypes = assetTypesTable.Select("CHANNEL_ID = " + channel.ID).ToList();

                    foreach (DataRow drMediaType in mediaTypes)
                    {
                        channel.AssetTypes.Add(ODBCWrapper.Utils.GetIntSafeVal(drMediaType, "MEDIA_TYPE_ID"));
                    }
                }

                #endregion

                #region Order

                channel.Order = new OrderObj();

                int orderBy = ODBCWrapper.Utils.GetIntSafeVal(rowData["order_by_type"]);
                int orderDirection = ODBCWrapper.Utils.GetIntSafeVal(rowData["order_by_dir"]) - 1;

                // all META_STR/META_DOUBLE values
                if (orderBy >= 1 && orderBy <= 30)
                {
                    // get the specific value of the meta
                    int nMetaEnum = (orderBy);
                    string enumName = Enum.GetName(typeof(MetasEnum), nMetaEnum);

                    if (metas != null && metas.ContainsKey(enumName))
                    {
                        channel.Order.m_sOrderValue = metas[enumName];
                        channel.Order.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.META;
                    }
                }
                else
                {
                    channel.Order.m_eOrderBy =
                        (ApiObjects.SearchObjects.OrderBy)ApiObjects.SearchObjects.OrderBy.ToObject(typeof(ApiObjects.SearchObjects.OrderBy), orderBy);
                }

                channel.Order.m_eOrderDir = (OrderDir)orderDirection;

                #endregion

                channel.FilterQuery = ODBCWrapper.Utils.ExtractString(rowData, "KSQL_FILTER");

                BooleanPhraseNode node = null;
                var parseStatus = BooleanPhraseNode.ParseSearchExpression(channel.FilterQuery, ref node);

                if (parseStatus.Code != 0)
                {
                    log.WarnFormat("KSQL channel {0} has invalid KSQL expression: {1}", channel.ID, channel.FilterQuery);
                }
            }
            else
            {
                channel = null;
            }

            return channel;
        }

        public static KSQLChannel InsertKSQLChannel(int groupID, KSQLChannel channel, Dictionary<string, string> metas)
        {
            KSQLChannel result = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_KSQLChannel");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupID);
            sp.AddParameter("@name", channel.Name);
            sp.AddParameter("@isActive", channel.IsActive);
            sp.AddParameter("@status", 1);
            sp.AddParameter("@description", channel.Description);
            sp.AddParameter("@orderBy", (int)channel.Order.m_eOrderBy);
            sp.AddParameter("@orderDirection", (int)channel.Order.m_eOrderDir + 1);
            sp.AddParameter("@Filter", channel.FilterQuery);
            sp.AddIDListParameter<int>("@AssetTypes", channel.AssetTypes, "Id");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                DataTable assetTypes = null;

                if (ds.Tables.Count > 1)
                {
                    assetTypes = ds.Tables[1];
                }

                result = CreateKSQLChannelByDataRow(assetTypes, ds.Tables[0].Rows[0], metas);
            }

            return result;
        }

        public static KSQLChannel UpdateKSQLChannel(int groupID, KSQLChannel channel, Dictionary<string, string> metas)
        {
            KSQLChannel result = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_KSQLChannel");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@channelId", channel.ID);
            sp.AddParameter("@groupId", groupID);
            sp.AddParameter("@name", channel.Name);
            sp.AddParameter("@isActive", channel.IsActive);
            sp.AddParameter("@description", channel.Description);
            sp.AddParameter("@Filter", channel.FilterQuery);
            sp.AddParameter("@orderBy", (int)channel.Order.m_eOrderBy);
            sp.AddParameter("@orderDirection", (int)channel.Order.m_eOrderDir + 1);
            sp.AddIDListParameter<int>("@AssetTypes", channel.AssetTypes, "Id");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                DataTable assetTypes = null;

                if (ds.Tables.Count > 1)
                {
                    assetTypes = ds.Tables[1];
                }

                result = CreateKSQLChannelByDataRow(assetTypes, ds.Tables[0].Rows[0], metas);
            }

            return result;

        }

        public static DataRowCollection GetPicsTableData(int assetId, eAssetImageType assetImageType, int? ratioId = null, int? extraStatus = null)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PicsData");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@AssetId", assetId);
                sp.AddParameter("@AssetImageType", (int)assetImageType);
                sp.AddParameter("@RatioId", ratioId);
                sp.AddParameter("@ExtraStatus", extraStatus);

                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    return ds.Tables[0].Rows;
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to get pics data", ex);
            }
            return null;
        }

        public static DataRowCollection GetGroupPicSizesTableData(int groupId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_MediaPicSizes");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupId);

                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    return ds.Tables[0].Rows;
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to get group picture sizes", ex);
            }
            return null;
        }

        public static int GetEpgPicsData(int groupId, string description)
        {
            int picId = 0;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EPGPicsData");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupId", groupId);
                sp.AddParameter("@Description", description);

                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    picId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to get pics data", ex);
            }
            return picId;
        }

        public static int InsertPic(int groupId, string name, string description, string baseUrl, int ratioId, int assetId, eAssetImageType assetImageType)
        {
            int picId = 0;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_Pic");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupId", groupId);
                sp.AddParameter("@Name", name);
                sp.AddParameter("@Description", description);
                sp.AddParameter("@BaseUrl", baseUrl);
                sp.AddParameter("@RatioId", ratioId);
                sp.AddParameter("@AssetId", assetId);
                sp.AddParameter("@AssetImageType", (int)assetImageType);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    picId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                }
            }
            catch (Exception ex)
            {
                log.DebugFormat("ERROR saving InsertPic ex {0} ", ex);
            }

            return picId;
        }

        public static int InsertEPGPic(int groupId, string name, string description, string baseUrl)
        {
            int picId = 0;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_EPGPic");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupId", groupId);
                sp.AddParameter("@Name", name);
                sp.AddParameter("@Description", description);
                sp.AddParameter("@BaseUrl", baseUrl);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    picId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                }
            }
            catch (Exception ex)
            {
                log.DebugFormat("ERROR saving InsertEPGPic ex {0} ", ex);
            }

            return picId;
        }

        public static List<Ratio> GetGroupRatios(int groupID)
        {
            List<Ratio> ratios = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_GroupPicsRatios");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);

                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    ratios = new List<Ratio>();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ratios.Add(new Ratio()
                        {
                            Id = Utils.GetIntSafeVal(row, "RATIO_ID"),
                            Name = Utils.GetSafeStr(row, "RATIO")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get group ratios. GID: {0}, ex: {1}", groupID, ex);
            }
            return ratios;
        }

        public static DataTable GetGroupsMediaType(string subGroups)
        {
            DataTable result = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            //selectQuery += "select " + sFieldName + " from " + sTable + " where ";
            selectQuery += "SELECT ID, MEDIA_TYPE_ID FROM groups_media_type WHERE GROUP_ID IN (" + subGroups + ") ";
            selectQuery += "AND IS_ACTIVE = 1 AND STATUS = 1";

            if (selectQuery.Execute("query", true) != null)
            {
                result = selectQuery.Table("query");
            }

            selectQuery.Finish();
            selectQuery = null;

            return result;
        }

        public static PlayCycleSession InsertPlayCycleSession(string siteGuid, int MediaFileID, int groupID, string UDID, int platform, int mediaConcurrencyRuleID, int domainID)
        {
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.SOCIAL);
            int limitRetries = RETRY_LIMIT;
            PlayCycleSession playCycleSession = null;
            Random sleepVal = new Random();

            // create new playCycleKey even when updating an existing document since its a new session
            string playCycleKey = Guid.NewGuid().ToString();

            try
            {
                string docKey = UtilsDal.GetPlayCycleKey(siteGuid, MediaFileID, groupID, UDID, platform);

                ulong version;
                playCycleSession = cbClient.GetWithVersion<PlayCycleSession>(docKey, out version);

                if (version != 0 && playCycleSession != null)
                {
                    playCycleSession.MediaConcurrencyRuleID = mediaConcurrencyRuleID;
                    playCycleSession.CreateDateMs = Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                    playCycleSession.PlayCycleKey = playCycleKey;
                    playCycleSession.DomainID = domainID;
                }
                else
                {
                    playCycleSession = new PlayCycleSession(mediaConcurrencyRuleID, playCycleKey, Utils.DateTimeToUnixTimestamp(DateTime.UtcNow), domainID);
                }

                int ttl = 0;
                bool shouldUseTtl = int.TryParse(CB_PLAYCYCLE_DOC_EXPIRY_MIN, out ttl);

                bool setResult = cbClient.SetWithVersionWithRetry<PlayCycleSession>(docKey, playCycleSession, version, limitRetries, 50, (uint)(ttl * 60));

                if (!setResult)
                {
                    playCycleSession = null;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed InsertPlayCycleSession, userId: {0}, groupID: {1}, UDID: {2}, platform: {3}, mediaConcurrencyRuleID: {4}, playCycleKey: {5}, mediaFileID: {6}, Exception: {7}",
                                 siteGuid, groupID, UDID, platform, mediaConcurrencyRuleID, playCycleKey, MediaFileID, ex.Message);
            }

            if (playCycleSession == null)
            {
                log.ErrorFormat("Error in InsertPlayCycleSession, playCycleSession is null. userId: {0}, groupID: {1}, UDID: {2}, platform: {3}, mediaConcurrencyRuleID: {4}, playCycleKey: {5}, mediaFileID: {6}",
                                 siteGuid, groupID, UDID, platform, mediaConcurrencyRuleID, playCycleKey, MediaFileID);
            }
            return playCycleSession;
        }

        public static PlayCycleSession GetUserPlayCycle(string siteGuid, int MediaFileID, int groupID, string UDID, int platform)
        {
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.SOCIAL);

            PlayCycleSession playCycleSession = null;
            try
            {
                string docKey = UtilsDal.GetPlayCycleKey(siteGuid, MediaFileID, groupID, UDID, platform);
                double ttl;
                bool shouldUseTtl = double.TryParse(CB_PLAYCYCLE_DOC_EXPIRY_MIN, out ttl);

                playCycleSession = cbClient.Get<PlayCycleSession>(docKey);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetUserPlayCycle, userId: {0}, groupID: {1}, UDID: {2}, platform: {3}, mediaFileID: {4}, Exception: {5}",
                                 siteGuid, groupID, UDID, platform, MediaFileID, ex.Message);
            }

            if (playCycleSession == null)
            {
                log.ErrorFormat("Error in GetUserPlayCycle, playCycleSession is null. userId: {0}, groupID: {1}, UDID: {2}, platform: {3}, mediaFileID: {4}",
                                 siteGuid, groupID, UDID, platform, MediaFileID);
            }

            return playCycleSession;
        }

        public static DataSet GetLinearChannelSettings(int groupId, List<int> epgChannelIDs)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_LinearChannelSettings");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupId", groupId);
                sp.AddIDListParameter<int>("@EpgChannelID", epgChannelIDs, "id");

                return sp.ExecuteDataSet();
            }
            catch
            {
                return null;
            }
        }

        public static string GetEPGChannelCDVRId(int groupId, string epgChannelId)
        {
            string cdvrId = string.Empty;

            object value =
                ODBCWrapper.Utils.GetTableSingleVal("epg_channels", "CDVR_ID", Convert.ToInt32(epgChannelId), 60, "MAIN_CONNECTION_STRING");

            if (value != DBNull.Value)
            {
                cdvrId = Convert.ToString(value);
            }

            return cdvrId;
        }

    }
}
