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

/// <summary>
/// Summary description for ActionHelper
/// </summary>
/// 
namespace TVPApi
{
    public class ActionHelper
    {

        //public static string GetSiteGuid(string userName, string password, int groupID, PlatformType platform)
        //{
        //    string retVal = string.Empty;
        //    UsersService userService = new TVPApiModule.users.UsersService();
        //    int regGroupID = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByAccountType(AccountType.Regular).BaseGroupID;
        //    UserResponseObject respObj = userService.CheckUserPassword(string.Format("users_{0}", regGroupID), "11111", userName, password, false);
        //    if (respObj != null && respObj.m_RespStatus == ResponseStatus.OK)
        //    {
        //        if (respObj.m_user != null)
        //        {
        //            retVal = respObj.m_user.m_sSiteGUID;
        //        }
        //    }

        //    userService
        //    return retVal;

        //}

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

        public static bool PerformAction(ActionType action, int mediaID, int mediaType, int groupID, PlatformType platform, string sUserID, int iDomainID, string sUDID, int extraVal)
        {
            bool retVal = false;
            string isOfflineSync = ConfigurationManager.AppSettings[string.Concat(groupID, "_OfflineFavoriteSync")];

            switch (action)
            {
                case ActionType.AddFavorite:
                    {
                        long guidNum = Convert.ToInt64(sUserID);
                        int regGroupID = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByAccountType(AccountType.Regular).BaseGroupID;
                        retVal = new ApiUsersService(groupID, platform).AddUserFavorite(sUserID, iDomainID, sUDID, mediaType.ToString(), mediaID.ToString(), string.Empty);

                        if (!string.IsNullOrEmpty(isOfflineSync))
                            new ApiUsersService(groupID, platform).AddUserOfflineMedia(sUserID, mediaID);

                        //retVal = FavoritesHelper.AddToFavorites(mediaType, mediaID.ToString(), guidNum);
                        break;
                    }
                case ActionType.RemoveFavorite:
                    {
                        new ApiUsersService(groupID, platform).RemoveUserFavorite(sUserID, new int[] { mediaID });
                        retVal = true;

                        if (!string.IsNullOrEmpty(isOfflineSync))
                            new ApiUsersService(groupID, platform).RemoveUserOfflineMedia(sUserID, mediaID);

                        //long guidNum = Convert.ToInt64(sUserID);
                        //int regGroupID = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByAccountType(AccountType.Regular).BaseGroupID;
                        //FavoritObject[] favoritesObj = new ApiUsersService(groupID, platform).GetUserFavorites(sUserID, string.Empty, iDomainID, string.Empty);
                        //if (favoritesObj != null)
                        //{
                        //    int[] favoriteID = new int[] { };
                        //    for (int i = 0; i < favoritesObj.Length; i++)
                        //    {
                        //        if (favoritesObj[i].m_sItemCode == mediaID.ToString())
                        //        {
                        //            favoriteID.SetValue(favoritesObj[i].m_nID, i);
                        //            break;
                        //        }
                        //    }
                        //    if (favoriteID.Length > 0)
                        //{
                        //    new ApiUsersService(groupID, platform).RemoveUserFavorite(sUserID, favoriteID);
                        //    retVal = true;
                        //}
                        //}
                        //userService.RemoveUserFavorit(string.Format("users_{0}", regGroupID.ToString()), "11111", sID,
                        //if (mediaID > 0)
                        //{
                        //    retVal = FavoritesHelper.RemoveFromFavorites(mediaID.ToString(), mediaType, guidNum);
                        //}
                        //else
                        //{
                        //    retVal = FavoritesHelper.RemoveAllUserFavorites(guidNum);
                        //}
                        break;
                    }
                case ActionType.Rate:
                    {
                        //TVPApiModule.tvapi.tvapi service = new tvapi();
                        ////service.Url = "http://localhost/TVApi/tvapi.asmx";
                        ////tvapiService2.Url = "http://localhost/TVMApi/tvapi.asmx";
                        //TVPApiModule.tvapi.InitializationObject initObj = new TVPApiModule.tvapi.InitializationObject();
                        //initObj.m_oPlayerIMRequestObject = new PlayerIMRequestObject();
                        //int favGroupID = WSUtils.GetGroupIDByMediaType(mediaType);
                        //initObj.m_oPlayerIMRequestObject.m_sPalyerID = string.Format("tvpapi_{0}", favGroupID.ToString());
                        //initObj.m_oPlayerIMRequestObject.m_sPlayerKey = "11111";
                        //initObj.m_oUserIMRequestObject = new UserIMRequestObject();
                        //initObj.m_oUserIMRequestObject.m_sSiteGuid = "11111";
                        //string apiWsUser = string.Format("api_{0}", favGroupID.ToString());
                        //RateResponseObject obj = service.TVAPI_RateMedia(apiWsUser, "11111", DateTime.UtcNow, initObj, mediaID, extraVal);
                        TVPPro.SiteManager.TvinciPlatform.api.RateMediaObject rro = new ApiApiService(groupID, platform).RateMedia(sUserID, mediaID, extraVal);
                        retVal = true;
                        break;
                        //long guidNum = Convert.ToInt64(sID);
                        //retVal = FavoritesHelper.AddToFavorites(mediaType, mediaID.ToString(), guidNum);
                        //break;
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

        public static string MediaMark(InitializationObject initObj, int groupID, PlatformType platform, action Action, int mediaType, long iMediaID, long iFileID, int iLocation)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            return new APIMediaMark(account.TVMUser, account.TVMPass)
            {
                GroupID = groupID,
                Platform = platform,
                Action = Action,
                MediaID = iMediaID,
                Location = iLocation,
                DeviceUDID = initObj.UDID,
                SiteGUID = initObj.SiteGuid
            }.Execute();
        }

        public static string MediaMark(InitializationObject initObj, int groupID, PlatformType platform, action Action, FileHolder fileParams, int iLocation)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            return new APIMediaMark(account.TVMUser, account.TVMPass)
            {
                GroupID = groupID,
                Platform = platform,
                Action = Action,
                MediaID = fileParams.mediaID,
                Location = iLocation,
                DeviceUDID = initObj.UDID,
                SiteGUID = initObj.SiteGuid,
                AvgBitRate = fileParams.avg_bit_rate_num,
                CurrentBitRate = fileParams.current_bit_rate_num,
                TotalBitRateNum = fileParams.total_bit_rate_num
            }.Execute();
        }

        public static string MediaHit(InitializationObject initObj, int groupID, PlatformType platform, int mediaType, long iMediaID, long iFileID, int iLocation)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByMediaType(mediaType);
            return new APIMediaHit(account.TVMUser, account.TVMPass) { GroupID = groupID, Platform = platform, FileID = iFileID, MediaID = iMediaID, Location = iLocation, DeviceUDID = initObj.UDID, SiteGUID = initObj.SiteGuid }.Execute();
        }

        public static string MediaHit(InitializationObject initObj, int groupID, PlatformType platform, long iMediaID, long iFileID, int iLocation)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
            return new APIMediaHit(account.TVMUser, account.TVMPass) { GroupID = groupID, Platform = platform, MediaID = iMediaID, Location = iLocation, DeviceUDID = initObj.UDID, SiteGUID = initObj.SiteGuid }.Execute();
        }

        public static string SendToFriend(InitializationObject initObj, int groupID, int mediaID, string senderName, string senderEmail, string toEmail, string msg)
        {
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, initObj.Platform).GetTVMAccountByAccountType(AccountType.Parent);
            return new APISendToFriendLoader(account.TVMUser, account.TVMPass, mediaID.ToString())
            {
                MediaID = mediaID.ToString(),
                SenderName = senderName,
                FriendEmail = toEmail,
                EmailFrom = senderEmail,
                AddedMessage = msg, 
                GroupID = groupID, 
                Platform = initObj.Platform
            }.Execute().response.type;
        }

        public ActionHelper()
        {

        }
    }
}
