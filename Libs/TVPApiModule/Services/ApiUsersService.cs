using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiModule.Services
{
    public class ApiUsersService : ApiBase
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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

        public ApiUsersService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Users.UsersService();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }

        public UserResponseObject ValidateUser(string userName, string password, bool isDoubleLogin)
        {
            UserResponseObject response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.CheckUserPassword(m_wsUserName, m_wsPassword, userName, password, isDoubleLogin);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ValidateUser, Error Message: {0}, Parameters :  Username: {1}, Password, {2}", true, ex, ex.Message, userName, password);
            }

            return response;
        }

        public LogInResponseData SignIn(string sUserName, string sPassword, string sSessionID, string sDeviceID, bool bIsDoubleLogin, System.Collections.Specialized.NameValueCollection extraParams)
        {
            LogInResponseData loginData = new LogInResponseData();

            try
            {
                List<KeyValuePair> keyValueList = new List<KeyValuePair>();

                if (extraParams != null)
                {
                    foreach (string key in extraParams.Keys)
                    {
                        keyValueList.Add(new KeyValuePair() { key = key, value = extraParams[key] });
                    }
                }

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    UserResponseObject response = m_Module.SignIn(m_wsUserName, m_wsPassword, sUserName, sPassword, sSessionID, SiteHelper.GetClientIP(), sDeviceID, bIsDoubleLogin, keyValueList.ToArray());

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
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SignIn, Error Message: {0}, Parameters :  Username: {1}, Password, {2}", true, ex, ex.Message, sUserName, sPassword);
            }

            return loginData;
        }

        public UserResponseObject SignUp(UserBasicData userBasicData, UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            UserResponseObject response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.AddNewUser(m_wsUserName, m_wsPassword, userBasicData, userDynamicData, sPassword, sAffiliateCode);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SignUp, Error Message: {0}, Parameters :  Username: {1}, Password, {2}", true, ex, ex.Message, userBasicData.m_sUserName, sPassword);
            }

            return response;
        }

        public void SignOut(string sSiteGuid, string sSessionID, string sDeviceID, bool bPreventDoubleLogin)
        {
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    UserResponseObject uro = m_Module.SignOut(m_wsUserName, m_wsPassword, sSiteGuid, sSessionID, SiteHelper.GetClientIP(), sDeviceID, bPreventDoubleLogin);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SignOut, Error Message: {0}, Parameters :  SiteGuid: {1}", true, ex, ex.Message, sSiteGuid);
            }
        }

        public bool IsUserLoggedIn(string sSiteGuid, string sSessionID, string sDeviceID, string sIP, bool bPreventDoubleLogin)
        {
            bool bRet = false;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    UserState response = m_Module.GetUserInstanceState(m_wsUserName, m_wsPassword, sSiteGuid, sSessionID, sDeviceID, sIP);

                    if (response == UserState.Activated || (response == UserState.SingleSignIn && bPreventDoubleLogin) ||
                        (!bPreventDoubleLogin && (response == UserState.SingleSignIn || response == UserState.DoubleSignIn)))
                    {
                        bRet = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : IsUserLoggedIn, Error Message: {0}, Parameters :  siteGuid: {1}", true, ex, ex.Message, sSiteGuid);
            }

            return bRet;
        }

        public bool RemoveUserFavorite(int[] iFavoriteID)
        {
            bool IsRemoved = false;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = m_Module.RemoveUserFavorit(m_wsUserName, m_wsPassword, SiteHelper.GetClientIP(), iFavoriteID);

                    if (res != null && res.Code == (int)eStatus.OK)
                    {
                        IsRemoved = true;
                    }
                    else
                    {
                        IsRemoved = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol AddUserFavorit, Error Message: {0} Parameters :ws User name : {1} , ws Password {2} ", true, ex, ex.Message, m_wsUserName, m_wsPassword);
            }

            return IsRemoved;
        }

        public FavoritObject[] GetUserFavorites(string sSiteGuid, string sItemType, int iDomainID, string sUDID)
        {
            FavoritObject[] response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = m_Module.GetUserFavorites(m_wsUserName, m_wsPassword, sSiteGuid, iDomainID, string.Empty, sItemType, FavoriteOrderBy.CreateDateAsc);
                    response = res.Favorites;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetUserFavorites, Error Message: {0} Parameters : User {1}", true, ex, ex.Message, sSiteGuid);
            }

            return response;
        }

        public bool AddUserFavorite(string sSiteGuid, int iDomainID, string sUDID, string sMediaType, string sMediaID, string sExtra)
        {

            bool bRet = false;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = m_Module.AddUserFavorit(m_wsUserName, m_wsPassword, sSiteGuid, iDomainID, sUDID, sMediaType, sMediaID, sExtra);
                    if (res != null && res.Code == (int)eStatus.OK)
                    {
                        bRet = true;
                    }
                    else
                    {
                        bRet = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol AddUserFavorite, Error Message: {0} Parameters : User {1}, Media: {2}", true, ex, ex.Message, sSiteGuid, sMediaID);
            }

            return bRet;
        }

        public void RemoveUserFavorite(string sSiteGuid, int[] mediaID)
        {
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    m_Module.RemoveUserFavorit(m_wsUserName, m_wsPassword, sSiteGuid, mediaID);

                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol RemoveUserFavorite, Error Message: {0} Parameters : User {1}, Favorite: {2}", true, ex, ex.Message, sSiteGuid, mediaID);
            }
        }

        public LogInResponseData SSOSignIn(string sUserName, string sPassword, int nProviderID, string sSessionID, string sIP, string sDeviceID, bool bIsPreventDoubleLogins)
        {
            LogInResponseData loginData = default(LogInResponseData);

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    UserResponseObject response = m_Module.SSOSignIn(m_wsUserName, m_wsPassword, sUserName, sPassword, nProviderID, sSessionID, sIP, sDeviceID, bIsPreventDoubleLogins);

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
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SSOSignIn, Error Message: {0} Parameters : User {1}", true, ex, ex.Message, sUserName);
            }

            return loginData;
        }

        public UserResponseObject SSOCheckLogin(string sUserName, int nProviderID)
        {
            UserResponseObject response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.SSOCheckLogin(m_wsUserName, m_wsPassword, sUserName, nProviderID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SSOCheckLogin, Error Message: {0} Parameters : User {1}", true, ex, ex.Message, sUserName);
            }

            return response;
        }

        public UserResponseObject GetSSOProviders(string sUserName, int nProviderID)
        {
            UserResponseObject response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.SSOCheckLogin(m_wsUserName, m_wsPassword, sUserName, nProviderID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SSOCheckLogin, Error Message: {0} Parameters : User {1}", true, ex, ex.Message, sUserName);
            }

            return response;
        }

        public UserResponseObject GetUserData(string sSiteGuid)
        {
            UserResponseObject response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.GetUserData(m_wsUserName, m_wsPassword, sSiteGuid, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetUserData, Error Message: {0} Parameters : User {1}", true, ex, ex.Message, sSiteGuid);
            }

            return response;
        }

        public UserResponseObject[] GetUsersData(string[] siteGuids)
        {
            UserResponseObject[] response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.GetUsersData(m_wsUserName, m_wsPassword, siteGuids, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetUsersData, Error Message: {0}", true, ex, ex.Message);
            }

            return response;
        }

        public UserResponseObject SetUserData(string sSiteGuid, UserBasicData userBasicData, UserDynamicData userDynamicData)
        {
            UserResponseObject response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.SetUserData(m_wsUserName, m_wsPassword, sSiteGuid, userBasicData, userDynamicData);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SetUserData, Error Message: {0} Parameters : User {1}", true, ex, ex.Message, sSiteGuid);
            }

            return response;
        }

        public UserResponseObject ActivateAccount(string sUserName, string sToken)
        {
            UserResponseObject response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = m_Module.ActivateAccount(m_wsUserName, m_wsPassword, sUserName, sToken);
                    if (res != null)
                        response = res.user;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ActivateAccount, Error Message: {0}, Parameters :  sUserName: {1}", true, ex, ex.Message, sUserName);
            }

            return response;
        }

        public bool ResendActivationMail(string sUserName, string sNewPassword)
        {
            bool response = false;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = m_Module.ResendActivationMail(m_wsUserName, m_wsPassword, sUserName, sNewPassword);
                    if (res != null && res.Code == (int)eStatus.OK)
                        response = true;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ResendActivationMail, Error Message: {0}, Parameters :  sUserName: {1}", true, ex, ex.Message, sUserName);
            }

            return response;
        }

        public UserOfflineObject[] GetUserOfflineList(string sSiteGuid)
        {
            UserOfflineObject[] response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.GetAllUserOfflineAssets(m_wsUserName, m_wsPassword, sSiteGuid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetUserOfflineList, Error Message: {0} Parameters : User {1}", true, ex, ex.Message, sSiteGuid);
            }

            return response;
        }

        public bool AddUserOfflineMedia(string siteGuid, int mediaID)
        {
            bool response = false;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.AddUserOfflineAsset(m_wsUserName, m_wsPassword, siteGuid, mediaID.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol AddUserOfflineMedia, Error Message: {0} Parameters : User {1}", true, ex, ex.Message, siteGuid);
            }

            return response;
        }

        public bool RemoveUserOfflineMedia(string siteGuid, int mediaID)
        {
            bool response = false;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.RemoveUserOfflineAsset(m_wsUserName, m_wsPassword, siteGuid, mediaID.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol RemoveUserOfflineMedia, Error Message: {0} Parameters : User {1}", true, ex, ex.Message, siteGuid);
            }

            return response;
        }

        public bool ClearUserOfflineList(string siteGuid)
        {
            bool response = false;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.ClearUserOfflineAssets(m_wsUserName, m_wsPassword, siteGuid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol ClearUserOfflineList, Error Message: {0} Parameters : User {1}", true, ex, ex.Message, siteGuid);
            }

            return response;
        }

        public bool SentNewPasswordToUser(string UserName)
        {
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    UserResponseObject uro = m_Module.ForgotPassword(m_wsUserName, m_wsPassword, UserName);
                    if (uro.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                    {
                        logger.InfoFormat("Sent new temp password protocol ForgotPassword, Parameters : User name {0}: ", true, UserName);
                        return true;
                    }
                    else
                    {
                        logger.InfoFormat("Can not send temp password protocol CheckUserPassword,Parameters : User name : {0}", true, UserName);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occurred in SentNewPasswordToUser protocol ForgotPassword, Error Message: {0} Parameters :User : {1} ", true, ex, ex.Message, UserName);
                return false;
            }
        }

        public string IpToCountry(string sIP)
        {
            string sRet = string.Empty;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    Country response = m_Module.GetIPToCountry(m_wsUserName, m_wsPassword, sIP);
                    sRet = response.m_sCountryName;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol IpToCountry, Error Message: {0} Parameters : UserIP {1}", true, ex, ex.Message, sIP);
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.SetUserDynamicData(m_wsUserName, m_wsPassword, sSiteGuid, sKey, sValue);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SetUserDynamicData, Error Message: {0} Parameters :ws User name : {1} , ws Password: {2}, SiteGUID: {3}, Key: {4}, Value: {5}", true, ex, ex.Message, m_wsUserName, m_wsPassword, sSiteGuid, sKey, sValue);
            }

            return bRet;
        }

        public UserResponseObject GetUserDataByCoGuid(string coGuid, int operatorID)
        {
            UserResponseObject response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.GetUserDataByCoGuid(m_wsUserName, m_wsPassword, coGuid, operatorID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetUserData, Error Message: {0} Parameters : coGuid {1}", true, ex, ex.Message, coGuid);
            }

            return response;
        }

        public UserResponseObject ChangeUserPassword(string sUN, string sOldPass, string sPass)
        {
            UserResponseObject bRet = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.ChangeUserPassword(m_wsUserName, m_wsPassword, sUN, sOldPass, sPass);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol ChangeUserPassword, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", true, ex, ex.Message, m_wsUserName, m_wsPassword);
            }

            return bRet;
        }

        public UserResponseObject GetUserByFacebookID(string facebookId)
        {
            UserResponseObject bRet = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.GetUserByFacebookID(m_wsUserName, m_wsPassword, facebookId);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetUserByFacebookID, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", true, ex, ex.Message, m_wsUserName, m_wsPassword);
            }

            return bRet;
        }

        public UserResponseObject GetUserByUsername(string userName)
        {
            UserResponseObject bRet = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.GetUserByUsername(m_wsUserName, m_wsPassword, userName);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetUserByUsername, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", true, ex, ex.Message, m_wsUserName, m_wsPassword);
            }

            return bRet;
        }

        public UserBasicData[] SearchUsers(string[] sTerms, string[] sFields, bool bIsExact)
        {
            UserBasicData[] bRet = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.SearchUsers(m_wsUserName, m_wsPassword, sTerms, sFields, bIsExact);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SearchUsers, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", true, ex, ex.Message, m_wsUserName, m_wsPassword);
            }

            return bRet;
        }

        public void Logout(string sSiteGuid)
        {
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    m_Module.Logout(m_wsUserName, m_wsPassword, sSiteGuid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol Logout, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, SiteGUID: {3}", true, ex, ex.Message, m_wsUserName, m_wsPassword, sSiteGuid);
            }
        }

        public Country[] GetCountriesList()
        {
            Country[] bRet = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.GetCountryList(m_wsUserName, m_wsPassword);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetCountryList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", true, ex, ex.Message, m_wsUserName, m_wsPassword);
            }

            return bRet;
        }

        public UserResponseObject CheckTemporaryToken(string sToken)
        {
            UserResponseObject bRet = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.CheckTemporaryToken(m_wsUserName, m_wsPassword, sToken);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol CheckTemporaryToken, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", true, ex, ex.Message, m_wsUserName, m_wsPassword);
            }

            return bRet;
        }

        public UserType[] GetGroupUserTypes()
        {
            UserType[] bRet = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.GetGroupUserTypes(m_wsUserName, m_wsPassword);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetGroupUserTypes, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", true, ex, ex.Message, m_wsUserName, m_wsPassword);
            }

            return bRet;
        }

        public UserResponseObject RenewUserPassword(string sUN, string sPass)
        {
            UserResponseObject bRet = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.RenewUserPassword(m_wsUserName, m_wsPassword, sUN, sPass);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol RenewUserPassword, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", true, ex, ex.Message, m_wsUserName, m_wsPassword);
            }

            return bRet;
        }

        public ResponseStatus RenewUserPIN(string sSiteGuid, int ruleID)
        {
            ResponseStatus bRet = ResponseStatus.OK;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.SendChangedPinMail(m_wsUserName, m_wsPassword, sSiteGuid, ruleID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol RenewUserPIN, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, sSiteGUID: {3}, ruleID: {4}", true, ex, ex.Message, m_wsUserName, m_wsPassword, sSiteGuid, ruleID);

                bRet = ResponseStatus.ErrorOnSendingMail;
            }

            return bRet;
        }

        public UserResponseObject ActivateAccountByDomainMaster(string masterUserName, string userName, string token)
        {
            UserResponseObject res = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.ActivateAccountByDomainMaster(m_wsUserName, m_wsPassword, masterUserName, userName, token);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol ActivateAccountByDomainMaster, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, masterUserName: {3}, userName: {4}, token: {5}", true, ex,
                    ex.Message, m_wsUserName, m_wsPassword, masterUserName, userName, token);
            }

            return res;
        }

        public bool SendPasswordMail(string userName)
        {
            bool res = false;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.SendPasswordMail(m_wsUserName, m_wsPassword, userName);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SendPasswordMail, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, userName: {3}", true, ex,
                    ex.Message, m_wsUserName, m_wsPassword, userName);
            }

            return res;
        }

        public bool AddItemToList(string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            bool res = false;

            try
            {
                UserItemList userItemList = new UserItemList()
                {
                    itemObj = itemObjects,
                    itemType = itemType,
                    listType = listType,
                    siteGuid = siteGuid

                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.AddItemToList(m_wsUserName, m_wsPassword, userItemList);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol AddItemToList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, siteGuide: {3}", true, ex,
                    ex.Message, m_wsUserName, m_wsPassword, siteGuid);
            }

            return res;
        }

        public bool RemoveItemFromList(string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            bool res = false;

            try
            {
                UserItemList userItemList = new UserItemList()
                {
                    itemObj = itemObjects,
                    itemType = itemType,
                    listType = listType,
                    siteGuid = siteGuid

                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.RemoveItemFromList(m_wsUserName, m_wsPassword, userItemList);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol RemoveItemFromList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, siteGuid: {3}", true, ex,
                    ex.Message, m_wsUserName, m_wsPassword, siteGuid);
            }

            return res;
        }

        public bool UpdateItemInList(string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            bool res = false;

            try
            {
                UserItemList userItemList = new UserItemList()
                {
                    itemObj = itemObjects,
                    itemType = itemType,
                    listType = listType,
                    siteGuid = siteGuid

                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.UpdateItemInList(m_wsUserName, m_wsPassword, userItemList);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol UpdateItemInList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, siteGuid: {3}", true, ex,
                    ex.Message, m_wsUserName, m_wsPassword, siteGuid);
            }

            return res;
        }

        public UserItemList[] GetItemFromList(string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            UserItemList[] res = null;

            try
            {
                UserItemList userItemList = new UserItemList()
                {
                    itemObj = itemObjects,
                    itemType = itemType,
                    listType = listType,
                    siteGuid = siteGuid

                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var response = m_Module.GetItemFromList(m_wsUserName, m_wsPassword, userItemList);
                    if (response != null)
                        res = response.UserItemLists;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetItemFromList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, siteGuid: {3}", true, ex,
                    ex.Message, m_wsUserName, m_wsPassword, siteGuid);
            }

            return res;
        }

        public KeyValuePair[] IsItemExistsInList(string siteGuid, ItemObj[] itemObjects, ItemType itemType, ListType listType)
        {
            KeyValuePair[] res = null;

            try
            {
                UserItemList userItemList = new UserItemList()
                {
                    itemObj = itemObjects,
                    itemType = itemType,
                    listType = listType,
                    siteGuid = siteGuid

                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.IsItemExistsInList(m_wsUserName, m_wsPassword, userItemList);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol IsItemExistsInList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, siteGuid: {3}", true, ex,
                    ex.Message, m_wsUserName, m_wsPassword, siteGuid);
            }

            return res;
        }

        public ResponseStatus SetUserTypeByUserID(string sSiteGuid, int userTypeID)
        {
            ResponseStatus bRet = ResponseStatus.OK;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.SetUserTypeByUserID(m_wsUserName, m_wsPassword, sSiteGuid, userTypeID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SetUserTypeByUserID, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, sSiteGUID: {3}, userTypeID: {4}", true, ex, ex.Message, m_wsUserName, m_wsPassword, sSiteGuid, userTypeID);
                bRet = ResponseStatus.ErrorOnUpdatingUserType;
            }
            return bRet;
        }

        public LogInResponseData SignInWithToken(string sToken, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins)
        {
            LogInResponseData loginData = new LogInResponseData();
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    UserResponseObject response = m_Module.SignInWithToken(m_wsUserName, m_wsPassword, sToken, sSessionID, sIP, sDeviceID, bPreventDoubleLogins);

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
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SignInWithToken, Error Message: {0}, Parameters : ws User name : {1}, ws Password: {2}, token: {3}", true, ex, ex.Message, m_wsUserName, m_wsPassword, sToken);
            }
            return loginData;
        }

        public TVPApiModule.Objects.Responses.PinCodeResponse GenerateLoginPIN(string siteGuid, string secret)
        {
            TVPApiModule.Objects.Responses.PinCodeResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = m_Module.GenerateLoginPIN(m_wsUserName, m_wsPassword, siteGuid, secret);
                    response = new TVPApiModule.Objects.Responses.PinCodeResponse(result);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error while trying to get regions.", ex);
                response = new TVPApiModule.Objects.Responses.PinCodeResponse();
                response.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }
            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponse LoginWithPIN(string PIN, string secret, string deviceID)
        {
            TVPApiModule.Objects.Responses.UserResponse response = null;
            try
            {
                string sessionID = "0";
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = m_Module.LoginWithPIN(m_wsUserName, m_wsPassword, PIN, sessionID, SiteHelper.GetClientIP(), deviceID, false, null, secret);
                    response = new TVPApiModule.Objects.Responses.UserResponse(result);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error while trying to get regions.", ex);
                response = new TVPApiModule.Objects.Responses.UserResponse();
                response.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }
            return response;
        }

        public TVPApiModule.Objects.Responses.PinCodeResponse SetLoginPIN(string siteGuid, string PIN, string secret)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.PinCodeResponse wsResponse = null;
            TVPApiModule.Objects.Responses.PinCodeResponse response;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    wsResponse = m_Module.SetLoginPIN(m_wsUserName, m_wsPassword, siteGuid, PIN, secret);
                    response = new TVPApiModule.Objects.Responses.PinCodeResponse(wsResponse);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetLoginPIN, Error Message: {0}, Parameters: PIN Id: {1}, secret: {2}, siteGuid : {3}", ex.Message, PIN, secret, siteGuid);
                response = new TVPApiModule.Objects.Responses.PinCodeResponse();
                response.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public ClientResponseStatus ClearLoginPINs(string siteGuid, string pinCode)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.Status result = null;
            ClientResponseStatus clientResponse;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    result = m_Module.ClearLoginPIN(m_wsUserName, m_wsPassword, siteGuid, pinCode);
                    clientResponse = new ClientResponseStatus(result.Code, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ClearLoginPIN, Error Message: {0}, Parameters: siteGuid : {1}", ex.Message, siteGuid);
                clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse("Error while calling webservice");
            }

            return clientResponse;
        }

        public ClientResponseStatus DeleteUser(string siteGuid)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.Status result = null;
            ClientResponseStatus clientResponse;
            int userid = 0;

            try
            {
                if (!string.IsNullOrEmpty(siteGuid) && int.TryParse(siteGuid, out userid))
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        result = m_Module.DeleteUser(m_wsUserName, m_wsPassword, userid);
                        clientResponse = new ClientResponseStatus(result.Code, result.Message);
                    }
                }
                else
                {
                    logger.ErrorFormat("Error while trying to delete user {0}", siteGuid != null ? siteGuid : "null");
                    clientResponse = new TVPApiModule.Objects.Responses.ClientResponseStatus();
                    clientResponse.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : DeleteUser, Error Message: {0}, Parameters: siteGuid : {1}", ex.Message, siteGuid);
                clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse("Error while calling webservice");
            }

            return clientResponse;
        }

        public ClientResponseStatus ChangeUsers(string initSiteGuid, string siteGuid, string udid)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.Status result = null;
            ClientResponseStatus clientResponse;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    result = m_Module.ChangeUsers(m_wsUserName, m_wsPassword, initSiteGuid, siteGuid, udid);
                    clientResponse = new ClientResponseStatus(result.Code, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChangeUsers, Error Message: {0}, Parameters: initSiteGuid : {1}, siteGuid: {2}", ex.Message, initSiteGuid, siteGuid);
                clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse("Error while calling webservice");
            }

            return clientResponse;
        }

    }
}
