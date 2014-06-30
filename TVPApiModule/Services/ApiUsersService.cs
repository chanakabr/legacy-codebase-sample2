using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.Helper;
using System.Web;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Extentions;
using TVPApiModule.Context;
using TVPApiModule.Manager;

namespace TVPApiModule.Services
{
    public class ApiUsersService : BaseService
    {
        #region Variables
        private readonly ILog logger = LogManager.GetLogger(typeof(ApiUsersService));
        //private TVPPro.SiteManager.TvinciPlatform.Users.UsersService m_Module;

        //private string m_wsUserName;
        //private string m_wsPassword;

        //private int m_groupID;
        //private PlatformType m_platform;

        [Serializable]
        public class LogInResponseData
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
            //m_Module = new TVPPro.SiteManager.TvinciPlatform.Users.UsersService();
            //m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.URL;
            //m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.DefaultUser;
            //m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.DefaultPassword;

            //m_groupID = groupID;
            //m_platform = platform;
        }

        public ApiUsersService()
        {
            // TODO: Complete member initialization
        }
        #endregion C'tor

        #region Properties

        protected TVPPro.SiteManager.TvinciPlatform.Users.UsersService Users
        {
            get
            {
                return (m_Module as TVPPro.SiteManager.TvinciPlatform.Users.UsersService);
            }
        }

        #endregion

        #region Public methods

        public TVPApiModule.Objects.Responses.UserResponseObject ValidateUser(string userName, string password, bool isDoubleLogin)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.CheckUserPassword(m_wsUserName, m_wsPassword, userName, password, isDoubleLogin);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public LogInResponseData SignIn(string sUserName, string sPassword, string sSessionID, string sDeviceID, bool bIsDoubleLogin)
        {
            LogInResponseData loginData = new LogInResponseData();

            loginData = Execute(() =>
                {
                    sDeviceID = string.Empty;
                    sUserName = HttpUtility.UrlDecode(sUserName);
                    TVPApiModule.Objects.Responses.UserResponseObject response = Users.SignIn(m_wsUserName, m_wsPassword, sUserName, sPassword, sSessionID, SiteHelper.GetClientIP(), sDeviceID, bIsDoubleLogin).ToApiObject();

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

                    return loginData;
                }) as LogInResponseData;

            return loginData;
        }

