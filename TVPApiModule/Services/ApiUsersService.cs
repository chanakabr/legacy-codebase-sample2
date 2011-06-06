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
                logger.ErrorFormat("Error calling webservice protocol : CheckUserPassword, Error Message: {0}, Parameters :  Username: {1}, Password, {2}", ex.Message, sUserName, sPassword);
            }

            return sGuid;
        }

        public bool RemoveUserFavorite(int iFavoriteID)
        {
            bool IsRemoved = false;

            try
            {
                m_Module.RemoveUserFavorit(m_wsUserName, m_wsPassword, SiteHelper.GetClientIP(), iFavoriteID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol AddUserFavorit, Error Message: {0} Parameters :ws User name : {1} , ws Password {2} ", ex.Message, m_wsUserName, m_wsPassword);
            }

            return IsRemoved;
        }

        public FavoritObject[] GetUserFavorites(string userGuid, string sApplication, string sItemType)
        {
            throw new NotImplementedException();
        }

        public void AddUserFavorite(string sID, string sApplication, string sMediaType, string sMediaID, string sExtra)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserFavorite(string sID, int favoriteID)
        {
            throw new NotImplementedException();
        }

        public UserResponseObject GetUserData(string sSiteGuid)
        {
            throw new NotImplementedException();
        }
        #endregion

        
    }
}
