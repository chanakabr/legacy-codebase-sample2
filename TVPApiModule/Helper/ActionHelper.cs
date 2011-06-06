using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.Helper;
using TVPApiModule.tvapi;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.Users;

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

        public static bool PerformAction(ActionType action, int mediaID, int mediaType, int groupID, PlatformType platform, string sID, int extraVal)
        {
            bool retVal = false;
            
            switch (action)
            {
                case ActionType.AddFavorite:
                    {
                        long guidNum = Convert.ToInt64(sID);
                        int regGroupID = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByAccountType(AccountType.Regular).BaseGroupID;
                        new ApiUsersService(groupID, platform).AddUserFavorite(sID, platform.ToString(), mediaType.ToString(), mediaID.ToString(), string.Empty);
                        //retVal = FavoritesHelper.AddToFavorites(mediaType, mediaID.ToString(), guidNum);
                        break;
                    }
                case ActionType.RemoveFavorite:
                    {
                        long guidNum = Convert.ToInt64(sID);
                        int regGroupID = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByAccountType(AccountType.Regular).BaseGroupID;
                        FavoritObject[] favoritesObj = new ApiUsersService(groupID, platform).GetUserFavorites(sID, platform.ToString(), string.Empty);
                        if (favoritesObj != null)
                        {
                            int favoriteID = 0;
                            for (int i = 0; i < favoritesObj.Length; i++)
                            {
                                if (favoritesObj[i].m_sItemCode == mediaID.ToString())
                                {
                                    favoriteID = favoritesObj[i].m_nID;
                                    break;
                                }
                            }
                            if (favoriteID > 0)
                            {
                                new ApiUsersService(groupID, platform).RemoveUserFavorite(sID, favoriteID);
                                retVal = true;
                            }
                        }
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
                        TVPApiModule.tvapi.tvapi service = new tvapi();
                        //service.Url = "http://localhost/TVApi/tvapi.asmx";
                        //tvapiService2.Url = "http://localhost/TVMApi/tvapi.asmx";
                        TVPApiModule.tvapi.InitializationObject initObj = new TVPApiModule.tvapi.InitializationObject();
                        initObj.m_oPlayerIMRequestObject = new PlayerIMRequestObject();
                        int favGroupID = WSUtils.GetGroupIDByMediaType(mediaType);
                        initObj.m_oPlayerIMRequestObject.m_sPalyerID = string.Format("tvpapi_{0}", favGroupID.ToString());
                        initObj.m_oPlayerIMRequestObject.m_sPlayerKey = "11111";
                        initObj.m_oUserIMRequestObject = new UserIMRequestObject();
                        initObj.m_oUserIMRequestObject.m_sSiteGuid = "11111";
                        string apiWsUser = string.Format("api_{0}", favGroupID.ToString());
                        RateResponseObject obj = service.TVAPI_RateMedia(apiWsUser, "11111", DateTime.UtcNow, initObj, mediaID, extraVal);
                        retVal = true;
                        break;
                        //long guidNum = Convert.ToInt64(sID);
                        //retVal = FavoritesHelper.AddToFavorites(mediaType, mediaID.ToString(), guidNum);
                        //break;
                    }
                default:
                    break;
            }
            return retVal;
        }

        public ActionHelper()
        {

        }
    }
}
