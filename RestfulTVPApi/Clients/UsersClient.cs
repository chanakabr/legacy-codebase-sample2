using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.Objects.Extentions;
using RestfulTVPApi.Users;
using TVPPro.SiteManager.Helper;
using RestfulTVPApi.Clients.ClientsCache;

namespace RestfulTVPApi.Clients
{
    public class UsersClient : BaseClient
    {
         #region Variables
        private readonly ILog logger = LogManager.GetLogger(typeof(UsersClient));
       

        [Serializable]
        public class LogInResponseData
        {
            public string SiteGuid { get; set; }
            public int DomainID { get; set; }
            public RestfulTVPApi.Objects.Responses.Enums.eResponseStatus LoginStatus { get; set; }
            public RestfulTVPApi.Objects.Responses.User UserData { get; set; }
        }
        #endregion

        #region C'tor
        public UsersClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
                    
        }

        public UsersClient()
        {
            // TODO: Complete member initialization
        }
        #endregion C'tor

        #region Properties

        protected RestfulTVPApi.Users.UsersService Users
        {
            get
            {
                return (Module as RestfulTVPApi.Users.UsersService);
            }
        }

        #endregion

        #region Public methods

        public RestfulTVPApi.Objects.Responses.UserResponseObject ValidateUser(string userName, string password, bool isDoubleLogin)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.CheckUserPassword(WSUserName, WSPassword, userName, password, isDoubleLogin);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public LogInResponseData SignIn(string sUserName, string sPassword, string sSessionID, string sDeviceID, bool bIsDoubleLogin)
        {
            LogInResponseData loginData = new LogInResponseData();

            loginData = Execute(() =>
                {                    
                    sDeviceID = string.Empty;
                    sUserName = HttpUtility.UrlDecode(sUserName);
                    RestfulTVPApi.Objects.Responses.UserResponseObject response = Users.SignIn(WSUserName, WSPassword, sUserName, sPassword, sSessionID, SiteHelper.GetClientIP(), sDeviceID, bIsDoubleLogin).ToApiObject();

                    if (response != null && response.user != null)
                    {
                        loginData.SiteGuid = response.user.site_guid;
                        loginData.DomainID = response.user.domain_id;
                        loginData.LoginStatus = response.resp_status;
                        loginData.UserData = response.user;
                    }
                    else if (response != null)
                    {
                        loginData.LoginStatus = response.resp_status;
                        loginData.SiteGuid = string.Empty;
                    }

                    return loginData;
                }) as LogInResponseData;

            return loginData;
        }

