using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ODBCWrapper;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using System.Data;
using TVPPro.SiteManager.Context;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.Helper
{
    public static class FavoritesHelper
    {
        #region Fields
        /// <summary>
        /// Holds the logger
        /// </summary>
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        #endregion

        #region Add to favorite
        public static bool AddToFavorites(TVPPro.SiteManager.Context.Enums.eFavoriteItemTypes ItemType, string ItemId)
        {
            return AddToFavorites((int)ItemType, ItemId);
        }

        public static bool AddToFavorites(int ItemType, string ItemId)
        {
            return AddToFavorites(ItemType, ItemId, UsersService.Instance.GetUserID());

        }

        public static bool AddToFavorites(int ItemType, string ItemId, string UserId)
        {
            return UsersService.Instance.AddToUserFavorite(ItemType.ToString(), ItemId, string.Empty, string.Empty);
        }
        #endregion Add to favorite

        #region Remove from favorites
        public static bool RemoveFromFavorites(string ItemId, Enums.eFavoriteItemTypes ItemType)
        {
            return RemoveFromFavorites(ItemId, (int)ItemType);
        }

        public static bool RemoveAllUserFavorites()
        {
            return RemoveAllUserFavorites(UsersService.Instance.GetUserID());
        }

        public static bool RemoveAllUserFavorites(string UserId)
        {
            var medias = UsersService.Instance.GetUserFavorite(string.Empty, UsersService.Instance.GetDomainID(), string.Empty);
            
            return UsersService.Instance.RemoveUserFavoriteItems(medias.Select(x=> int.Parse(x.m_sItemCode)).ToArray());
        }

        public static bool RemoveFromFavorites(string ItemId, int ItemType)
        {
            return RemoveFromFavorites(ItemId, ItemType, UsersService.Instance.GetUserID());
        }

        public static bool RemoveFromFavorites(string ItemId, int ItemType, string UserId)
        {
            int iMediaID;
            int.TryParse(ItemId, out iMediaID);
            return UsersService.Instance.RemoveUserFavorite(iMediaID);

            //if (!string.IsNullOrEmpty(UserId))
            //{
            //    UpdateQuery query = new UpdateQuery("UserFavorite");
            //    query += ODBCWrapper.Parameter.NEW_PARAM("Status", "=", "0");
            //    query += string.Format(" where ItemIdentifier in ({0}) and UserIdentifier in ({1}) and ContentTypeID in ({2})", ItemId, UserId, ItemType);

            //    if (!query.Execute())
            //    {
            //        return false;
            //    }

            //    query.Finish();
            //    query = null;

            //    return true;
            //}
            //else
            //{
            //    logger.ErrorFormat("User didnt pass authentication");
            //    return false;
            //}
        }
        #endregion Remove from favorites

        #region Get user favorites
        public static string[] GetUserFavoriteMedias(Enums.eFavoriteItemTypes ItemType)
        {
            return GetUserFavoriteMedias((int)ItemType);
        }

        public static string[] GetUserFavoriteMedias(int ItemType)
        {
            return GetUserFavoriteMedias(ItemType, UsersService.Instance.GetUserID(), false);
        }

        public static string[] GetUserFavoriteMedias(int ItemType, string userID, bool anonymous)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.FavoritObject[] oFavorites = UsersService.Instance.GetUserFavorite(ItemType != 0 ? ItemType.ToString() : string.Empty,
                UsersService.Instance.GetDomainID(), string.Empty);

            string[] sMediaIDs = new string[] { };

            if (oFavorites != null && oFavorites.Length > 0)
            {
                sMediaIDs = oFavorites.Select(i => i.m_sItemCode).ToArray();
            }

            return sMediaIDs;
        }
        #endregion Get user favorites


        public static bool ItemExistOnFavorite(string mediaID, string UserId)
        {
            //long guidNum = Convert.ToInt64(sID);
            bool retVal = false;
            TVPPro.SiteManager.TvinciPlatform.Users.FavoritObject[] favoriteObj = UsersService.Instance.GetUserFavorite(UsersService.Instance.GetUserID(), UsersService.Instance.GetDomainID(), string.Empty);
            if (favoriteObj != null)
            {
                for (int i = 0; i < favoriteObj.Length; i++)
                {
                    if (favoriteObj[i].m_sItemCode == mediaID.ToString())
                    {
                        retVal = true;
                        break;
                    }
                }
            }
            return retVal;
        }

        #region check if Item is already on favorite


        private static bool ItemExistOnFavorite(string ItemId)
        {
            return ItemExistOnFavorite(ItemId, UsersService.Instance.GetUserID());
        }

        #endregion check if Item is already on favorite
    }
}
