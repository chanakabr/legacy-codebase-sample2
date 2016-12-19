using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using TVPApiModule.Services;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPPro.SiteManager.TvinciPlatform.Billing;
using TVPPro.SiteManager.TvinciPlatform.Users;
using System.Web;
using TVPApiModule.Interfaces;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Manager;
using TVPApiModule.Objects.Authorization;
using KLogMonitor;
using System.Reflection;
using System.Configuration;

namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class UsersService : System.Web.Services.WebService, IUsersService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region public methods

        [WebMethod(EnableSession = true, Description = "Change user password")]
        [PrivateMethod]
        public UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ChangeUserPassword(sUN, sOldPass, sPass);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Renew user password")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get user facebook ID")]
        [PrivateMethod]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject GetUserByFacebookID(InitializationObject initObj, string facebookId)
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get user by username")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Logout")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
        }

        [WebMethod(EnableSession = true, Description = "Activate user account")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Resend activation mail")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "SignIn with encrypted password")]
        public TVPApiModule.Services.ApiUsersService.LogInResponseData SignInSecure(InitializationObject initObj, string sUsername, string sEncryptedPassword)
        {
            ApiUsersService.LogInResponseData response = new ApiUsersService.LogInResponseData();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignInSecure", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    SiteService siteSvc = new SiteService();
                    string privateKey = ConfigurationManager.AppSettings["SecureSiteGuidKey"];
                    string IV = ConfigurationManager.AppSettings["SecureSiteGuidIV"];
                    string sClearPassword = SecurityHelper.DecryptSiteGuid(privateKey, IV, sEncryptedPassword);

                    response = siteSvc.SignIn(initObj, sUsername, sClearPassword);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "GetCountriesList")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Check temporary token")]
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
                        HttpContext.Current.Items.Add("Error", "Temporary token is invalid Protocol CheckTemporaryToken");
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "GetGroupUserTypes")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Renews user PIN")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Activate Account By Domain Master")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Send Password Mail")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Adds Item To List")]
        [PrivateMethod]
        public bool AddItemToList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType)
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Removes Item From List")]
        [PrivateMethod]
        public bool RemoveItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType)
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Updates Item In List")]
        [PrivateMethod]
        public bool UpdateItemInList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType)
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Gets Item From List")]
        public UserItemList[] GetItemFromList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType)
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Is Item Exists In List")]
        [PrivateMethod]
        public KeyValuePair[] IsItemExistsInList(InitializationObject initObj, ItemObj[] itemObjects, ItemType itemType, ListType listType)
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Set UserType by UserID")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return response.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Set UserType by UserID")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return retVal;
        }

        [WebMethod(EnableSession = true, Description = "GenerateLoginPIN")]
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

        #endregion


        public TVPApiModule.Objects.Responses.UserResponse LoginWithPIN(InitializationObject initObj, string PIN, string secret)
        {
            TVPApiModule.Objects.Responses.UserResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "LoginWithPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).LoginWithPIN(PIN, secret,  initObj.UDID);

                    // if sign in successful and tokenization enabled - generate access token and add it to headers
                    if (AuthorizationManager.IsTokenizationEnabled() && response.Status != null && response.Status.Code == (int)eStatus.OK &&
                       response.Result != null && response.Result.user != null && response.Result.user.m_user != null &&
                       (response.Result.user.m_RespStatus != ResponseStatus.OK || response.Result.user.m_RespStatus != ResponseStatus.UserNotActivated ||
                       response.Result.user.m_RespStatus != ResponseStatus.DeviceNotRegistered || response.Result.user.m_RespStatus != ResponseStatus.UserNotMasterApproved ||
                       response.Result.user.m_RespStatus != ResponseStatus.UserNotIndDomain || response.Result.user.m_RespStatus != ResponseStatus.UserWithNoDomain ||
                       response.Result.user.m_RespStatus != ResponseStatus.UserSuspended))
                    {
                        var token = AuthorizationManager.Instance.GenerateAccessToken(response.Result.user.m_user.m_sSiteGUID, groupID, false, true, initObj.UDID, initObj.Platform);

                        HttpContext.Current.Response.Headers.Add("access_token", string.Format("{0}|{1}", token.AccessToken, token.AccessTokenExpiration));
                        HttpContext.Current.Response.Headers.Add("refresh_token", string.Format("{0}|{1}", token.RefreshToken, token.RefreshTokenExpiration));
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                    response = new TVPApiModule.Objects.Responses.UserResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
                response = new TVPApiModule.Objects.Responses.UserResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "SetLoginPIN")]
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

        [WebMethod(EnableSession = true, Description = "ClearLoginPIN")]
        [PrivateMethod]
        public ClientResponseStatus ClearLoginPIN(InitializationObject initObj, string pinCode)
        {
            TVPApiModule.Objects.Responses.ClientResponseStatus response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "LoginWithPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ClearLoginPINs(initObj.SiteGuid, pinCode);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                    response = new TVPApiModule.Objects.Responses.ClientResponseStatus();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
                response = new TVPApiModule.Objects.Responses.ClientResponseStatus();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "ClearLoginPINs")]
        [PrivateMethod]
        public ClientResponseStatus ClearLoginPINs(InitializationObject initObj)
        {
            TVPApiModule.Objects.Responses.ClientResponseStatus response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ClearLoginPINs", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ClearLoginPINs(initObj.SiteGuid, null);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                    response = new TVPApiModule.Objects.Responses.ClientResponseStatus();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
                response = new TVPApiModule.Objects.Responses.ClientResponseStatus();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }


    }
}
