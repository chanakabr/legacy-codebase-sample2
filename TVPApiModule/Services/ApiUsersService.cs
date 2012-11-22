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
            public User UserData;
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
        public UserResponseObject ValidateUser(string userName, string password, bool isDoubleLogin)
        {
            UserResponseObject response = null;
            try
            {
                response = m_Module.CheckUserPassword(m_wsUserName, m_wsPassword, userName, password, isDoubleLogin);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ValidateUser, Error Message: {0}, Parameters :  Username: {1}, Password, {2}", ex.Message, userName, password);
            }

            return response;
        }

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
                    loginData.UserData = response.m_user;
                }
                else if (response != null)
                {
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

        public UserResponseObject SSOSignIn(string sUserName, string sPassword, int nProviderID, string sSessionID, string sIP, string sDeviceID, bool bIsPreventDoubleLogins)
        {
            UserResponseObject response = null;

            try
            {
                response = m_Module.SSOSignIn(m_wsUserName, m_wsPassword, sUserName, sPassword, nProviderID, sSessionID, sIP, sDeviceID, bIsPreventDoubleLogins);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol SSOSignIn, Error Message: {0} Parameters : User {1}", ex.Message, sUserName);
            }

            return response;
        }        

        public UserResponseObject SSOCheckLogin(string sUserName, int nProviderID)
        {
            UserResponseObject response = null;

            try
            {
                response = m_Module.SSOCheckLogin(m_wsUserName, m_wsPassword, sUserName, nProviderID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol SSOCheckLogin, Error Message: {0} Parameters : User {1}", ex.Message, sUserName);
            }

            return response;
        }

        public UserResponseObject GetSSOProviders(string sUserName, int nProviderID)
        {
            UserResponseObject response = null;

            try
            {
                response = m_Module.SSOCheckLogin(m_wsUserName, m_wsPassword, sUserName, nProviderID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol SSOCheckLogin, Error Message: {0} Parameters : User {1}", ex.Message, sUserName);
            }

            return response;
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

        public UserResponseObject[] GetUsersData(string sSiteGuids)
        {
            UserResponseObject[] response = null;

            try
            {
                response = m_Module.GetUsersData(m_wsUserName, m_wsPassword, sSiteGuids.Split(';'));
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUsersData, Error Message: {0}", ex.Message);
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

        public UserOfflineObject[] GetUserOfflineList(string sSiteGuid)
        {
            UserOfflineObject[] response = null;

            try
            {                
                response = m_Module.GetAllUserOfflineAssets(m_wsUserName, m_wsPassword, sSiteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUserOfflineList, Error Message: {0} Parameters : User {1}", ex.Message, sSiteGuid);
            }

            return response;
        }

        public bool AddUserOfflineMedia(string siteGuid, int mediaID)
        {
            bool response = false;

            try
            {
                response = m_Module.AddUserOfflineAsset(m_wsUserName, m_wsPassword, siteGuid, mediaID.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol AddUserOfflineMedia, Error Message: {0} Parameters : User {1}", ex.Message, siteGuid);
            }

            return response;
        }

        public bool RemoveUserOfflineMedia(string siteGuid, int mediaID)
        {
            bool response = false;

            try
            {
                response = m_Module.RemoveUserOfflineAsset(m_wsUserName, m_wsPassword, siteGuid, mediaID.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol RemoveUserOfflineMedia, Error Message: {0} Parameters : User {1}", ex.Message, siteGuid);
            }

            return response;
        }

        public bool ClearUserOfflineList(string siteGuid)
        {
            bool response = false;

            try
            {
                response = m_Module.ClearUserOfflineAssets(m_wsUserName, m_wsPassword, siteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol ClearUserOfflineList, Error Message: {0} Parameters : User {1}", ex.Message, siteGuid);
            }

            return response;
        }

        public bool SentNewPasswordToUser(string UserName)
        {
            try
            {
                UserResponseObject uro = m_Module.ForgotPassword(m_wsUserName, m_wsPassword, UserName);
                if (uro.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                {
                    logger.InfoFormat("Sent new temp password protocol ForgotPassword, Parameters : User name {0}: ", UserName);
                    return true;
                }
                else
                {
                    logger.InfoFormat("Can not send temp password protocol CheckUserPassword,Parameters : User name : {0}", UserName);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in SentNewPasswordToUser protocol ForgotPassword, Error Message: {0} Parameters :User : {1} ", ex.Message, UserName);
                return false;
            }
        }

        public string IpToCountry(string sIP)
        {
            string sRet = string.Empty;

            try
            {
                Country response = m_Module.GetIPToCountry(m_wsUserName, m_wsPassword, sIP);
                sRet = response.m_sCountryName;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol IpToCountry, Error Message: {0} Parameters : UserIP {1}", ex.Message, sIP);
            }

            return sRet;
        }

        public bool IsOfflineModeEnabled(string siteGuid)
        {
            var offlineMode = GetUserData(siteGuid).m_user.m_oDynamicData.m_sUserData.Where(x => x.m_sDataType == "IsOfflineMode" && x.m_sValue == "true").FirstOrDefault();

            if (offlineMode == null)
                return false;

            if (offlineMode.m_sValue == "false")
                return false;

            return true;
        }

        private TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData cloneDynamicData(TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData curDynamicData, bool isAddNew)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData newDynamicData = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData();
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer dData;
            newDynamicData.m_sUserData = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer[curDynamicData.m_sUserData.Count() + (isAddNew ? 1 : 0)];
            int idx = 0;

            foreach (var UserData in curDynamicData.m_sUserData)
            {
                dData = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer();
                dData.m_sDataType = UserData.m_sDataType;
                dData.m_sValue = UserData.m_sValue;
                newDynamicData.m_sUserData[idx] = dData;
                idx++;
            }

            return newDynamicData;
        }

        public void ToggleOfflineMode(string siteGUID, bool isTurnOn)
        {
            if (isTurnOn)
            {
                var userData = GetUserData(siteGUID);
                var curDynamicData = userData.m_user.m_oDynamicData;
                var isOfflineMode = curDynamicData.m_sUserData.Where(x => x != null && x.m_sDataType == "IsOfflineMode").Count() > 0;
                var newDynamicData = cloneDynamicData(curDynamicData, !isOfflineMode);

                if (!isOfflineMode)
                {
                    TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer dData = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer();
                    dData.m_sDataType = "IsOfflineMode";
                    dData.m_sValue = "true";
                    newDynamicData.m_sUserData[newDynamicData.m_sUserData.Count() - 1] = dData;
                }
                else
                    newDynamicData.m_sUserData.Where(x => x.m_sDataType == "IsOfflineMode").First().m_sValue = "true";

                SetUserData(siteGUID, userData.m_user.m_oBasicData, newDynamicData);
            }
            else
            {
                var userData = GetUserData(siteGUID);
                var curDynamicData = userData.m_user.m_oDynamicData;
                var newDynamicData = cloneDynamicData(curDynamicData, false);

                newDynamicData.m_sUserData.Where(x => x.m_sDataType == "IsOfflineMode").First().m_sValue = "false";
                SetUserData(siteGUID, userData.m_user.m_oBasicData, newDynamicData);
            }
        }

        public bool SetUserDynamicData(string sSiteGuid, string sKey, string sValue)
        {
            bool bRet = false;
            try
            {
                bRet = m_Module.SetUserDynamicData(m_wsUserName, m_wsPassword, sSiteGuid, sKey, sValue);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol SetUserDynamicData, Error Message: {0} Parameters :ws User name : {1} , ws Password: {2}, SiteGUID: {3}, Key: {4}, Value: {5}", ex.Message, m_wsUserName, m_wsPassword, sSiteGuid, sKey, sValue);
            }

            return bRet;
        }
    }
}