        public TVPApiModule.Services.ApiUsersService.LogInResponseData SignInWithToken(string token, string udid, string sessionId, string ip, int groupId, PlatformType platform)
        {
            TVPApiModule.Services.ApiUsersService.LogInResponseData loginData = new TVPApiModule.Services.ApiUsersService.LogInResponseData();

            loginData = Execute(() =>
            {
                TVPApiModule.Objects.Responses.UserResponseObject userResponse = null;
                bool isSingleLogin = ConfigManager.GetInstance().GetConfig(groupId, platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject response = Users.SignInWithToken(m_wsUserName, m_wsPassword, token, sessionId, ip, udid, isSingleLogin);//.ToApiObject();

                if (response != null)
                {
                    userResponse = response.ToApiObject();
                    if (userResponse != null && response.m_user != null)
                    {
                        loginData.SiteGuid = userResponse.user.site_guid;
                        loginData.DomainID = userResponse.user.domian_id;
                        loginData.LoginStatus = userResponse.resp_status;
                        loginData.UserData = userResponse.user;
                    }
                    else if (userResponse != null)
                    {
                        loginData.LoginStatus = userResponse.resp_status;
                    }
                }

                return loginData;
            }) as LogInResponseData;

            return loginData;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject SignUp(TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.AddNewUser(m_wsUserName, m_wsPassword, userBasicData, userDynamicData, sPassword, sAffiliateCode);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public void SignOut(string sSiteGuid, string sSessionID, string sDeviceID, bool bPreventDoubleLogin)
        {
            Execute(() =>
                {
                    TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject uro = null;
                    uro = Users.SignOut(m_wsUserName, m_wsPassword, sSiteGuid, sSessionID, SiteHelper.GetClientIP(), sDeviceID, bPreventDoubleLogin);
                    return uro;
                });
        }

        public bool IsUserLoggedIn(string sSiteGuid, string sSessionID, string sDeviceID, string sIP, bool bPreventDoubleLogin)
        {
            bool bRet = false;

            bRet = Convert.ToBoolean(Execute(() =>
            {
                TVPPro.SiteManager.TvinciPlatform.Users.UserState response = Users.GetUserInstanceState(m_wsUserName, m_wsPassword, sSiteGuid, sSessionID, sDeviceID, sIP);
                if (response == TVPPro.SiteManager.TvinciPlatform.Users.UserState.Activated || (response == TVPPro.SiteManager.TvinciPlatform.Users.UserState.SingleSignIn && bPreventDoubleLogin) ||
                    (!bPreventDoubleLogin && (response == TVPPro.SiteManager.TvinciPlatform.Users.UserState.SingleSignIn || response == TVPPro.SiteManager.TvinciPlatform.Users.UserState.DoubleSignIn)))
                {
                    bRet = true;
                }

                return bRet;
            }));

            return bRet;
        }

        public bool RemoveUserFavorite(int[] iFavoriteID)
        {
            bool IsRemoved = false;

            IsRemoved = Convert.ToBoolean(Execute(() =>
                {
                    Users.RemoveUserFavorit(m_wsUserName, m_wsPassword, SiteHelper.GetClientIP(), iFavoriteID);
                    IsRemoved = true;

                    return IsRemoved;
                }));

            return IsRemoved;
        }

        public List<FavoriteObject> GetUserFavorites(string sSiteGuid, string sItemType, int iDomainID, string sUDID)
        {
            List<FavoriteObject> retVal = null;

            retVal = Execute(() =>
                {
                    var response = Users.GetUserFavorites(m_wsUserName, m_wsPassword, sSiteGuid, iDomainID, string.Empty, sItemType);

                    if (response != null && response.Length > 0)
                    {
                        retVal = response.Where(f => f != null).Select(f => f.ToApiObject()).ToList();
                        if (retVal != null)
                        {
                            retVal = retVal.OrderByDescending(r => r.update_date.Date).ThenByDescending(r => r.update_date.TimeOfDay).ToList();
                        }
                    }

                    return retVal;
                }) as List<FavoriteObject>;

            return retVal;
        }

        public bool AddUserFavorite(string sSiteGuid, int iDomainID, string sUDID, string sMediaType, string sMediaID, string sExtra)
        {
            bool bRet = false;

            bRet = Convert.ToBoolean(Execute(() =>
                {
                    bRet = Users.AddUserFavorit(m_wsUserName, m_wsPassword, sSiteGuid, iDomainID, sUDID, sMediaType, sMediaID, sExtra);
                    return bRet;
                }));
            
            return bRet;
        }

        public void RemoveUserFavorite(string sSiteGuid, int[] mediaID)
        {
            Execute(() =>
                {
                    Users.RemoveUserFavorit(m_wsUserName, m_wsPassword, sSiteGuid, mediaID);
                    return 0;
                });
        }

        public TVPApiModule.Objects.Responses.UserResponseObject SSOSignIn(string sUserName, string sPassword, int nProviderID, string sSessionID, string sIP, string sDeviceID, bool bIsPreventDoubleLogins)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.SSOSignIn(m_wsUserName, m_wsPassword, sUserName, sPassword, nProviderID, sSessionID, sIP, sDeviceID, bIsPreventDoubleLogins);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject SSOCheckLogin(string sUserName, int nProviderID)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.SSOCheckLogin(m_wsUserName, m_wsPassword, sUserName, nProviderID);
                        if (res != null)
                            response = res.ToApiObject();
                    
                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject GetSSOProviders(string sUserName, int nProviderID)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.SSOCheckLogin(m_wsUserName, m_wsPassword, sUserName, nProviderID);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject GetUserData(string sSiteGuid)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.GetUserData(m_wsUserName, m_wsPassword, sSiteGuid);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public List<TVPApiModule.Objects.Responses.UserResponseObject> GetUsersData(string sSiteGuids)
        {
            List<TVPApiModule.Objects.Responses.UserResponseObject> response = null;

            response = Execute(() =>
                {
                    var res = Users.GetUsersData(m_wsUserName, m_wsPassword, sSiteGuids.Split(';'));
                    if (res != null)
                        response = res.Where(ur => ur != null).Select(u => u.ToApiObject()).ToList();

                    return response;
                }) as List<TVPApiModule.Objects.Responses.UserResponseObject>;

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject SetUserData(string sSiteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.SetUserData(m_wsUserName, m_wsPassword, sSiteGuid, userBasicData, userDynamicData);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject ActivateAccount(string sUserName, string sToken)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.ActivateAccount(m_wsUserName, m_wsPassword, sUserName, sToken);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public bool ResendActivationMail(string sUserName, string sNewPassword)
        {
            bool response = false;

            response = Convert.ToBoolean(Execute(() =>
                {
                    response = Users.ResendActivationMail(m_wsUserName, m_wsPassword, sUserName, sNewPassword);
                    return response;
                }));

            return response;
        }

        #endregion

        public UserOfflineObject[] GetUserOfflineList(string sSiteGuid)
        {
            UserOfflineObject[] response = null;

            response = Execute(() =>
            {
                response = Users.GetAllUserOfflineAssets(m_wsUserName, m_wsPassword, sSiteGuid);
                return response;
            }) as UserOfflineObject[];

            return response;
        }

        public bool AddUserOfflineMedia(string siteGuid, int mediaID)
        {
            bool response = false;

            response = Convert.ToBoolean(Execute(() =>
                {
                    response = Users.AddUserOfflineAsset(m_wsUserName, m_wsPassword, siteGuid, mediaID.ToString());
                    return response;
                }));

            return response;
        }

        public bool RemoveUserOfflineMedia(string siteGuid, int mediaID)
        {
            bool response = false;

            response = Convert.ToBoolean(Execute(() =>
            {
                response = Users.RemoveUserOfflineAsset(m_wsUserName, m_wsPassword, siteGuid, mediaID.ToString());
                return response;
            }));

            return response;
        }

        public bool ClearUserOfflineList(string siteGuid)
        {
            bool response = false;

            response = Convert.ToBoolean(Execute(() =>
                {
                    response = Users.ClearUserOfflineAssets(m_wsUserName, m_wsPassword, siteGuid);
                    return response;
                }));

            return response;
        }

        public bool SentNewPasswordToUser(string UserName)
        {
            bool returnedValue = false;

            returnedValue = Convert.ToBoolean(Execute(() =>
                {
                    TVPApiModule.Objects.Responses.UserResponseObject uro = Users.ForgotPassword(m_wsUserName, m_wsPassword, UserName).ToApiObject();
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
                }));

            return returnedValue;
        }

        public string IpToCountry(string sIP)
        {
            string sRet = string.Empty;

            sRet = Execute(() =>
                {
                    TVPPro.SiteManager.TvinciPlatform.Users.Country response = Users.GetIPToCountry(m_wsUserName, m_wsPassword, sIP);
                    sRet = response.m_sCountryName;

                    return sRet;
                }) as string;

            return sRet;
        }

        public bool IsOfflineModeEnabled(string siteGuid)
        {
            bool isOfflineModeEnabled = false;

            isOfflineModeEnabled = Convert.ToBoolean(Execute(() =>
                {
                    var offlineMode = GetUserData(siteGuid).user.dynamic_data.user_data.Where(x => x.data_type == "IsOfflineMode" && x.value == "true").FirstOrDefault();

                    if (offlineMode == null)
                        return false;

                    if (offlineMode.value == "false")
                        return false;

                    return true;
                }));

            return isOfflineModeEnabled;
        }

        private TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData cloneDynamicData(TVPApiModule.Objects.Responses.UserDynamicData curDynamicData, bool isAddNew)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData newDynamicData = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData();

            newDynamicData = Execute(() =>
                {
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
                }) as TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData;

            return newDynamicData;
        }

        public void ToggleOfflineMode(string siteGUID, bool isTurnOn)
        {
            Execute(() =>
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

                    return 0;
                });
        }

        public bool SetUserDynamicData(string sSiteGuid, string sKey, string sValue)
        {
            bool bRet = false;

            bRet = Convert.ToBoolean(Execute(() =>
                {
                    bRet = Users.SetUserDynamicData(m_wsUserName, m_wsPassword, sSiteGuid, sKey, sValue);
                    return bRet;
                }));

            return bRet;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject GetUserDataByCoGuid(string coGuid, int operatorID)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.GetUserDataByCoGuid(m_wsUserName, m_wsPassword, coGuid, operatorID);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject ChangeUserPassword(string sUN, string sOldPass, string sPass)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.ChangeUserPassword(m_wsUserName, m_wsPassword, sUN, sOldPass, sPass);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject GetUserByFacebookID(string facebookId)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.GetUserByFacebookID(m_wsUserName, m_wsPassword, facebookId);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject GetUserByUsername(string userName)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.GetUserByUsername(m_wsUserName, m_wsPassword, userName);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public List<TVPApiModule.Objects.Responses.UserBasicData> SearchUsers(string[] sTerms, string[] sFields, bool bIsExact)
        {
            List<TVPApiModule.Objects.Responses.UserBasicData> response = null;

            response = Execute(() =>
                {
                    var res = Users.SearchUsers(m_wsUserName, m_wsPassword, sTerms, sFields, bIsExact);
                    if (res != null)
                        response = res.Where(ubd => ubd != null).Select(u => u.ToApiObject()).ToList();

                    return response;
                }) as List<TVPApiModule.Objects.Responses.UserBasicData>;

            return response;
        }

        public void Logout(string sSiteGuid)
        {
            Execute(() =>
            {
                Users.Logout(m_wsUserName, m_wsPassword, sSiteGuid);
                return 0;
            });
        }

        public List<TVPApiModule.Objects.Responses.Country> GetCountriesList()
        {
            List<TVPApiModule.Objects.Responses.Country> response = null;

            response = Execute(() =>
                {
                    var res = Users.GetCountryList(m_wsUserName, m_wsPassword);
                    if (res != null)
                        response = res.Where(c => c != null).Select(c => c.ToApiObject()).ToList();

                    return response;
                }) as List<TVPApiModule.Objects.Responses.Country>;

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject CheckTemporaryToken(string sToken)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.CheckTemporaryToken(m_wsUserName, m_wsPassword, sToken);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public List<TVPApiModule.Objects.Responses.UserType> GetGroupUserTypes()
        {
            List<TVPApiModule.Objects.Responses.UserType> response = null;

            response = Execute(() =>
                {
                    var res = Users.GetGroupUserTypes(m_wsUserName, m_wsPassword);
                    if (res != null)
                        response = res.Where(ut => ut != null).Select(u => u.ToApiObject()).ToList();

                    return response;
                }) as List<TVPApiModule.Objects.Responses.UserType>;

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject RenewUserPassword(string sUN, string sPass)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
            {
                var res = Users.RenewUserPassword(m_wsUserName, m_wsPassword, sUN, sPass);
                if (res != null)
                    response = res.ToApiObject();

                return response;
            }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public TVPApiModule.Objects.Responses.eResponseStatus RenewUserPIN(string sSiteGuid, int ruleID)
        {
            TVPApiModule.Objects.Responses.eResponseStatus response = TVPApiModule.Objects.Responses.eResponseStatus.ErrorOnSendingMail;

            response = (eResponseStatus)Enum.Parse(typeof(eResponseStatus), Execute(() =>
                {
                    response = (TVPApiModule.Objects.Responses.eResponseStatus)Users.SendChangedPinMail(m_wsUserName, m_wsPassword, sSiteGuid, ruleID);
                    return response;
                }).ToString());

            return response;
        }

        public TVPApiModule.Objects.Responses.UserResponseObject ActivateAccountByDomainMaster(string masterUserName, string userName, string token)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.ActivateAccountByDomainMaster(m_wsUserName, m_wsPassword, masterUserName, userName, token);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.UserResponseObject;

            return response;
        }

        public bool SendPasswordMail(string userName)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Users.SendPasswordMail(m_wsUserName, m_wsPassword, userName);
                    return res;
                }));

            return res;
        }

        public bool AddItemToList(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    TVPPro.SiteManager.TvinciPlatform.Users.UserItemList userItemList = new TVPPro.SiteManager.TvinciPlatform.Users.UserItemList()
                        {
                            itemObj = itemObjects,
                            itemType = itemType,
                            listType = listType,
                            siteGuid = siteGuid

                        };
                    res = Users.AddItemToList(m_wsUserName, m_wsPassword, userItemList);

                    return res;
                }));

            return res;
        }

        public bool RemoveItemFromList(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    TVPPro.SiteManager.TvinciPlatform.Users.UserItemList userItemList = new TVPPro.SiteManager.TvinciPlatform.Users.UserItemList()
                        {
                            itemObj = itemObjects,
                            itemType = itemType,
                            listType = listType,
                            siteGuid = siteGuid

                        };
                    res = Users.RemoveItemFromList(m_wsUserName, m_wsPassword, userItemList);

                    return res;
                }));

            return res;
        }

        public bool UpdateItemInList(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    TVPPro.SiteManager.TvinciPlatform.Users.UserItemList userItemList = new TVPPro.SiteManager.TvinciPlatform.Users.UserItemList()
                        {
                            itemObj = itemObjects,
                            itemType = itemType,
                            listType = listType,
                            siteGuid = siteGuid

                        };
                    res = Users.UpdateItemInList(m_wsUserName, m_wsPassword, userItemList);

                    return res;
                }));

            return res;
        }

        public List<TVPApiModule.Objects.Responses.UserItemList> GetItemFromList(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            List<TVPApiModule.Objects.Responses.UserItemList> response = null;

            response = Execute(() =>
                {
                    TVPPro.SiteManager.TvinciPlatform.Users.UserItemList userItemList = new TVPPro.SiteManager.TvinciPlatform.Users.UserItemList()
                                   {
                                       itemObj = itemObjects,
                                       itemType = itemType,
                                       listType = listType,
                                       siteGuid = siteGuid

                                   };

                    var res = Users.GetItemFromList(m_wsUserName, m_wsPassword, userItemList);

                    if (res != null)
                        response = res.Where(uil => uil != null).Select(u => u.ToApiObject()).ToList();

                    return response;
                }) as List<TVPApiModule.Objects.Responses.UserItemList>;

            return response;
        }

        public List<TVPApiModule.Objects.KeyValuePair> IsItemExistsInList(string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            List<TVPApiModule.Objects.KeyValuePair> response = null;

            response = Execute(() =>
                {
                    TVPPro.SiteManager.TvinciPlatform.Users.UserItemList userItemList = new TVPPro.SiteManager.TvinciPlatform.Users.UserItemList()
                        {
                            itemObj = itemObjects,
                            itemType = itemType,
                            listType = listType,
                            siteGuid = siteGuid

                        };

                    var res = Users.IsItemExistsInList(m_wsUserName, m_wsPassword, userItemList);
                    if (res != null)
                        response = res.Where(kv => kv != null).Select(kv => kv.ToApiObject()).ToList();

                    return response;
                }) as List<TVPApiModule.Objects.KeyValuePair>;

            return response;
        }

        public TVPApiModule.Objects.Responses.eResponseStatus SetUserTypeByUserID(string sSiteGuid, int userTypeID)
        {
            TVPApiModule.Objects.Responses.eResponseStatus response = TVPApiModule.Objects.Responses.eResponseStatus.OK;

            response = (eResponseStatus)Enum.Parse(typeof(eResponseStatus), Execute(() => 
                {
                    response = (TVPApiModule.Objects.Responses.eResponseStatus)Users.SetUserTypeByUserID(m_wsUserName, m_wsPassword, sSiteGuid, userTypeID);
                    return response;
                }).ToString());
            
            return response;
        }
    }
}
