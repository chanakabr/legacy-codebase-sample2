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
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Extentions;

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
            public TVPApiModule.Objects.Responses.eResponseStatus LoginStatus;
            public TVPApiModule.Objects.Responses.User UserData;
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

        public TVPApiModule.Objects.Responses.UserResponseObject ValidateUser(string userName, string password, bool isDoubleLogin)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;
            try
            {
                var res = m_Module.CheckUserPassword(m_wsUserName, m_wsPassword, userName, password, isDoubleLogin);
                if (res != null)
                    response = res.ToApiObject();
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
                TVPApiModule.Objects.Responses.UserResponseObject response = m_Module.SignIn(m_wsUserName, m_wsPassword, sUserName, sPassword, sSessionID, SiteHelper.GetClientIP(), sDeviceID, bIsDoubleLogin).ToApiObject();

                if (response != null && response.user != null)
                {
                    loginData.SiteGuid = response.user.site_guid;
                    loginData.DomainID = response.user.domian_id;
                    loginData.LoginStatus = response.resp_status;
                    loginData.UserData = response.user;
                }
                else if (response != null)
                {
                    loginData.LoginStatus = response.resp_status;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SignIn, Error Message: {0}, Parameters :  Username: {1}, Password, {2}", ex.Message, sUserName, sPassword);
            }

            return loginData;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject SignUp(TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;
            try
            {
                var res = m_Module.AddNewUser(m_wsUserName, m_wsPassword, userBasicData, userDynamicData, sPassword, sAffiliateCode);
                if (res != null)
                    response = res.ToApiObject();
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
                TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject uro = m_Module.SignOut(m_wsUserName, m_wsPassword, sSiteGuid, sSessionID, SiteHelper.GetClientIP(), sDeviceID, bPreventDoubleLogin);
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
                TVPPro.SiteManager.TvinciPlatform.Users.UserState response = m_Module.GetUserInstanceState(m_wsUserName, m_wsPassword, sSiteGuid, sSessionID, sDeviceID, sIP);
                if (response == TVPPro.SiteManager.TvinciPlatform.Users.UserState.Activated || (response == TVPPro.SiteManager.TvinciPlatform.Users.UserState.SingleSignIn && bPreventDoubleLogin) ||
                    (!bPreventDoubleLogin && (response == TVPPro.SiteManager.TvinciPlatform.Users.UserState.SingleSignIn || response == TVPPro.SiteManager.TvinciPlatform.Users.UserState.DoubleSignIn)))
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

        public List<FavoriteObject> GetUserFavorites(string sSiteGuid, string sItemType, int iDomainID, string sUDID)
        {
            List<FavoriteObject> retVal = null;

            try
            {
                var response = m_Module.GetUserFavorites(m_wsUserName, m_wsPassword, sSiteGuid, iDomainID, string.Empty, sItemType);

                if (response != null && response.Length > 0)
                    retVal = response.Where(f => f != null).Select(f => f.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUserFavorites, Error Message: {0} Parameters : User {1}", ex.Message, sSiteGuid);
            }

            return retVal;
        }

        public bool AddUserFavorite(string sSiteGuid, int iDomainID, string sUDID, string sMediaType, string sMediaID, string sExtra)
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

        public TVPApiModule.Objects.Responses.UserResponseObject SSOSignIn(string sUserName, string sPassword, int nProviderID, string sSessionID, string sIP, string sDeviceID, bool bIsPreventDoubleLogins)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            try
            {
                var res = m_Module.SSOSignIn(m_wsUserName, m_wsPassword, sUserName, sPassword, nProviderID, sSessionID, sIP, sDeviceID, bIsPreventDoubleLogins);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol SSOSignIn, Error Message: {0} Parameters : User {1}", ex.Message, sUserName);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject SSOCheckLogin(string sUserName, int nProviderID)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            try
            {
                var res = m_Module.SSOCheckLogin(m_wsUserName, m_wsPassword, sUserName, nProviderID);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol SSOCheckLogin, Error Message: {0} Parameters : User {1}", ex.Message, sUserName);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject GetSSOProviders(string sUserName, int nProviderID)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            try
            {
                var res = m_Module.SSOCheckLogin(m_wsUserName, m_wsPassword, sUserName, nProviderID);
                if (res != null)
                    response = res.ToApiObject();

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol SSOCheckLogin, Error Message: {0} Parameters : User {1}", ex.Message, sUserName);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject GetUserData(string sSiteGuid)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            try
            {
                var res = m_Module.GetUserData(m_wsUserName, m_wsPassword, sSiteGuid);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUserData, Error Message: {0} Parameters : User {1}", ex.Message, sSiteGuid);
            }

            return response;
        }

        public List<TVPApiModule.Objects.Responses.UserResponseObject> GetUsersData(string sSiteGuids)
        {
            List<TVPApiModule.Objects.Responses.UserResponseObject> response = null;

            try
            {
                var res = m_Module.GetUsersData(m_wsUserName, m_wsPassword, sSiteGuids.Split(';'));

                if (res != null)
                    response = res.Where(ur => ur != null).Select(u => u.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUsersData, Error Message: {0}", ex.Message);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject SetUserData(string sSiteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            try
            {
                var res = m_Module.SetUserData(m_wsUserName, m_wsPassword, sSiteGuid, userBasicData, userDynamicData);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol SetUserData, Error Message: {0} Parameters : User {1}", ex.Message, sSiteGuid);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject ActivateAccount(string sUserName, string sToken)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            try
            {
                var res = m_Module.ActivateAccount(m_wsUserName, m_wsPassword, sUserName, sToken);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ActivateAccount, Error Message: {0}, Parameters :  sUserName: {1}", ex.Message, sUserName);
            }

            return response;
        }

        public bool ResendActivationMail(string sUserName, string sNewPassword)
        {
            bool response = false;

            try
            {
                response = m_Module.ResendActivationMail(m_wsUserName, m_wsPassword, sUserName, sNewPassword);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ResendActivationMail, Error Message: {0}, Parameters :  sUserName: {1}", ex.Message, sUserName);
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
                TVPApiModule.Objects.Responses.UserResponseObject uro = m_Module.ForgotPassword(m_wsUserName, m_wsPassword, UserName).ToApiObject();
                if (uro.resp_status == TVPApiModule.Objects.Responses.eResponseStatus.OK)
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
                TVPPro.SiteManager.TvinciPlatform.Users.Country response = m_Module.GetIPToCountry(m_wsUserName, m_wsPassword, sIP);
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
            var offlineMode = GetUserData(siteGuid).user.dynamic_data.user_data.Where(x => x.data_type == "IsOfflineMode" && x.value == "true").FirstOrDefault();

            if (offlineMode == null)
                return false;

            if (offlineMode.value == "false")
                return false;

            return true;
        }

        private TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData cloneDynamicData(TVPApiModule.Objects.Responses.UserDynamicData curDynamicData, bool isAddNew)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData newDynamicData = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData();
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer dData;
            newDynamicData.m_sUserData = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer[curDynamicData.user_data.Count() + (isAddNew ? 1 : 0)];
            int idx = 0;

            foreach (var UserData in curDynamicData.user_data)
            {
                dData = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer();
                dData.m_sDataType = UserData.data_type;
                dData.m_sValue = UserData.value;
                newDynamicData.m_sUserData[idx] = dData;
                idx++;
            }

            return newDynamicData;
        }

        public void ToggleOfflineMode(string siteGUID, bool isTurnOn)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData newDynamicData;
            TVPApiModule.Objects.Responses.UserResponseObject userData;
            TVPApiModule.Objects.Responses.UserDynamicData curDynamicData;

            if (isTurnOn)
            {
                
                userData = GetUserData(siteGUID);
                curDynamicData = userData.user.dynamic_data;
                var isOfflineMode = curDynamicData.user_data.Where(x => x != null && x.data_type == "IsOfflineMode").Count() > 0;
                newDynamicData = cloneDynamicData(curDynamicData, !isOfflineMode);

                if (!isOfflineMode)
                {
                    TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer dData = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer();
                    dData.m_sDataType = "IsOfflineMode";
                    dData.m_sValue = "true";
                    newDynamicData.m_sUserData[newDynamicData.m_sUserData.Count() - 1] = dData;
                }
                else
                    newDynamicData.m_sUserData.Where(x => x.m_sDataType == "IsOfflineMode").First().m_sValue = "true";

                
            }
            else
            {
                userData = GetUserData(siteGUID);
                curDynamicData = userData.user.dynamic_data;
                newDynamicData = cloneDynamicData(curDynamicData, false);

                newDynamicData.m_sUserData.Where(x => x.m_sDataType == "IsOfflineMode").First().m_sValue = "false";
            }

            TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData = new TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData()
            {
                m_bIsFacebookImagePermitted = userData.user.basic_data.is_facebook_image_permitted,
                m_CoGuid = userData.user.basic_data.co_guid,
                m_Country = new TVPPro.SiteManager.TvinciPlatform.Users.Country()
                {
                    m_nObjecrtID = userData.user.basic_data.country.object_id,
                    m_sCountryCode = userData.user.basic_data.country.country_code,
                    m_sCountryName = userData.user.basic_data.country.country_name
                },
                m_ExternalToken = userData.user.basic_data.external_token,
                m_sAddress = userData.user.basic_data.address,
                m_sAffiliateCode = userData.user.basic_data.affiliate_code,
                m_sCity = userData.user.basic_data.city,
                m_sEmail = userData.user.basic_data.email,
                m_sFacebookID = userData.user.basic_data.facebook_id,
                m_sFacebookImage = userData.user.basic_data.facebook_image,
                m_sFacebookToken = userData.user.basic_data.facebook_token,
                m_sFirstName = userData.user.basic_data.first_name,
                m_sLastName = userData.user.basic_data.last_name,
                m_sPhone = userData.user.basic_data.phone,
                m_State = new TVPPro.SiteManager.TvinciPlatform.Users.State()
                {
                    m_Country = new TVPPro.SiteManager.TvinciPlatform.Users.Country()
                    {
                        m_nObjecrtID = userData.user.basic_data.state.country.object_id,
                        m_sCountryCode = userData.user.basic_data.state.country.country_code,
                        m_sCountryName = userData.user.basic_data.state.country.country_name
                    },
                    m_nObjecrtID = userData.user.basic_data.state.object_id,
                    m_sStateCode = userData.user.basic_data.state.state_code,
                    m_sStateName = userData.user.basic_data.state.state_name
                },
                m_sUserName = userData.user.basic_data.user_name,
                m_sZip = userData.user.basic_data.zip,
                m_UserType = new TVPPro.SiteManager.TvinciPlatform.Users.UserType()
                {
                    Description = userData.user.basic_data.user_type.description,
                    ID = userData.user.basic_data.user_type.id,
                    IsDefault = userData.user.basic_data.user_type.is_default
                }
            };

            SetUserData(siteGUID, userBasicData, newDynamicData);
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

        public TVPApiModule.Objects.Responses.UserResponseObject GetUserDataByCoGuid(string coGuid, int operatorID)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            try
            {
                var res = m_Module.GetUserDataByCoGuid(m_wsUserName, m_wsPassword, coGuid, operatorID);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUserData, Error Message: {0} Parameters : coGuid {1}", ex.Message, coGuid);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject ChangeUserPassword(string sUN, string sOldPass, string sPass)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;
            try
            {
                var res = m_Module.ChangeUserPassword(m_wsUserName, m_wsPassword, sUN, sOldPass, sPass);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol ChangeUserPassword, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", ex.Message, m_wsUserName, m_wsPassword);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject GetUserByFacebookID(string facebookId)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;
            try
            {
                var res = m_Module.GetUserByFacebookID(m_wsUserName, m_wsPassword, facebookId);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetUserByFacebookID, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", ex.Message, m_wsUserName, m_wsPassword);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject GetUserByUsername(string userName)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;
            try
            {
                var res  = m_Module.GetUserByUsername(m_wsUserName, m_wsPassword, userName);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetUserByUsername, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", ex.Message, m_wsUserName, m_wsPassword);
            }

            return response;
        }

        public List<TVPApiModule.Objects.Responses.UserBasicData> SearchUsers(string[] sTerms, string[] sFields, bool bIsExact)
        {
            List<TVPApiModule.Objects.Responses.UserBasicData> response = null;

            try
            {
                var res = m_Module.SearchUsers(m_wsUserName, m_wsPassword, sTerms, sFields, bIsExact);

                if (res != null)
                    response = res.Where(ubd => ubd != null).Select(u => u.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SearchUsers, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", ex.Message, m_wsUserName, m_wsPassword);
            }

            return response;
        }

        public void Logout(string sSiteGuid)
        {
            try
            {
                m_Module.Logout(m_wsUserName, m_wsPassword, sSiteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol Logout, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, SiteGUID: {3}", ex.Message, m_wsUserName, m_wsPassword, sSiteGuid);
            }
        }

        public List<TVPApiModule.Objects.Responses.Country> GetCountriesList()
        {
            List<TVPApiModule.Objects.Responses.Country> response = null;

            try
            {
                var res = m_Module.GetCountryList(m_wsUserName, m_wsPassword);

                if (res != null)
                    response = res.Where(c => c != null).Select(c => c.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetCountryList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", ex.Message, m_wsUserName, m_wsPassword);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject CheckTemporaryToken(string sToken)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;
            try
            {
                var res = m_Module.CheckTemporaryToken(m_wsUserName, m_wsPassword, sToken);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol CheckTemporaryToken, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", ex.Message, m_wsUserName, m_wsPassword);
            }

            return response;
        }

        public List<TVPApiModule.Objects.Responses.UserType> GetGroupUserTypes()
        {
            List<TVPApiModule.Objects.Responses.UserType> response = null;

            try
            {
                var res = m_Module.GetGroupUserTypes(m_wsUserName, m_wsPassword);
                if (res != null)
                    response = res.Where(ut => ut != null).Select(u => u.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetGroupUserTypes, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", ex.Message, m_wsUserName, m_wsPassword);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject RenewUserPassword(string sUN, string sPass)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;
            try
            {
                var res = m_Module.RenewUserPassword(m_wsUserName, m_wsPassword, sUN, sPass);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol RenewUserPassword, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}", ex.Message, m_wsUserName, m_wsPassword);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.eResponseStatus RenewUserPIN(string sSiteGuid, int ruleID)
        {
            TVPApiModule.Objects.Responses.eResponseStatus response = TVPApiModule.Objects.Responses.eResponseStatus.OK;

            try
            {
                response = (TVPApiModule.Objects.Responses.eResponseStatus)m_Module.SendChangedPinMail(m_wsUserName, m_wsPassword, sSiteGuid, ruleID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol RenewUserPIN, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, sSiteGUID: {3}, ruleID: {4}", ex.Message, m_wsUserName, m_wsPassword, sSiteGuid, ruleID);

                response = TVPApiModule.Objects.Responses.eResponseStatus.ErrorOnSendingMail;
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject ActivateAccountByDomainMaster(string masterUserName, string userName, string token)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            try
            {
                var res = m_Module.ActivateAccountByDomainMaster(m_wsUserName, m_wsPassword, masterUserName, userName, token);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol ActivateAccountByDomainMaster, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, masterUserName: {3}, userName: {4}, token: {5}", 
                    ex.Message, m_wsUserName, m_wsPassword, masterUserName, userName, token);
            }

            return response;
        }

        public bool SendPasswordMail(string userName)
        {
            bool res = false;

            try
            {
                res = m_Module.SendPasswordMail(m_wsUserName, m_wsPassword, userName);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SendPasswordMail, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, userName: {3}",
                    ex.Message, m_wsUserName, m_wsPassword, userName);
            }

            return res;
        }

        public bool AddItemToList(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            bool res = false;

            try
            {
                TVPPro.SiteManager.TvinciPlatform.Users.UserItemList userItemList = new TVPPro.SiteManager.TvinciPlatform.Users.UserItemList()
                {
                    itemObj = itemObjects,
                    itemType = itemType,
                    listType = listType,
                    siteGuid = siteGuid
                    
                };
                res = m_Module.AddItemToList(m_wsUserName, m_wsPassword, userItemList);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol AddItemToList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, siteGuide: {3}",
                    ex.Message, m_wsUserName, m_wsPassword, siteGuid);
            }

            return res;
        }

        public bool RemoveItemFromList(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            bool res = false;

            try
            {
                TVPPro.SiteManager.TvinciPlatform.Users.UserItemList userItemList = new TVPPro.SiteManager.TvinciPlatform.Users.UserItemList()
                {
                    itemObj = itemObjects,
                    itemType = itemType,
                    listType = listType,
                    siteGuid = siteGuid

                };
                res = m_Module.RemoveItemFromList(m_wsUserName, m_wsPassword, userItemList);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol RemoveItemFromList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, siteGuid: {3}",
                    ex.Message, m_wsUserName, m_wsPassword, siteGuid);
            }

            return res;
        }

        public bool UpdateItemInList(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            bool res = false;

            try
            {
                TVPPro.SiteManager.TvinciPlatform.Users.UserItemList userItemList = new TVPPro.SiteManager.TvinciPlatform.Users.UserItemList()
                {
                    itemObj = itemObjects,
                    itemType = itemType,
                    listType = listType,
                    siteGuid = siteGuid

                };
                res = m_Module.UpdateItemInList(m_wsUserName, m_wsPassword, userItemList);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol UpdateItemInList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, siteGuid: {3}",
                    ex.Message, m_wsUserName, m_wsPassword, siteGuid);
            }

            return res;
        }

        public List<TVPApiModule.Objects.Responses.UserItemList> GetItemFromList(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            List<TVPApiModule.Objects.Responses.UserItemList> response = null;

            try
            {
                TVPPro.SiteManager.TvinciPlatform.Users.UserItemList userItemList = new TVPPro.SiteManager.TvinciPlatform.Users.UserItemList()
                {
                    itemObj = itemObjects,
                    itemType = itemType,
                    listType = listType,
                    siteGuid = siteGuid

                };

                var res = m_Module.GetItemFromList(m_wsUserName, m_wsPassword, userItemList);

                if (res != null)
                    response = res.Where(uil => uil != null).Select(u => u.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol GetItemFromList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, siteGuid: {3}",
                    ex.Message, m_wsUserName, m_wsPassword, siteGuid);
            }

            return response;
        }

        public List<TVPApiModule.Objects.Responses.KeyValuePair> IsItemExistsInList(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            List<TVPApiModule.Objects.Responses.KeyValuePair> response = null;
            
            try
            {
                TVPPro.SiteManager.TvinciPlatform.Users.UserItemList userItemList = new TVPPro.SiteManager.TvinciPlatform.Users.UserItemList()
                {
                    itemObj = itemObjects,
                    itemType = itemType,
                    listType = listType,
                    siteGuid = siteGuid

                };
                var res = m_Module.IsItemExistsInList(m_wsUserName, m_wsPassword, userItemList);

                if (res != null)
                    response = res.Where(kv => kv != null).Select(kv => kv.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol IsItemExistsInList, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, siteGuid: {3}",
                    ex.Message, m_wsUserName, m_wsPassword, siteGuid);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.eResponseStatus SetUserTypeByUserID(string sSiteGuid, int userTypeID)
        {
            TVPApiModule.Objects.Responses.eResponseStatus response = TVPApiModule.Objects.Responses.eResponseStatus.OK;
            try
            {
                response = (TVPApiModule.Objects.Responses.eResponseStatus)m_Module.SetUserTypeByUserID(m_wsUserName, m_wsPassword, sSiteGuid, userTypeID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error receive user data Protocol SetUserTypeByUserID, Error Message: {0} Parameters :WS User name : {1} , ws Password: {2}, sSiteGUID: {3}, userTypeID: {4}", ex.Message, m_wsUserName, m_wsPassword, sSiteGuid, userTypeID);
                response = TVPApiModule.Objects.Responses.eResponseStatus.ErrorOnUpdatingUserType;
            }
            return response;
        }
    }
}