        public RestfulTVPApi.Clients.UsersClient.LogInResponseData SignInWithToken(string token, string udid, string sessionId, string ip, int groupId, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            RestfulTVPApi.Clients.UsersClient.LogInResponseData loginData = new RestfulTVPApi.Clients.UsersClient.LogInResponseData();

            loginData = Execute(() =>
            {
                RestfulTVPApi.Objects.Responses.UserResponseObject userResponse = null;
                bool isSingleLogin = TVPApiModule.Manager.ConfigManager.GetInstance().GetConfig(groupId, (TVPApiModule.Context.PlatformType)platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                RestfulTVPApi.Users.UserResponseObject response = Users.SignInWithToken(WSUserName, WSPassword, token, sessionId, ip, udid, isSingleLogin);//.ToApiObject();

                if (response != null)
                {
                    userResponse = response.ToApiObject();
                    if (userResponse != null && response.m_user != null)
                    {
                        loginData.SiteGuid = userResponse.user.site_guid;
                        loginData.DomainID = userResponse.user.domain_id;
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

        public RestfulTVPApi.Objects.Responses.UserResponseObject SignUp(RestfulTVPApi.Users.UserBasicData userBasicData, RestfulTVPApi.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.AddNewUser(WSUserName, WSPassword, userBasicData, userDynamicData, sPassword, sAffiliateCode);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public void SignOut(string sSiteGuid, string sSessionID, string sDeviceID, bool bPreventDoubleLogin)
        {
            Execute(() =>
                {
                    RestfulTVPApi.Users.UserResponseObject uro = null;
                    uro = Users.SignOut(WSUserName, WSPassword, sSiteGuid, sSessionID, SiteHelper.GetClientIP(), sDeviceID, bPreventDoubleLogin);
                    return uro;
                });
        }

        public bool IsUserLoggedIn(string sSiteGuid, string sSessionID, string sDeviceID, string sIP, bool bPreventDoubleLogin)
        {
            bool bRet = false;

            bRet = Convert.ToBoolean(Execute(() =>
            {
                RestfulTVPApi.Users.UserState response = Users.GetUserInstanceState(WSUserName, WSPassword, sSiteGuid, sSessionID, sDeviceID, sIP);
                if (response == RestfulTVPApi.Users.UserState.Activated || (response == RestfulTVPApi.Users.UserState.SingleSignIn && bPreventDoubleLogin) ||
                    (!bPreventDoubleLogin && (response == RestfulTVPApi.Users.UserState.SingleSignIn || response == RestfulTVPApi.Users.UserState.DoubleSignIn)))
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
                    Users.RemoveUserFavorit(WSUserName, WSPassword, SiteHelper.GetClientIP(), iFavoriteID);
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
                    var response = Users.GetUserFavorites(WSUserName, WSPassword, sSiteGuid, iDomainID, string.Empty, sItemType);

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
                    bRet = Users.AddUserFavorit(WSUserName, WSPassword, sSiteGuid, iDomainID, sUDID, sMediaType, sMediaID, sExtra);
                    return bRet;
                }));
            
            return bRet;
        }

        public void RemoveUserFavorite(string sSiteGuid, int[] mediaID)
        {
            Execute(() =>
                {
                    Users.RemoveUserFavorit(WSUserName, WSPassword, sSiteGuid, mediaID);
                    return 0;
                });
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject SSOSignIn(string sUserName, string sPassword, int nProviderID, string sSessionID, string sIP, string sDeviceID, bool bIsPreventDoubleLogins)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.SSOSignIn(WSUserName, WSPassword, sUserName, sPassword, nProviderID, sSessionID, sIP, sDeviceID, bIsPreventDoubleLogins);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject SSOCheckLogin(string sUserName, int nProviderID)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.SSOCheckLogin(WSUserName, WSPassword, sUserName, nProviderID);
                        if (res != null)
                            response = res.ToApiObject();
                    
                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject GetSSOProviders(string sUserName, int nProviderID)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.SSOCheckLogin(WSUserName, WSPassword, sUserName, nProviderID);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject GetUserData(string sSiteGuid)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.GetUserData(WSUserName, WSPassword, sSiteGuid);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public List<RestfulTVPApi.Objects.Responses.UserResponseObject> GetUsersData(string sSiteGuids)
        {
            List<RestfulTVPApi.Objects.Responses.UserResponseObject> response = null;

            response = Execute(() =>
                {
                    var res = Users.GetUsersData(WSUserName, WSPassword, sSiteGuids.Split(';'));
                    if (res != null)
                        response = res.Where(ur => ur != null).Select(u => u.ToApiObject()).ToList();

                    return response;
                }) as List<RestfulTVPApi.Objects.Responses.UserResponseObject>;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject SetUserData(string sSiteGuid, RestfulTVPApi.Objects.Responses.UserBasicData userBasicData, RestfulTVPApi.Objects.Responses.UserDynamicData userDynamicData)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.SetUserData(WSUserName, WSPassword, sSiteGuid, userBasicData.ToTvmObject(), userDynamicData.ToTvmObject());
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject ActivateAccount(string sUserName, string sToken)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.ActivateAccount(WSUserName, WSPassword, sUserName, sToken);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public bool ResendActivationMail(string sUserName, string sNewPassword)
        {
            bool response = false;

            response = Convert.ToBoolean(Execute(() =>
                {
                    response = Users.ResendActivationMail(WSUserName, WSPassword, sUserName, sNewPassword);
                    return response;
                }));

            return response;
        }

        public UserOfflineObject[] GetUserOfflineList(string sSiteGuid)
        {
            UserOfflineObject[] response = null;

            response = Execute(() =>
            {
                response = Users.GetAllUserOfflineAssets(WSUserName, WSPassword, sSiteGuid);
                return response;
            }) as UserOfflineObject[];

            return response;
        }

        public bool AddUserOfflineMedia(string siteGuid, int mediaID)
        {
            bool response = false;

            response = Convert.ToBoolean(Execute(() =>
                {
                    response = Users.AddUserOfflineAsset(WSUserName, WSPassword, siteGuid, mediaID.ToString());
                    return response;
                }));

            return response;
        }

        public bool RemoveUserOfflineMedia(string siteGuid, int mediaID)
        {
            bool response = false;

            response = Convert.ToBoolean(Execute(() =>
            {
                response = Users.RemoveUserOfflineAsset(WSUserName, WSPassword, siteGuid, mediaID.ToString());
                return response;
            }));

            return response;
        }

        public bool ClearUserOfflineList(string siteGuid)
        {
            bool response = false;

            response = Convert.ToBoolean(Execute(() =>
                {
                    response = Users.ClearUserOfflineAssets(WSUserName, WSPassword, siteGuid);
                    return response;
                }));

            return response;
        }

        public Status SentNewPasswordToUser(string UserName)
        {
            Status statusResult;

            statusResult = Execute(() =>
                {
                    RestfulTVPApi.Objects.Responses.UserResponseObject uro = Users.ForgotPassword(WSUserName, WSPassword, UserName).ToApiObject();
                    statusResult = new Status();
                    if (uro.resp_status == RestfulTVPApi.Objects.Responses.Enums.eResponseStatus.OK)
                    {
                        logger.InfoFormat("Sent new temp password protocol ForgotPassword, Parameters : User name {0}: ", UserName);
                        statusResult.status = StatusObjectCode.OK;
                        return statusResult;
                    }
                    else
                    {
                        logger.InfoFormat("Can not send temp password protocol CheckUserPassword,Parameters : User name : {0}", UserName);
                        statusResult.status = StatusObjectCode.Fail;
                        return statusResult;
                    }
                }) as Status;

            return statusResult;
        }

        public string IpToCountry(string sIP)
        {
            string sRet = string.Empty;

            sRet = Execute(() =>
                {
                    RestfulTVPApi.Users.Country response = Users.GetIPToCountry(WSUserName, WSPassword, sIP);
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

        private RestfulTVPApi.Users.UserDynamicData cloneDynamicData(RestfulTVPApi.Objects.Responses.UserDynamicData curDynamicData, bool isAddNew)
        {
            RestfulTVPApi.Users.UserDynamicData newDynamicData = new RestfulTVPApi.Users.UserDynamicData();

            newDynamicData = Execute(() =>
                {
                    RestfulTVPApi.Users.UserDynamicDataContainer dData;
                    newDynamicData.m_sUserData = new RestfulTVPApi.Users.UserDynamicDataContainer[curDynamicData.user_data.Count() + (isAddNew ? 1 : 0)];
                    int idx = 0;

                    foreach (var UserData in curDynamicData.user_data)
                    {
                        dData = new RestfulTVPApi.Users.UserDynamicDataContainer();
                        dData.m_sDataType = UserData.data_type;
                        dData.m_sValue = UserData.value;
                        newDynamicData.m_sUserData[idx] = dData;
                        idx++;
                    }

                    return newDynamicData;
                }) as RestfulTVPApi.Users.UserDynamicData;

            return newDynamicData;
        }

        public void ToggleOfflineMode(string siteGUID, bool isTurnOn)
        {
            Execute(() =>
                {
                    RestfulTVPApi.Users.UserDynamicData newDynamicData;
                    RestfulTVPApi.Objects.Responses.UserResponseObject userData;
                    RestfulTVPApi.Objects.Responses.UserDynamicData curDynamicData;

                    if (isTurnOn)
                    {

                        userData = GetUserData(siteGUID);
                        curDynamicData = userData.user.dynamic_data;
                        var isOfflineMode = curDynamicData.user_data.Where(x => x != null && x.data_type == "IsOfflineMode").Count() > 0;
                        newDynamicData = cloneDynamicData(curDynamicData, !isOfflineMode);

                        if (!isOfflineMode)
                        {
                            RestfulTVPApi.Users.UserDynamicDataContainer dData = new RestfulTVPApi.Users.UserDynamicDataContainer();
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

                    RestfulTVPApi.Users.UserBasicData userBasicData = new RestfulTVPApi.Users.UserBasicData()
                    {
                        m_bIsFacebookImagePermitted = userData.user.basic_data.is_facebook_image_permitted,
                        m_CoGuid = userData.user.basic_data.co_guid,
                        m_Country = new RestfulTVPApi.Users.Country()
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
                        m_State = new RestfulTVPApi.Users.State()
                        {
                            m_Country = new RestfulTVPApi.Users.Country()
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
                        m_UserType = new RestfulTVPApi.Users.UserType()
                        {
                            Description = userData.user.basic_data.user_type.description,
                            ID = userData.user.basic_data.user_type.id,
                            IsDefault = userData.user.basic_data.user_type.is_default
                        }
                    };

                    SetUserData(siteGUID, userBasicData.ToApiObject(), newDynamicData.ToApiObject());

                    return 0;
                });
        }

        public bool SetUserDynamicData(string sSiteGuid, string sKey, string sValue)
        {
            bool bRet = false;

            bRet = Convert.ToBoolean(Execute(() =>
                {
                    bRet = Users.SetUserDynamicData(WSUserName, WSPassword, sSiteGuid, sKey, sValue);
                    return bRet;
                }));

            return bRet;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject GetUserDataByCoGuid(string coGuid, int operatorID)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.GetUserDataByCoGuid(WSUserName, WSPassword, coGuid, operatorID);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject ChangeUserPassword(string sUN, string sOldPass, string sPass)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.ChangeUserPassword(WSUserName, WSPassword, sUN, sOldPass, sPass);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject GetUserByFacebookID(string facebookId)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.GetUserByFacebookID(WSUserName, WSPassword, facebookId);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject GetUserByUsername(string userName)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.GetUserByUsername(WSUserName, WSPassword, userName);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public List<RestfulTVPApi.Objects.Responses.UserBasicData> SearchUsers(string[] sTerms, string[] sFields, bool bIsExact)
        {
            List<RestfulTVPApi.Objects.Responses.UserBasicData> response = null;

            response = Execute(() =>
                {
                    var res = Users.SearchUsers(WSUserName, WSPassword, sTerms, sFields, bIsExact);
                    if (res != null)
                        response = res.Where(ubd => ubd != null).Select(u => u.ToApiObject()).ToList();

                    return response;
                }) as List<RestfulTVPApi.Objects.Responses.UserBasicData>;

            return response;
        }

        public void Logout(string sSiteGuid)
        {
            Execute(() =>
            {
                Users.Logout(WSUserName, WSPassword, sSiteGuid);
                return 0;
            });
        }

        public List<RestfulTVPApi.Objects.Responses.Country> GetCountriesList()
        {
            List<RestfulTVPApi.Objects.Responses.Country> response = null;

            response = Execute(() =>
                {
                    var res = Users.GetCountryList(WSUserName, WSPassword);
                    if (res != null)
                        response = res.Where(c => c != null).Select(c => c.ToApiObject()).ToList();

                    return response;
                }) as List<RestfulTVPApi.Objects.Responses.Country>;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject CheckTemporaryToken(string sToken)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.CheckTemporaryToken(WSUserName, WSPassword, sToken);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public List<RestfulTVPApi.Objects.Responses.UserType> GetGroupUserTypes()
        {
            List<RestfulTVPApi.Objects.Responses.UserType> response = null;

            response = Execute(() =>
                {
                    var res = Users.GetGroupUserTypes(WSUserName, WSPassword);
                    if (res != null)
                        response = res.Where(ut => ut != null).Select(u => u.ToApiObject()).ToList();

                    return response;
                }) as List<RestfulTVPApi.Objects.Responses.UserType>;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject RenewUserPassword(string sUN, string sPass)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
            {
                var res = Users.RenewUserPassword(WSUserName, WSPassword, sUN, sPass);
                if (res != null)
                    response = res.ToApiObject();

                return response;
            }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.Enums.eResponseStatus RenewUserPIN(string sSiteGuid, int ruleID)
        {
            RestfulTVPApi.Objects.Responses.Enums.eResponseStatus response = RestfulTVPApi.Objects.Responses.Enums.eResponseStatus.ErrorOnSendingMail;

            response = (eResponseStatus)Enum.Parse(typeof(eResponseStatus), Execute(() =>
                {
                    response = (RestfulTVPApi.Objects.Responses.Enums.eResponseStatus)Users.SendChangedPinMail(WSUserName, WSPassword, sSiteGuid, ruleID);
                    return response;
                }).ToString());

            return response;
        }

        public RestfulTVPApi.Objects.Responses.UserResponseObject ActivateAccountByDomainMaster(string masterUserName, string userName, string token)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Users.ActivateAccountByDomainMaster(WSUserName, WSPassword, masterUserName, userName, token);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.UserResponseObject;

            return response;
        }

        public bool SendPasswordMail(string userName)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Users.SendPasswordMail(WSUserName, WSPassword, userName);
                    return res;
                }));

            return res;
        }

        public bool AddItemToList(string siteGuid, RestfulTVPApi.Users.ItemObj[] itemObjects, RestfulTVPApi.Users.ItemType itemType, RestfulTVPApi.Users.ListType listType)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    RestfulTVPApi.Users.UserItemList userItemList = new RestfulTVPApi.Users.UserItemList()
                        {
                            itemObj = itemObjects,
                            itemType = itemType,
                            listType = listType,
                            siteGuid = siteGuid

                        };
                    res = Users.AddItemToList(WSUserName, WSPassword, userItemList);

                    return res;
                }));

            return res;
        }

        public bool RemoveItemFromList(string siteGuid, RestfulTVPApi.Users.ItemObj[] itemObjects, RestfulTVPApi.Users.ItemType itemType, RestfulTVPApi.Users.ListType listType)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    RestfulTVPApi.Users.UserItemList userItemList = new RestfulTVPApi.Users.UserItemList()
                        {
                            itemObj = itemObjects,
                            itemType = itemType,
                            listType = listType,
                            siteGuid = siteGuid

                        };
                    res = Users.RemoveItemFromList(WSUserName, WSPassword, userItemList);

                    return res;
                }));

            return res;
        }

        public bool UpdateItemInList(string siteGuid, RestfulTVPApi.Users.ItemObj[] itemObjects, RestfulTVPApi.Users.ItemType itemType, RestfulTVPApi.Users.ListType listType)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    RestfulTVPApi.Users.UserItemList userItemList = new RestfulTVPApi.Users.UserItemList()
                        {
                            itemObj = itemObjects,
                            itemType = itemType,
                            listType = listType,
                            siteGuid = siteGuid

                        };
                    res = Users.UpdateItemInList(WSUserName, WSPassword, userItemList);

                    return res;
                }));

            return res;
        }

        public List<RestfulTVPApi.Objects.Responses.UserItemList> GetItemFromList(string siteGuid, RestfulTVPApi.Users.ItemObj[] itemObjects, RestfulTVPApi.Users.ItemType itemType, RestfulTVPApi.Users.ListType listType)
        {
            List<RestfulTVPApi.Objects.Responses.UserItemList> response = null;

            response = Execute(() =>
                {
                    RestfulTVPApi.Users.UserItemList userItemList = new RestfulTVPApi.Users.UserItemList()
                                   {
                                       itemObj = itemObjects,
                                       itemType = itemType,
                                       listType = listType,
                                       siteGuid = siteGuid

                                   };

                    var res = Users.GetItemFromList(WSUserName, WSPassword, userItemList);

                    if (res != null)
                        response = res.Where(uil => uil != null).Select(u => u.ToApiObject()).ToList();

                    return response;
                }) as List<RestfulTVPApi.Objects.Responses.UserItemList>;

            return response;
        }

        public List<RestfulTVPApi.Objects.Responses.KeyValuePair> IsItemExistsInList(string siteGuid, RestfulTVPApi.Users.ItemObj[] itemObjects, RestfulTVPApi.Users.ItemType itemType, RestfulTVPApi.Users.ListType listType)
        {
            List<RestfulTVPApi.Objects.Responses.KeyValuePair> response = null;

            response = Execute(() =>
                {
                    RestfulTVPApi.Users.UserItemList userItemList = new RestfulTVPApi.Users.UserItemList()
                        {
                            itemObj = itemObjects,
                            itemType = itemType,
                            listType = listType,
                            siteGuid = siteGuid

                        };

                    var res = Users.IsItemExistsInList(WSUserName, WSPassword, userItemList);
                    if (res != null)
                        response = res.Where(kv => kv != null).Select(kv => kv.ToApiObject()).ToList();

                    return response;
                }) as List<RestfulTVPApi.Objects.Responses.KeyValuePair>;

            return response;
        }

        public RestfulTVPApi.Objects.Responses.Enums.eResponseStatus SetUserTypeByUserID(string sSiteGuid, int userTypeID)
        {
            RestfulTVPApi.Objects.Responses.Enums.eResponseStatus response = RestfulTVPApi.Objects.Responses.Enums.eResponseStatus.OK;

            response = (eResponseStatus)Enum.Parse(typeof(eResponseStatus), Execute(() => 
                {
                    response = (RestfulTVPApi.Objects.Responses.Enums.eResponseStatus)Users.SetUserTypeByUserID(WSUserName, WSPassword, sSiteGuid, userTypeID);
                    return response;
                }).ToString());
            
            return response;
        }
        #endregion
    }
}