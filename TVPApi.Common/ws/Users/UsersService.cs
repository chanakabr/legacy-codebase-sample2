using ApiObjects;
using Core.Users;
using KLogMonitor;
using System;
using System.Configuration;
using System.Reflection;
using System.Web;
using TVPApi;
using TVPApiModule.Interfaces;
using TVPApiModule.Manager;
using TVPApiModule.Objects.Authorization;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using ClientResponseStatus = TVPApiModule.Objects.Responses.ClientResponseStatus;
using Country = Core.Users.Country;
using InitializationObject = TVPApi.InitializationObject;
using TVinciShared;
using TVPApiModule.Objects.CRM;
using ConfigurationManager;
using KeyValuePair = ApiObjects.KeyValuePair;

namespace TVPApiServices
{
    [System.ComponentModel.ToolboxItem(false)]
    public class UsersService : IUsersService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region public methods

        [PrivateMethod]
        public UserResponseObjectDTO ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass)
        {
            UserResponseObjectDTO response = new UserResponseObjectDTO();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    var res = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ChangeUserPassword(sUN, sOldPass, sPass);
                    response = UserResponseObjectDTO.ConvertToDTO(res);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error on ChangeUserPasword for username = {sUN}. ex = {ex}");
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public UserResponseObject RenewUserPassword(InitializationObject initObj, string sUN, string sPass)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RenewUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RenewUserPassword(sUN, sPass);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [PrivateMethod]
        public UserResponseObject GetUserByFacebookID(InitializationObject initObj, string facebookId)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserByFacebookID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserByFacebookID(facebookId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [PrivateMethod]
        public UserResponseObject GetUserByUsername(InitializationObject initObj, string userName)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserByUsername", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserByUsername(userName);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [PrivateMethod]
        public void Logout(InitializationObject initObj, string sSiteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "Logout", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sSiteGuid, 0, null, groupID, initObj.Platform))
                {
                    return;
                }
                try
                {
                    string siteGuid = string.IsNullOrEmpty(sSiteGuid) ? initObj.SiteGuid : sSiteGuid;
                    new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).Logout(siteGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
        }

        [PrivateMethod]
        public UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActivateAccount", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ActivateAccount(sUserName, sToken);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public bool ResendActivationMail(InitializationObject initObj, string sUserName, string sNewPassword)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ResendActivationMail", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ResendActivationMail(sUserName, sNewPassword);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public TVPApiModule.Services.ApiUsersService.LogInResponseData SignInSecure(InitializationObject initObj, string sUsername, string sEncryptedPassword)
        {
            ApiUsersService.LogInResponseData response = new ApiUsersService.LogInResponseData();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignInSecure", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    SiteService siteSvc = new SiteService();
                    string privateKey = ApplicationConfiguration.Current.TVPApiConfiguration.SecureSiteGuidKey.Value;
                    string IV = ApplicationConfiguration.Current.TVPApiConfiguration.SecureSiteGuidIV.Value;
                    string sClearPassword = SecurityHelper.DecryptSiteGuid(privateKey, IV, sEncryptedPassword);

                    response = siteSvc.SignIn(initObj, sUsername, sClearPassword);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public Country[] GetCountriesList(InitializationObject initObj)
        {
            Country[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetCountriesList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetCountriesList();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public string CheckTemporaryToken(InitializationObject initObj, string sToken)
        {
            string response = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CheckTemporaryToken", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    UserResponseObject userResponseObject = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).CheckTemporaryToken(sToken);

                    if (userResponseObject != null && userResponseObject.m_RespStatus == ResponseStatus.OK)
                    {
                        response = userResponseObject.m_user.m_oBasicData.m_sUserName;
                    }
                    else
                    {
                        HttpContext.Current.Items["Error"] = "Temporary token is invalid Protocol CheckTemporaryToken";
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public UserType[] GetGroupUserTypes(InitializationObject initObj)
        {
            UserType[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupUserTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetGroupUserTypes();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [PrivateMethod]
        public string RenewUserPIN(InitializationObject initObj, string sSiteGUID, int ruleID)
        {
            ResponseStatus response = ResponseStatus.OK;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RenewUserPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sSiteGUID, 0, null, groupID, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RenewUserPIN(sSiteGUID, ruleID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response.ToString();
        }

        [PrivateMethod]
        public UserResponseObject ActivateAccountByDomainMaster(InitializationObject initObj, string masterUserName, string userName, string token)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActivateAccountByDomainMaster", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ActivateAccountByDomainMaster(masterUserName, userName, token);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public bool SendPasswordMail(InitializationObject initObj, string userName)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SendPasswordMail", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SendPasswordMail(userName);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [PrivateMethod]
        public bool AddItemToList(InitializationObject initObj, ItemObj[] itemObjects, ListItemType itemType, ListType listType)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddItemToList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).AddItemToList(initObj.SiteGuid, itemObjects, itemType, listType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [PrivateMethod]
        public bool RemoveItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ListItemType itemType, ListType listType)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveItemFromList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RemoveItemFromList(initObj.SiteGuid, itemObjects, itemType, listType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [PrivateMethod]
        public bool UpdateItemInList(InitializationObject initObj, ItemObj[] itemObjects, ListItemType itemType, ListType listType)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "UpdateItemInList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).UpdateItemInList(initObj.SiteGuid, itemObjects, itemType, listType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        public UserItemList[] GetItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ListItemType itemType, ListType listType)
        {
            UserItemList[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetItemFromList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetItemFromList(initObj.SiteGuid, itemObjects, itemType, listType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [PrivateMethod]
        public KeyValuePair[] IsItemExistsInList(InitializationObject initObj, ItemObj[] itemObjects, ListItemType itemType, ListType listType)
        {
            KeyValuePair[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsItemExistsInList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IsItemExistsInList(initObj.SiteGuid, itemObjects, itemType, listType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [PrivateMethod]
        public string SetUserTypeByUserID(InitializationObject initObj, string sSiteGUID, int nUserTypeID)
        {
            ResponseStatus response = ResponseStatus.OK;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserTypeByUserID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    // Tokenization: validate siteGuid
                    if (AuthorizationManager.IsTokenizationEnabled() &&
                        !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sSiteGUID, 0, null, groupID, initObj.Platform))
                    {
                        return null;
                    }
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserTypeByUserID(sSiteGUID, nUserTypeID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return response.ToString();
        }

        [PrivateMethod]
        public TVPApiModule.Objects.UserResponse SetUserDynamicDataEx(InitializationObject initObj, string key, string value)
        {
            TVPApiModule.Objects.UserResponse retVal = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserDynamicData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    IImplementation impl = WSUtils.GetImplementation(groupID, initObj);
                    retVal = impl.SetUserDynamicData(initObj, groupID, key, value);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return retVal;
        }

        [PrivateMethod]
        public TVPApiModule.Objects.Responses.PinCodeResponse GenerateLoginPIN(InitializationObject initObj, string secret)
        {
            TVPApiModule.Objects.Responses.PinCodeResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GenerateLoginPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GenerateLoginPIN(initObj.SiteGuid, secret);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new TVPApiModule.Objects.Responses.PinCodeResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new TVPApiModule.Objects.Responses.PinCodeResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        #endregion


        public TVPApiModule.Objects.Responses.UserResponse LoginWithPIN(InitializationObject initObj, string PIN, string secret)
        {
            TVPApiModule.Objects.Responses.UserResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "LoginWithPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(PIN))
                    {
                        response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).LoginWithPIN(PIN, secret, initObj.UDID, 
                            System.Web.HttpContext.Current.Request.GetHeaders());
                    }
                    else
                    {
                        bool isSingleLogin = TVPApi.ConfigManager.GetInstance()
                                       .GetConfig(groupID, initObj.Platform)
                                       .SiteConfiguration.Data.Features.SingleLogin.SupportFeature;

                        var logInResponse = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).
                            LogIn(string.Empty, string.Empty, string.Empty, initObj.UDID, isSingleLogin, System.Web.HttpContext.Current.Request.GetHeaders());

                        if (logInResponse != null)
                        {
                            response = new TVPApiModule.Objects.Responses.UserResponse()
                            {
                                Result = new UserResult(logInResponse),
                                Status = new TVPApiModule.Objects.Responses.Status(logInResponse.resp.Code, logInResponse.resp.Message)
                            };
                        }
                        else
                        {
                            response = new TVPApiModule.Objects.Responses.UserResponse()
                            {
                                Status = new TVPApiModule.Objects.Responses.Status((int)eStatus.Error, eStatus.Error.ToString())
                            };
                        }
                    }

                    // if sign in successful and tokenization enabled - generate access token and add it to headers
                    if (AuthorizationManager.IsTokenizationEnabled() && 
                        response.Status != null && response.Status.Code == (int)eStatus.OK && response.Result != null && response.Result.user != null && response.Result.user.m_user != null &&
                       (response.Result.user.m_RespStatus != ResponseStatus.OK || 
                        response.Result.user.m_RespStatus != ResponseStatus.UserNotActivated ||
                        response.Result.user.m_RespStatus != ResponseStatus.DeviceNotRegistered || 
                        response.Result.user.m_RespStatus != ResponseStatus.UserNotMasterApproved ||
                        response.Result.user.m_RespStatus != ResponseStatus.UserNotIndDomain || 
                        response.Result.user.m_RespStatus != ResponseStatus.UserWithNoDomain ||
                        response.Result.user.m_RespStatus != ResponseStatus.UserSuspended))
                    {
                        var token = AuthorizationManager.Instance.GenerateAccessToken(response.Result.user.m_user.m_sSiteGUID, groupID, false, true, initObj.UDID, initObj.Platform, response.Result.user.m_user.m_domianID, response.Result.user.m_user.m_oBasicData.RoleIds);

                        HttpContext.Current.Response.Headers.Add("access_token", string.Format("{0}|{1}", token.AccessToken, token.AccessTokenExpiration));
                        HttpContext.Current.Response.Headers.Add("refresh_token", string.Format("{0}|{1}", token.RefreshToken, token.RefreshTokenExpiration));
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new TVPApiModule.Objects.Responses.UserResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new TVPApiModule.Objects.Responses.UserResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [PrivateMethod]
        public TVPApiModule.Objects.Responses.PinCodeResponse SetLoginPIN(InitializationObject initObj, string PIN, string secret)
        {
            TVPApiModule.Objects.Responses.PinCodeResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "LoginWithPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetLoginPIN(initObj.SiteGuid, PIN, secret);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                    response = new TVPApiModule.Objects.Responses.PinCodeResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
                response = new TVPApiModule.Objects.Responses.PinCodeResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [PrivateMethod]
        public ClientResponseStatus ClearLoginPIN(InitializationObject initObj, string pinCode)
        {
            ClientResponseStatus response = new ClientResponseStatus();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "LoginWithPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                if (string.IsNullOrEmpty(pinCode))
                {
                    response.Status = new TVPApiModule.Objects.Responses.Status((int)TVPApiModule.Objects.Responses.eStatus.BadRequest, "pinCode cannot be empty");
                    return response;
                }

                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ClearLoginPINs(initObj.SiteGuid, pinCode);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                    response = new ClientResponseStatus();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
                response = new ClientResponseStatus();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [PrivateMethod]
        public ClientResponseStatus ClearLoginPINs(InitializationObject initObj)
        {
            ClientResponseStatus response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ClearLoginPINs", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ClearLoginPINs(initObj.SiteGuid, null);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new ClientResponseStatus();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new ClientResponseStatus();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [PrivateMethod]
        public ClientResponseStatus DeleteUser(InitializationObject initObj)
        {
            ClientResponseStatus response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "DeleteUser", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            bool isTokenizationValid = false;

            if (groupID > 0)
            {
                try
                {
                    // Tokenization: validate siteGuid
                    if (AuthorizationManager.IsTokenizationEnabled() &&
                        !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, initObj.SiteGuid, 0, null, groupID, initObj.Platform))
                    {
                        return null;
                    }

                    isTokenizationValid = true;

                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).DeleteUser(initObj.SiteGuid);

                    if (response.Status.Code == (int)TVPApiModule.Objects.Responses.eStatus.OK && isTokenizationValid)
                    {
                        AuthorizationManager.Instance.DeleteAccessToken(initObj.Token);
                    }

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new ClientResponseStatus();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new ClientResponseStatus();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }
    }

}
