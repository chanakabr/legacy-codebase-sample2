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

namespace Tvinci.Core.DAL
{
    public class CatalogDAL : BaseDal
    {
        private static readonly string CB_MEDIA_MARK_DESGIN = ODBCWrapper.Utils.GetTcmConfigValue("cb_media_mark_design");

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
        /// <param name="p_sSiteGuid"></param>
        /// <returns></returns>
        public static DateTime? Get_MediaUserLastWatch(int p_nMedia, string p_sSiteGuid)
        {
            DateTime? dt = null;

            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

            // get document of media mark
            object objDocument = cbManager.Get<object>(UtilsDal.getUserMediaMarkDocKey(p_sSiteGuid, p_nMedia));

            if (objDocument != null)
            {
                // Deserialize to known class - for comfortable access
                MediaMarkLog mediaMarkLog = JsonConvert.DeserializeObject<MediaMarkLog>(objDocument.ToString());

                dt = mediaMarkLog.LastMark.CreatedAt;
            }

            return dt;
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

        public static void UpdateOrInsert_UsersMediaMark(int nDomainID, int nSiteUserGuid, string sUDID, int nMediaID, int nGroupID, int nLoactionSec, int fileDuration, string action, int mediaTypeId)
        {
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            DateTime currentDate = DateTime.UtcNow;

            while (limitRetries >= 0)
            {
                string docKey = UtilsDal.getDomainMediaMarksDocKey(nDomainID);

                ulong version;
                var data = cbManager.GetWithVersion<string>(docKey, out version);
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

                DomainMediaMark mm = new DomainMediaMark();

                //Create new if doesn't exist
                if (data == null)
                {
                    mm.devices = new List<UserMediaMark>();
                    mm.devices.Add(dev);
                }
                else
                {
                    mm = JsonConvert.DeserializeObject<DomainMediaMark>(data);
                    UserMediaMark existdev = mm.devices.Where(x => x.UDID == sUDID).FirstOrDefault();

                    if (existdev != null)
                        mm.devices.Remove(existdev);

                    mm.devices.Add(dev);
                }
                bool res = cbManager.SetWithVersion(docKey, JsonConvert.SerializeObject(mm, Formatting.None), version);

                if (!res)
                {
                    Thread.Sleep(r.Next(50));
                    limitRetries--;
                }
                else
                    break;
            }

            //Now storing this by the mediaID
            limitRetries = RETRY_LIMIT;
            string mmKey = UtilsDal.getUserMediaMarkDocKey(nSiteUserGuid, nMediaID);
            while (limitRetries >= 0)
            {
                ulong version;
                var data = cbManager.GetWithVersion<string>(mmKey, out version);
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

                MediaMarkLog umm = new MediaMarkLog();

                if (data == null)
                {
                    umm.devices = new List<UserMediaMark>();
                    umm.devices.Add(dev);
                }
                else
                {
                    umm = JsonConvert.DeserializeObject<MediaMarkLog>(data);
                    UserMediaMark existdev = umm.devices.Where(x => x.UDID == sUDID).FirstOrDefault();

                    if (existdev != null)
                        umm.devices.Remove(existdev);

                    umm.devices.Add(dev);
                }

                //For quick last position access
                umm.LastMark = dev;

                bool res = cbManager.SetWithVersion(mmKey, JsonConvert.SerializeObject(umm, Formatting.None), version);

                if (!res)
                {
                    Thread.Sleep(r.Next(50));
                    limitRetries--;
                }
                else
                    break;
            }
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


        public static DataSet GetChannelDetails(List<int> nChannelId)
        {

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetChannelDetails");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@ChannelsID", nChannelId, "Id");
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
                        language = new LanguageObj() { ID = id, Name = name, Code = code, Direction = direction };
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

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null)
            {
                DataRow row = ds.Tables[0].Rows[0];
                result = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_TYPE_ID");
            }

            return result;
        }

        public static int GetLastPosition(int mediaID, int userID)
        {
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            string key = UtilsDal.getUserMediaMarkDocKey(userID, mediaID);
            var data = cbManager.Get<string>(key);
            if (data == null)
                return 0;
            var umm = JsonConvert.DeserializeObject<MediaMarkLog>(data);
            return umm.LastMark.Location;
        }

