using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using ODBCWrapper;

namespace Tvinci.Core.DAL
{
    public class CatalogDAL : BaseDal
    {
     
        public static DataSet Get_MediaDetails(int nGroupID, int nMediaID, string sSiteGuid, bool bOnlyActiveMedia, int nLanguage, string sEndDate, bool bUseStartDate)
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

            DataSet ds = spGet_MediaDetails.ExecuteDataSet();

            return ds;
        }

        public static DataSet Build_MediaRelated(int nGroupID, int nMediaID, int nLanguage)
        {
            ODBCWrapper.StoredProcedure spBuild_MediaRelated = new ODBCWrapper.StoredProcedure("Build_MediaRelated");
            spBuild_MediaRelated.SetConnectionKey("MAIN_CONNECTION_STRING");
            spBuild_MediaRelated.AddParameter("@MediaID", nMediaID);
            spBuild_MediaRelated.AddParameter("@GroupID", nGroupID);
            spBuild_MediaRelated.AddParameter("@Language", nLanguage);

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

        public static DataSet Get_ChannelsListByCategory(int nCategoryID, int nGroupID, int nLanguage)
        {
            ODBCWrapper.StoredProcedure spCategoryChannels = new ODBCWrapper.StoredProcedure("Get_ChannelsListByCategory");
            spCategoryChannels.SetConnectionKey("MAIN_CONNECTION_STRING");
            spCategoryChannels.AddParameter("@CategoryID", nCategoryID);
            spCategoryChannels.AddParameter("@GroupID", nGroupID);
            spCategoryChannels.AddParameter("@LanguageID", nLanguage);

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

        public static DataTable Get_PersonalLastWatched( int nGroupID, string sSiteGuid)
        {
            ODBCWrapper.StoredProcedure spPersonalLastWatched = new ODBCWrapper.StoredProcedure("Get_PersonalLastWatched");
            spPersonalLastWatched.SetConnectionKey("MAIN_CONNECTION_STRING");
            spPersonalLastWatched.AddParameter("@GroupID", nGroupID);
            spPersonalLastWatched.AddParameter("@SiteGuid", sSiteGuid);

            DataSet ds = spPersonalLastWatched.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
        }
        
        public static DataTable Get_PersonalLasDevice(List<int> nMediaIDs,int nGroupID, string sSiteGuid)
        {
            ODBCWrapper.StoredProcedure spPersonalLastDevice = new ODBCWrapper.StoredProcedure("Get_PersonalLasDevice");
            spPersonalLastDevice.SetConnectionKey("MAIN_CONNECTION_STRING");
            spPersonalLastDevice.AddParameter("@GroupID", nGroupID);
            spPersonalLastDevice.AddParameter("@SiteGuid", sSiteGuid);
            spPersonalLastDevice.AddIDListParameter("@MediaID", nMediaIDs, "Id");

            DataSet ds = spPersonalLastDevice.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];
            return null;
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

        public static DataTable Get_PersonalRecommended(int nGroupID, string sSiteGuid, int Top)
        {
            ODBCWrapper.StoredProcedure spPersonalRecommended = new ODBCWrapper.StoredProcedure("Get_PersonalRecommended");
            spPersonalRecommended.SetConnectionKey("MAIN_CONNECTION_STRING");

            spPersonalRecommended.AddParameter("@GroupID", nGroupID);
            spPersonalRecommended.AddParameter("@SiteGuid", sSiteGuid);
            spPersonalRecommended.AddParameter("@Top", Top);

            DataSet ds = spPersonalRecommended.ExecuteDataSet();
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
            ODBCWrapper.StoredProcedure spPWWAWProtocol = new ODBCWrapper.StoredProcedure("Get_PWWAWProtocol");
            spPWWAWProtocol.SetConnectionKey("MAIN_US_CONNECTION_STRING");

            spPWWAWProtocol.AddParameter("@MediaID", nMediaID);
            spPWWAWProtocol.AddParameter("@GroupID", nGroupID);
            spPWWAWProtocol.AddParameter("@Language", nLanguage);
            spPWWAWProtocol.AddParameter("@CountryID", nCountryID);
            spPWWAWProtocol.AddParameter("@EndDateField", sEndDate);
            spPWWAWProtocol.AddParameter("@DeviceID", nDeviceId);
            spPWWAWProtocol.AddParameter("@SiteGuid", sSiteGuid);

            DataSet ds = spPWWAWProtocol.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_ChannelsBySubscription(int nGroupID, int nSubscriptionID)
        {
            ODBCWrapper.StoredProcedure spUserSocial = new ODBCWrapper.StoredProcedure("Get_ChannelsBySubscription");            
            spUserSocial.SetConnectionKey("MAIN_US_CONNECTION_STRING");

            spUserSocial.AddParameter("@GroupID", nGroupID);
            spUserSocial.AddParameter("@SubscriptionID", nSubscriptionID);

            DataSet ds = spUserSocial.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_PicProtocol(int nGroupID, List<int> nPicIDs)
        {
            ODBCWrapper.StoredProcedure spPicProtocol = new ODBCWrapper.StoredProcedure("Get_PicProtocol");
            spPicProtocol.SetConnectionKey("MAIN_CONNECTION_STRING");

            spPicProtocol.AddIDListParameter("@PicIdList", nPicIDs, "Id");
            spPicProtocol.AddParameter("@GroupID", nGroupID);            

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
            spGetLastPlayCycleKey.SetConnectionKey("MAIN_US_CONNECTION_STRING");
            
            spGetLastPlayCycleKey.AddParameter("@SiteGuid", sSiteGUID);
            spGetLastPlayCycleKey.AddParameter("@MediaID", nMediaID);
            spGetLastPlayCycleKey.AddParameter("@MediaFileID", nMediaFileID);
            spGetLastPlayCycleKey.AddParameter("@DeviceUDID", sUDID);
            spGetLastPlayCycleKey.AddParameter("@Platform", nPlatform);
            
            DataSet ds = spGetLastPlayCycleKey.ExecuteDataSet();

            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                result = ODBCWrapper.Utils.GetSafeStr( dt.Rows[0], "play_cycle_key"); 
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
                                              int nFirstPlayCounter, int nPlayCounter, int nLoadCounter, int nPauseCounter, int nStopCounter, int nFullScreenCounter,int nExitFullScreenCounter, int nSendToFriendCounter,
                                              int nPlayTimeCounter, int nFileQualityID, int nFileFormatID, DateTime dStartHourDate, int nUpdaterID, int nBrowser, int nPlatform, string sSiteGuid, string sDeviceUdID, string sPlayCycleID,int nSwooshCounter                                              
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

        public static void Insert_NewPlayCycleKey(int nGroupID, int nMediaID, int nMediaFileID, string sSiteGuid, int nPlatform, string sUDID, int nCountryID, string sPlayCycleKey)
        {
            ODBCWrapper.StoredProcedure spNewCycleKey = new ODBCWrapper.StoredProcedure("Insert_NewPlayCycleKey");
            spNewCycleKey.SetConnectionKey("MAIN_CONNECTION_STRING");

            spNewCycleKey.AddParameter("@GroupID", nGroupID);
            spNewCycleKey.AddParameter("@MediaID", nMediaID);
            spNewCycleKey.AddParameter("@MediaFileID", nMediaFileID);
            spNewCycleKey.AddParameter("@SiteGuid", sSiteGuid);
            spNewCycleKey.AddParameter("@Platform", nPlatform);
            spNewCycleKey.AddParameter("@DeviceUDID", sUDID);
            spNewCycleKey.AddParameter("@CountryID", nCountryID);
            spNewCycleKey.AddParameter("@PlayCycleKey", sPlayCycleKey); 

            spNewCycleKey.ExecuteNonQuery();
        }

        public static void Insert_NewMediaFileVideoQuality(int nWatcherID, int nUserSiteGuid, string sSessionID, int nMediaID, int nMediaFileID, int nAvgMaxBitRate, int nBitRateIndex,
                                                           int nTotalBitRatesNum, int nLoactionSec, int nBrowser, int nPlatform, int nCountryID, int nStatus, int nGroupID)   
        {
            ODBCWrapper.StoredProcedure spInsertNewMediaFileVideoQuality = new ODBCWrapper.StoredProcedure("Insert_NewMediaFileVideoQuality");
            spInsertNewMediaFileVideoQuality.SetConnectionKey("MAIN_CONNECTION_STRING");

            spInsertNewMediaFileVideoQuality.AddParameter("@WatcherID", nWatcherID); 
            spInsertNewMediaFileVideoQuality.AddParameter("@UserSiteGuid", nUserSiteGuid);
            spInsertNewMediaFileVideoQuality.AddParameter("@SessionID", sSessionID);
            spInsertNewMediaFileVideoQuality.AddParameter("@MediaID" , nMediaID);
            spInsertNewMediaFileVideoQuality.AddParameter("@MediaFileID" , nMediaFileID);
            spInsertNewMediaFileVideoQuality.AddParameter("@AvgMaxBitRate", nAvgMaxBitRate); 
            spInsertNewMediaFileVideoQuality.AddParameter("@BitRateIndex", nBitRateIndex);
            spInsertNewMediaFileVideoQuality.AddParameter("@TotalBitRatesNum", nTotalBitRatesNum);
            spInsertNewMediaFileVideoQuality.AddParameter("@LocationSec", nLoactionSec);
            spInsertNewMediaFileVideoQuality.AddParameter("@Browser", nBrowser);
            spInsertNewMediaFileVideoQuality.AddParameter("@Platform" , nPlatform);
            spInsertNewMediaFileVideoQuality.AddParameter("@CountryID", nCountryID);
            spInsertNewMediaFileVideoQuality.AddParameter("@Status", nStatus);
            spInsertNewMediaFileVideoQuality.AddParameter("@GroupID", nGroupID);

            spInsertNewMediaFileVideoQuality.ExecuteNonQuery();
        }

        public static void Insert_NewPlayerErrorMessage(int nGroupID, int nMediaID, int nMediaFileID,int nLoactionSec, int nPlatform, int nSiteUserGuid, string sUDID, string sErrorCode, string sErrorMessage)                                                  
        {
            ODBCWrapper.StoredProcedure spInsertNewPlayerError = new ODBCWrapper.StoredProcedure("Insert_NewPlayerError");
            spInsertNewPlayerError.SetConnectionKey("MAIN_CONNECTION_STRING");

            spInsertNewPlayerError.AddParameter("@GroupID", nGroupID);
            spInsertNewPlayerError.AddParameter("@MediaID ", nMediaID );
            spInsertNewPlayerError.AddParameter("@MediaFileID", nMediaFileID);
            spInsertNewPlayerError.AddParameter("@PlayTimeCounter", nLoactionSec);
            spInsertNewPlayerError.AddParameter("@Platform", nPlatform);
            spInsertNewPlayerError.AddParameter("@SiteGuid", nSiteUserGuid );
            spInsertNewPlayerError.AddParameter("@DeviceUdID", sUDID );
            spInsertNewPlayerError.AddParameter("@ErrorCode", sErrorCode);
            spInsertNewPlayerError.AddParameter("@ErrorMessage", sErrorMessage); 

            spInsertNewPlayerError.ExecuteNonQuery();
        }

        public static void UpdateOrInsert_UsersMediaMark(int nID, int nSiteUserGuid, string sUDID, int nMediaID, int nGroupID, int nLoactionSec, int nUpdateOrInsert)
        {
            ODBCWrapper.StoredProcedure spUpdateOrInsertUsersMediaMark = new ODBCWrapper.StoredProcedure("UpdateOrInsert_UsersMediaMark");
            spUpdateOrInsertUsersMediaMark.SetConnectionKey("MAIN_CONNECTION_STRING");

            spUpdateOrInsertUsersMediaMark.AddParameter("@ID", nID);
            spUpdateOrInsertUsersMediaMark.AddParameter("@SiteUserGuid", nSiteUserGuid);
            spUpdateOrInsertUsersMediaMark.AddParameter("@DeviceUDID", sUDID);
            spUpdateOrInsertUsersMediaMark.AddParameter("@MediaID", nMediaID);
            spUpdateOrInsertUsersMediaMark.AddParameter("@GroupID", nGroupID);
            spUpdateOrInsertUsersMediaMark.AddParameter("@LocationSec", nLoactionSec);
            spUpdateOrInsertUsersMediaMark.AddParameter("@UpdateOrInsert", nUpdateOrInsert);

            spUpdateOrInsertUsersMediaMark.ExecuteNonQuery();
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

        public static DataSet Get_MetasByGroup(int groupID)
        {
            ODBCWrapper.StoredProcedure spMetas= new ODBCWrapper.StoredProcedure("Get_MetasByGroup");
            spMetas.SetConnectionKey("MAIN_CONNECTION_STRING");
            spMetas.AddParameter("@GroupId", groupID);

            DataSet ds = spMetas.ExecuteDataSet();

            if (ds != null)
                return ds;
            return null;
        }

        public static DataTable GetMappedMetasByGroupId(int nParentGroupId)
        {
            DataTable returnedDataTable = null;

            try
            {
                ODBCWrapper.StoredProcedure spGroupsMappedMetaColumns = new ODBCWrapper.StoredProcedure("Get_MetasByGroup");
                spGroupsMappedMetaColumns.SetConnectionKey("MAIN_CONNECTION_STRING");
                spGroupsMappedMetaColumns.AddParameter("@GroupId", nParentGroupId);
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


        public static DataTable GetChannelByChannelId(int nChannelId)
        {
            DataTable returnedDataTable = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            try
            {
                //selectQuery += "select c.*, g.parent_group_id from channels c, groups g (nolock) where ";
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

        public static DataTable GetPermittedWatchRulesByGroupId(int nGroupId)
        {
            DataTable permittedWatchRules = null;

            try
            {
                ODBCWrapper.StoredProcedure spPermittedWatchRulesID = new ODBCWrapper.StoredProcedure("Get_PermittedWatchRulesID");
                spPermittedWatchRulesID.SetConnectionKey("MAIN_CONNECTION_STRING");
                spPermittedWatchRulesID.AddParameter("@GroupID", nGroupId);
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
                dt =  ds.Tables[0];

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

        public static bool InsertEpgComment(int nEpgProgramID, int nLanguage, string sWriter,  int nGroupID, string sCommentIp,
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

        public static DataSet Get_EPGCommentsList(int nEpgProgramID, int nGroupID, int nLanguage)
        {
            ODBCWrapper.StoredProcedure spPersonalRecommended = new ODBCWrapper.StoredProcedure("Get_EpgCommentsList");
            spPersonalRecommended.SetConnectionKey("MAIN_CONNECTION_STRING");

            spPersonalRecommended.AddParameter("@GroupID", nGroupID);
            spPersonalRecommended.AddParameter("@EpgProgramID", nEpgProgramID);
            spPersonalRecommended.AddParameter("@LangID", nLanguage);

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

            sp.AddParameter("@groupID",  groupID);
            sp.AddParameter("@mediaID",  mediaID);
            sp.AddParameter("@SiteGUID", siteGUID);
            sp.AddParameter("@PCFlag",   PCFlag);
            sp.AddParameter("@sUDID",    sUDID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                dt = ds.Tables[0];

            return dt;
        }

        public static DataTable Get_GroupChannels(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spPersonalRecommended = new ODBCWrapper.StoredProcedure("Get_GroupChannels");
            spPersonalRecommended.SetConnectionKey("MAIN_CONNECTION_STRING");

            spPersonalRecommended.AddParameter("@GroupID", nGroupID);
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


        public static DataSet Get_GroupMedias(int m_nGroupID, int nMediaID)
        {
            ODBCWrapper.StoredProcedure GroupMedias = new ODBCWrapper.StoredProcedure("Get_GroupMedias");
            GroupMedias.SetConnectionKey("MAIN_CONNECTION_STRING");

            GroupMedias.AddParameter("@GroupID", m_nGroupID);
            GroupMedias.AddParameter("@MediaID", nMediaID);

            DataSet ds = GroupMedias.ExecuteDataSet();

            return ds;
        }

        public static DataSet GetMediasStats(int nGroupID, List<int> mediaIDs, DateTime? dStartDate, DateTime? dEndDate)
        {
            ODBCWrapper.StoredProcedure MediaStats = new ODBCWrapper.StoredProcedure("Get_MediasStats");
            MediaStats.SetConnectionKey("MAIN_CONNECTION_STRING");

            MediaStats.AddParameter("@groupID", nGroupID);
            MediaStats.AddParameter("@startDate", dStartDate);
            MediaStats.AddParameter("@endDate", dEndDate);
            MediaStats.AddIDListParameter("@MediasIDs", mediaIDs, "Id");

            DataSet ds = MediaStats.ExecuteDataSet();
            return ds;
        }


        public static DataSet GetEpgStats(int nGroupID, List<int> epgIDs, DateTime? dStartDate, DateTime? dEndDate)
        {
            ODBCWrapper.StoredProcedure MediaStats = new ODBCWrapper.StoredProcedure("Get_EpgStats");
            MediaStats.SetConnectionKey("MAIN_CONNECTION_STRING");

            MediaStats.AddParameter("@groupID", nGroupID);
            MediaStats.AddParameter("@startDate", dStartDate);
            MediaStats.AddParameter("@endDate", dEndDate);
            MediaStats.AddIDListParameter("@EpgIDs", epgIDs, "Id");

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
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_IPWWAWProtocol");
            sp.SetConnectionKey("MAIN_US_CONNECTION_STRING");

            sp.AddParameter("@MediaID", nMediaID);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@Language", nLanguage);
            sp.AddParameter("@CountryID", nCountryID);
            sp.AddParameter("@EndDateField", sEndDate);
            sp.AddParameter("@DeviceID", nDeviceId);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@OperatorID", nOperatorID);

            DataSet ds = sp.ExecuteDataSet();

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

        public static DataTable Get_IPersonalRecommended(int nGroupID, string sSiteGuid, int nTop, int nOperatorID)
        {
            StoredProcedure sp = new StoredProcedure("Get_IPersonalRecommended");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@Top", nTop);
            sp.AddParameter("@OperatorID", nOperatorID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
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
    }
}
