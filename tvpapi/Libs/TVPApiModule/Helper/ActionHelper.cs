using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.Helper;
using TVPApiModule.tvapi;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.Users;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApiModule.DataLoaders;
using System.Configuration;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.TVMDataLoader.Protocols.SendToFriend;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

/// <summary>
/// Summary description for ActionHelper
/// </summary>
namespace TVPApi
{
    public class ActionHelper
    {
        public class FileHolder
        {
            public long fileID { get; set; }
            public long mediaID { get; set; }
            public int mediaType { get; set; }
            public string billingType { get; set; }
            public string duration { get; set; }
            public string original_file_format { get; set; }
            public string file_format { get; set; }
            public int avg_bit_rate_num { get; set; }
            public int current_bit_rate_num { get; set; }
            public int total_bit_rate_num { get; set; }
        }

        public static bool PerformAction(ActionType action, int mediaID, int mediaType, int groupID, PlatformType platform, string sUserID, int iDomainID, 
                                         string sUDID, int extraVal)
        {
            bool retVal = false;
            string isOfflineSync = ConfigurationManager.AppSettings[string.Concat(groupID, "_OfflineFavoriteSync")];

            switch (action)
            {
                case ActionType.AddFavorite:
                    {
                        long guidNum = 0;
                        if (Int64.TryParse(sUserID, out guidNum))
                        {
                            int regGroupID = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByAccountType(AccountType.Regular).BaseGroupID;

                            if (!string.IsNullOrEmpty(isOfflineSync))
                                new ApiUsersService(groupID, platform).AddUserOfflineMedia(sUserID, mediaID);

                            retVal = new ApiUsersService(groupID, platform).AddUserFavorite(sUserID, iDomainID, sUDID, mediaType.ToString(), mediaID.ToString(), extraVal.ToString());
                        }
                        break;
                    }
                case ActionType.RemoveFavorite:
                    {
                        new ApiUsersService(groupID, platform).RemoveUserFavorite(sUserID, new int[] { mediaID });
                        retVal = true;

                        if (!string.IsNullOrEmpty(isOfflineSync))
                            new ApiUsersService(groupID, platform).RemoveUserOfflineMedia(sUserID, mediaID);


                        break;
                    }
                case ActionType.Rate:
                    {

                        TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject rro = new ApiApiService(groupID, platform).RateMedia(sUserID, mediaID, extraVal);
                        retVal = rro.oStatus != null && rro.oStatus.m_nStatusCode == 0;
                        break;

                    }
                case ActionType.Vote:
                    {
                        string sRet = TVPApiModule.Helper.VotesHelper.UserVote(mediaID.ToString(), sUserID, platform, groupID);
                        retVal = sRet.Equals("Success");
                        break;
                    }
                default:
                    break;
            }
            return retVal;
        }
        
        public static string MediaMark(InitializationObject initObj, int groupId, PlatformType platform, action Action, int iLocation, string npvrID, long programId,
                                       long iMediaID, long iFileID, bool isReportingMode, eAssetTypes assetType = eAssetTypes.UNKNOWN, int avgBitRate = 0, int currentBitRate = 0,
                                       int totalBitRate = 0)
        {
            Tvinci.Data.Loaders.TvinciPlatform.Catalog.Status status =
                new TVPPro.SiteManager.CatalogLoaders.MediaMarkLoader(groupId, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, (int)iMediaID, (int)iFileID,
                                                                      npvrID, avgBitRate, currentBitRate, iLocation, totalBitRate, Action.ToString(), string.Empty,
                                                                      string.Empty, string.Empty, string.Empty, programId, isReportingMode, assetType)
                {
                    Platform = platform.ToString()
                }.Execute() as Tvinci.Data.Loaders.TvinciPlatform.Catalog.Status;

            switch (status.Code)
            {
                // ok
                case 0:
                    return "media_mark";

                // ConcurrencyLimitation
                case 4001:
                    return "Concurrent";

                // UserNotAllowed, InvalidAssetType, ProgramDoesntExist, ActionNotRecognized, InvalidAssetId
                case 1027:
                case 4021:
                case 4022:
                case 4023:
                case 4024:
                    return status.Message;

                default:
                    return "Error";
            }
        }

        public static string MediaMark(InitializationObject initObj, int groupId, PlatformType platform, action Action, int iLocation, string npvrID, FileHolder fileParams,
                                       bool isReportingMode, long programId = 0)
        {
            return new TVPPro.SiteManager.CatalogLoaders.MediaMarkLoader(groupId, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, (int)fileParams.mediaID,
                                                                         (int)fileParams.fileID, npvrID, fileParams.avg_bit_rate_num, fileParams.current_bit_rate_num,
                                                                         iLocation, fileParams.total_bit_rate_num, Action.ToString(), fileParams.duration, string.Empty,
                                                                         string.Empty, string.Empty, programId, isReportingMode)
            {
                Platform = platform.ToString()
            }.Execute() as string;
        }
        
        public static string MediaHit(InitializationObject initObj, int groupId, PlatformType platform, long iMediaID, long iFileID, int iLocation, string npvrID, 
                                      long programId, bool isReportingMode)
        {
            return new TVPPro.SiteManager.CatalogLoaders.MediaHitLoader(groupId, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, (int)iMediaID, (int)iFileID,
                                                                        npvrID, 0, 0, iLocation, 0, string.Empty, string.Empty, programId, isReportingMode)
            {
                Platform = platform.ToString()
            }.Execute() as string;
        }
        
        public static void MediaError(InitializationObject initObj, int groupID, PlatformType platform, FileHolder fileParams, int iLocation, string sErrorCode, 
                                      string sErrorMessage, string npvrID)
        {
            new TVPPro.SiteManager.CatalogLoaders.MediaMarkLoader(groupID, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, (int)fileParams.mediaID, 
                                                                  (int)fileParams.fileID, npvrID, fileParams.avg_bit_rate_num, fileParams.current_bit_rate_num, 
                                                                  iLocation, fileParams.total_bit_rate_num, string.Empty, fileParams.duration, string.Empty, string.Empty, 
                                                                  string.Empty, 0, false)
            {
                Platform = platform.ToString()
            }.Execute();
        }

        public static bool SendToFriend(InitializationObject initObj, int groupID, int mediaID, string senderName, string senderEmail, string toEmail, string msg)
        {
            return new ApiApiService(groupID, initObj.Platform).SendToFriend(senderName, senderEmail, toEmail, toEmail, mediaID);
        }        
    }
}