        public static List<UserMediaMark> GetDomainLastPositions(int nDomainID, int ttl, ePlayType ePlay = ePlayType.MEDIA)
        {
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

            string docKey = UtilsDal.getDomainMediaMarksDocKey(nDomainID);
            var data = cbManager.Get<string>(docKey);

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
                var marks = cbManager.GetWithVersion<string>(docKey, out version);

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

                bool res = cbManager.SetWithVersion(docKey, JsonConvert.SerializeObject(dm, Formatting.None), version);

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

        public static List<WatchHistory> GetUserWatchHistory(string siteGuid, List<int> assetTypes, List<int> excludedAssetTypes, eWatchStatus filterStatus, int numOfDays, OrderDir orderDir, int pageIndex, int pageSize, int finishedPercent, out int totalItems)
        {
            List<WatchHistory> usersWatchHistory = new List<WatchHistory>();
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            totalItems = 0;

            // build date filter
            long minFilterdate = Utils.DateTimeToUnixTimestamp(DateTime.UtcNow.AddDays(-numOfDays));
            long maxFilterDate = Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            try
            {
                // get views
                ViewManager viewManager = new ViewManager(CB_MEDIA_MARK_DESGIN, "users_watch_history")
                {
                    startKey = new object[] { long.Parse(siteGuid), minFilterdate },
                    endKey = new object[] { long.Parse(siteGuid), maxFilterDate },
                    staleState = CouchbaseManager.ViewStaleState.False,
                    asJson = true
                };

                List<WatchHistory> unFilteredresult = cbManager.View<WatchHistory>(viewManager);

                if (unFilteredresult != null && unFilteredresult.Count > 0)
                {
                    // validate media on DB
                    DataTable dt = CatalogDAL.GetActiveMedia(unFilteredresult.Where(x => x.AssetTypeId != (int)eAssetTypes.EPG &&
                                                                                         x.AssetTypeId != (int)eAssetTypes.NPVR)
                                                                                         .Select(x => int.Parse(x.AssetId)).ToList());

                    // get active media list and update updated date
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        List<int> activeMediaIds = new List<int>();
                        int mediaId;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            mediaId = Utils.GetIntSafeVal(dt.Rows[i], "ID");
                            activeMediaIds.Add(mediaId);

                            // get update date
                            unFilteredresult.First(x => int.Parse(x.AssetId) == mediaId &&
                                                        x.AssetTypeId != (int)eAssetTypes.EPG &&
                                                        x.AssetTypeId != (int)eAssetTypes.NPVR)
                                                        .UpdateDate = System.Convert.ToDateTime(dt.Rows[i]["UPDATE_DATE"].ToString());
                        }

                        // remove medias that are not active
                        unFilteredresult.RemoveAll(x => x.AssetTypeId != (int)eAssetTypes.EPG &&
                                                        x.AssetTypeId != (int)eAssetTypes.NPVR &&
                                                        !activeMediaIds.Contains(int.Parse(x.AssetId)));
                    }

                    // filter status 
                    switch (filterStatus)
                    {
                        case eWatchStatus.Progress:

                        // remove all finished
                        unFilteredresult.RemoveAll(x => (x.Duration != 0) && (((float)x.Location / (float)x.Duration * 100) >= finishedPercent));
                        unFilteredresult.ForEach(x => x.IsFinishedWatching = false);
                        break;

                        case eWatchStatus.Done:

                        // remove all in progress
                        unFilteredresult.RemoveAll(x => (x.Duration != 0) && (((float)x.Location / (float)x.Duration * 100) < finishedPercent));
                        unFilteredresult.ForEach(x => x.IsFinishedWatching = true);
                        break;

                        case eWatchStatus.All:

                        foreach (var item in unFilteredresult)
                        {
                            if ((item.Duration != 0) && (((float)item.Location / (float)item.Duration * 100) >= finishedPercent))
                                item.IsFinishedWatching = true;
                            else
                                item.IsFinishedWatching = false;
                        }
                        break;

                        default:
                        break;
                    }

                    // filter asset types
                    if (assetTypes != null && assetTypes.Count > 0)
                        unFilteredresult = unFilteredresult.Where(x => assetTypes.Contains(x.AssetTypeId)).ToList();

                    // filter excluded asset types
                    if (excludedAssetTypes != null && excludedAssetTypes.Count > 0)
                        unFilteredresult.RemoveAll(x => excludedAssetTypes.Contains(x.AssetTypeId));

                    // order list
                    switch (orderDir)
                    {
                        case OrderDir.ASC:

                        unFilteredresult = unFilteredresult.OrderBy(x => x.LastWatch).ToList();
                        break;

                        case OrderDir.DESC:
                        case OrderDir.NONE:
                        default:

                        unFilteredresult = unFilteredresult.OrderByDescending(x => x.LastWatch).ToList();
                        break;
                    }

                    // update total items
                    totalItems = unFilteredresult.Count;

                    // page index 
                    usersWatchHistory = unFilteredresult.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                // ASK IRA. NO LOG IN THIS DAMN CLASS !!!
                throw ex;
            }

            return usersWatchHistory;
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
                        keys = userList,
                        asJson = true
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

        public static void UpdateOrInsert_UsersNpvrMark(int nDomainID, int nSiteUserGuid, string sUDID, string sAssetID, int nGroupID, int nLoactionSec, int fileDuration, string action)
        {
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();

            DateTime currentDate = DateTime.UtcNow;

            while (limitRetries >= 0)
            {

                string docKey = UtilsDal.getDomainMediaMarksDocKey(nDomainID);

                ulong version;
                var data = cbManager.GetWithVersion<string>(docKey, out version);
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

                DomainMediaMark mm = new DomainMediaMark();

                //Create new if doesn't exist
                if (data == null)
                {
                    mm.devices = new List<UserMediaMark>();
                    mm.devices.Add(dev);
                }
                else
                {
                    mm = JsonConvert.DeserializeObject<DomainMediaMark>(data);
                    var existdev = mm.devices.Where(x => x.UDID == sUDID).FirstOrDefault();

                    if (existdev != null)
                        mm.devices.Remove(existdev);

                    mm.devices.Add(dev);
                }
                bool res = cbManager.SetWithVersion(docKey, JsonConvert.SerializeObject(mm, Formatting.None), version);

                if (!res)
                {
                    Thread.Sleep(r.Next(50));
                    limitRetries--;
                }
                else
                    break;
            }

            limitRetries = RETRY_LIMIT;
            string mmKey = UtilsDal.getUserNpvrMarkDocKey(nSiteUserGuid, sAssetID);
            while (limitRetries >= 0)
            {
                ulong version;
                var data = cbManager.GetWithVersion<string>(mmKey, out version);
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

                MediaMarkLog umm = new MediaMarkLog();

                if (data == null)
                {
                    umm.devices = new List<UserMediaMark>();
                    umm.devices.Add(dev);
                }
                else
                {
                    umm = JsonConvert.DeserializeObject<MediaMarkLog>(data);
                    var existdev = umm.devices.Where(x => x.UDID == sUDID).FirstOrDefault();

                    if (existdev != null)
                        umm.devices.Remove(existdev);

                    umm.devices.Add(dev);
                }

                //For quick last position access
                umm.LastMark = dev;

                bool res = cbManager.Set(mmKey, JsonConvert.SerializeObject(umm, Formatting.None));

                if (!res)
                {
                    Thread.Sleep(r.Next(50));
                    limitRetries--;
                }
                else
                    break;
            }
        }

        public static int GetLastPosition(string NpvrID, int userID)
        {
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
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
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            // get domain document 
            string key = UtilsDal.getDomainMediaMarksDocKey(domainID);
            var data = cbManager.Get<string>(key);
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

        public static bool GetMediaPlayData(int mediaID, int mediaFileID, ref int ownerGroupID, ref int cdnID, ref int qualityID, ref int formatID, ref int mediaTypeID, ref int billingTypeID)
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
        public static void GetMediaTypes(int groupId, out Dictionary<int, string> idToName, out Dictionary<string, int> nameToId, out Dictionary<int, int> parentMediaTypes)
        {
            idToName = new Dictionary<int, string>();
            nameToId = new Dictionary<string, int>();
            parentMediaTypes = new Dictionary<int, int>();

            DataTable mediaTypes = GetMediaTypesTable(groupId);

            if (mediaTypes != null)
            {
                foreach (DataRow mediaType in mediaTypes.Rows)
                {
                    int id = ODBCWrapper.Utils.ExtractInteger(mediaType, "ID");
                    string name = ODBCWrapper.Utils.ExtractString(mediaType, "NAME");
                    int parent = ODBCWrapper.Utils.ExtractInteger(mediaType, "PARENT_TYPE_ID");

                    idToName.Add(id, name);
                    nameToId.Add(name, id);
                    parentMediaTypes.Add(id, parent);
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

        public static Dictionary<int, List<EpgPicture>> GetGroupTreeMultiPicEpgUrl(int parentGroupID)
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

            }
            return ds;
        }
    }
}
