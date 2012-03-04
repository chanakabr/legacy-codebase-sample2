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
using System.Web;

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

        [Serializable]
        public struct LogInResponseData
        {
            public string SiteGuid;
            public int DomainID;
            public ResponseStatus LoginStatus;
        }
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
        public LogInResponseData SignIn(string sUserName, string sPassword, string sSessionID, string sDeviceID, bool bIsDoubleLogin)
        {
            LogInResponseData loginData = new LogInResponseData();

            try
            {
                sDeviceID = string.Empty;
                sUserName = HttpUtility.UrlDecode(sUserName);
                UserResponseObject response = m_Module.SignIn(m_wsUserName, m_wsPassword, sUserName, sPassword, sSessionID, SiteHelper.GetClientIP(), sDeviceID, bIsDoubleLogin);                 

                if (response != null && response.m_user != null)
                {                    
                    loginData.SiteGuid = response.m_user.m_sSiteGUID;
                    loginData.DomainID = response.m_user.m_domianID;
                    loginData.LoginStatus = response.m_RespStatus;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SignIn, Error Message: {0}, Parameters :  Username: {1}, Password, {2}", ex.Message, sUserName, sPassword);
            }

            return loginData;
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

        public void SignOut(string sSiteGuid, string sSessionID, string sDeviceID, bool bPreventDoubleLogin)
        {            
            try
            {
                UserResponseObject uro = m_Module.SignOut(m_wsUserName, m_wsPassword, sSiteGuid, sSessionID, SiteHelper.GetClientIP(), sDeviceID, bPreventDoubleLogin);                
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SignOut, Error Message: {0}, Parameters :  SiteGuid: {1}", ex.Message, sSiteGuid);
            }            
        }

        public bool IsUserLoggedIn(string sSiteGuid, string sSessionID, string sDeviceID, string sIP, bool bPreventDoubleLogin)
        {
            bool bRet = false;
            try
            {                
                UserState response = m_Module.GetUserInstanceState(m_wsUserName, m_wsPassword, sSiteGuid, sSessionID, sDeviceID, sIP);
                if (response == UserState.Activated || (response == UserState.SingleSignIn && bPreventDoubleLogin) || 
                    (!bPreventDoubleLogin && (response == UserState.SingleSignIn || response == UserState.DoubleSignIn)))
                {
                    bRet = true;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : IsUserLoggedIn, Error Message: {0}, Parameters :  siteGuid: {1}", ex.Message, sSiteGuid);
            }

            return bRet;
        }

        public bool RemoveUserFavorite(int[] iFavoriteID)
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

        public FavoritObject[] GetUserFavorites(string sSiteGuid, string sItemType, int iDomainID, string sUDID)
        {
            FavoritObject[] response = null;

            try
            {
                response = m_Module.GetUserFavorites(m_wsUserName, m_wsPassword, sSiteGuid, iDomainID, string.Empty, sItemType);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUserFavorites, Error Message: {0} Parameters : User {1}", ex.Message, sSiteGuid);
            }

            return response;
        }

        public bool  AddUserFavorite(string sSiteGuid, int iDomainID, string sUDID, string sMediaType, string sMediaID, string sExtra)
        {
            bool bRet = false;
            try
            {
                bRet = m_Module.AddUserFavorit(m_wsUserName, m_wsPassword, sSiteGuid, iDomainID, sUDID, sMediaType, sMediaID, sExtra);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol AddUserFavorite, Error Message: {0} Parameters : User {1}, Media: {2}", ex.Message, sSiteGuid, sMediaID);
            }

            return bRet;
        }

        public void RemoveUserFavorite(string sSiteGuid, int[] mediaID)
        {
            try
            {
                m_Module.RemoveUserFavorit(m_wsUserName, m_wsPassword, sSiteGuid, mediaID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol RemoveUserFavorite, Error Message: {0} Parameters : User {1}, Favourite: {2}", ex.Message, sSiteGuid, mediaID);
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
