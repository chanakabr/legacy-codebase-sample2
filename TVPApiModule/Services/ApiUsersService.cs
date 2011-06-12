using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Services;
using TVPApi;
using TVPPro.SiteManager.Context;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.Helper;

namespace TVPApiModule.Services
{
    public class ApiUsersService
    {
        #region Variables
        private readonly ILog logger = LogManager.GetLogger(typeof(ApiUsersService));
        private TVPPro.SiteManager.TvinciPlatform.Users.UsersService m_Module;

        private string m_wsUserName;
        private string m_wsPassword;

        private int m_groupID;
        private PlatformType m_platform;
        #endregion

        #region C'tor
        public ApiUsersService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Users.UsersService();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }
        #endregion C'tor

        #region Public methods
        public string SignIn(string sUserName, string sPassword)
        {
            string sGuid = string.Empty;
            try
            {
                UserResponseObject response = m_Module.CheckUserPassword(m_wsUserName, m_wsPassword, sUserName, sPassword, true);

                if (response != null && response.m_user != null)
                {
                    sGuid = response.m_user.m_sSiteGUID;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SignIn, Error Message: {0}, Parameters :  Username: {1}, Password, {2}", ex.Message, sUserName, sPassword);
            }

            return sGuid;
        }

        public UserResponseObject SignUp(UserBasicData userBasicData, UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            UserResponseObject response = null;
            try
            {
                response = m_Module.AddNewUser(m_wsUserName, m_wsPassword, userBasicData, userDynamicData, sPassword, sAffiliateCode);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SignUp, Error Message: {0}, Parameters :  Username: {1}, Password, {2}", ex.Message, userBasicData.m_sUserName, sPassword);
            }

            return response;
        }

        public bool IsUserLoggedIn(string sUsername)
        {
            bool bRet = false;
            try
            {
                /* TODO: complete */
                UserResponseObject response = m_Module.GetUserByUsername(m_wsUserName, m_wsPassword, sUsername);
                if (response.m_RespStatus == ResponseStatus.UserAllreadyLoggedIn)
                    bRet = true;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : IsUserLoggedIn, Error Message: {0}, Parameters :  Username: {1}", ex.Message, sUsername);
            }

            return bRet;
        }

        public bool RemoveUserFavorite(int iFavoriteID)
        {
            bool IsRemoved = false;

            try
            {
                m_Module.RemoveUserFavorit(m_wsUserName, m_wsPassword, SiteHelper.GetClientIP(), iFavoriteID);
                IsRemoved = true;                
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol AddUserFavorit, Error Message: {0} Parameters :ws User name : {1} , ws Password {2} ", ex.Message, m_wsUserName, m_wsPassword);
            }

            return IsRemoved;
        }

        public FavoritObject[] GetUserFavorites(string sSiteGuid, string sApplication, string sItemType)
        {
            FavoritObject[] response = null;

            try
            {
                response = m_Module.GetUserFavorites(m_wsUserName, m_wsPassword, sSiteGuid, sApplication, sItemType);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUserFavorites, Error Message: {0} Parameters : User {1}, Application: {2}", ex.Message, sSiteGuid, sApplication);
            }

            return response;
        }

        public void AddUserFavorite(string sSiteGuid, string sApplication, string sMediaType, string sMediaID, string sExtra)
        {
            try
            {
                m_Module.AddUserFavorit(m_wsUserName, m_wsPassword, sSiteGuid, sApplication, sMediaType, sMediaID, sExtra);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol AddUserFavorite, Error Message: {0} Parameters : User {1}, Application: {2}, Media: {3}", ex.Message, sSiteGuid, sApplication, sMediaID);
            }
        }

        public void RemoveUserFavorite(string sSiteGuid, int favoriteID)
        {
            try
            {
                m_Module.RemoveUserFavorit(m_wsUserName, m_wsPassword, sSiteGuid, favoriteID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol RemoveUserFavorite, Error Message: {0} Parameters : User {1}, Favourite: {2}", ex.Message, sSiteGuid, favoriteID);
            }
        }

        public UserResponseObject GetUserData(string sSiteGuid)
        {
            UserResponseObject response = null;

            try
            {
                response = m_Module.GetUserData(m_wsUserName, m_wsPassword, sSiteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUserData, Error Message: {0} Parameters : User {1}", ex.Message, sSiteGuid);
            }

            return response;
        }

        public UserResponseObject SetUserData(string sSiteGuid, UserBasicData userBasicData, UserDynamicData userDynamicData)
        {
            UserResponseObject response = null;

            try
            {
                response = m_Module.SetUserData(m_wsUserName, m_wsPassword, sSiteGuid, userBasicData, userDynamicData);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol SetUserData, Error Message: {0} Parameters : User {1}", ex.Message, sSiteGuid);
            }

            return response;
        }
        #endregion

    }
}
