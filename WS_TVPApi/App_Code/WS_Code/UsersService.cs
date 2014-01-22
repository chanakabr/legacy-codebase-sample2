using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using log4net;
using TVPApiModule.Services;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPPro.SiteManager.TvinciPlatform.Billing;
//using TVPPro.SiteManager.TvinciPlatform.Users;
using System.Web;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class UsersService : System.Web.Services.WebService//, IUsersService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(BillingService));

        #region public methods

        [WebMethod(EnableSession = true, Description = "Change user password")]
        public TVPApiModule.Objects.Responses.UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = new TVPApiModule.Objects.Responses.UserResponseObject();

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
        public TVPApiModule.Objects.Responses.UserResponseObject RenewUserPassword(InitializationObject initObj, string sUN, string sPass)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = new TVPApiModule.Objects.Responses.UserResponseObject();

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
        public TVPApiModule.Objects.Responses.UserResponseObject GetUserByFacebookID(InitializationObject initObj, string facebookId)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = new TVPApiModule.Objects.Responses.UserResponseObject();

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
        public TVPApiModule.Objects.Responses.UserResponseObject GetUserByUsername(InitializationObject initObj, string userName)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = new TVPApiModule.Objects.Responses.UserResponseObject();

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
        public void Logout(InitializationObject initObj, string siteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "Logout", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
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
        public TVPApiModule.Objects.Responses.UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = new TVPApiModule.Objects.Responses.UserResponseObject();

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
                    string sClearPassword = siteSvc.GetSiteGuidFromSecured(initObj,sEncryptedPassword);
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
        public TVPApiModule.Objects.Responses.Country[] GetCountriesList(InitializationObject initObj)
        {
            TVPApiModule.Objects.Responses.Country[] response = null;

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
                    TVPApiModule.Objects.Responses.UserResponseObject userResponseObject = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).CheckTemporaryToken(sToken);

                    if (userResponseObject != null && userResponseObject.m_RespStatus == TVPApiModule.Objects.Responses.ResponseStatus.OK)
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
        public TVPApiModule.Objects.Responses.UserType[] GetGroupUserTypes(InitializationObject initObj)
        {
            TVPApiModule.Objects.Responses.UserType[] response = null;

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
        public string RenewUserPIN(InitializationObject initObj, string siteGUID, int ruleID)
        {
            TVPApiModule.Objects.Responses.ResponseStatus response = TVPApiModule.Objects.Responses.ResponseStatus.OK;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RenewUserPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RenewUserPIN(siteGUID, ruleID);
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
        public TVPApiModule.Objects.Responses.UserResponseObject ActivateAccountByDomainMaster(InitializationObject initObj, string masterUserName, string userName, string token)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

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
        public bool AddItemToList(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddItemToList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).AddItemToList(siteGuid, itemObjects, itemType, listType);
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
        public bool RemoveItemFromList(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveItemFromList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RemoveItemFromList(siteGuid, itemObjects, itemType, listType);
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
        public bool UpdateItemInList(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "UpdateItemInList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).UpdateItemInList(siteGuid, itemObjects, itemType, listType);
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
        public TVPApiModule.Objects.Responses.UserItemList[] GetItemFromList(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            TVPApiModule.Objects.Responses.UserItemList[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetItemFromList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetItemFromList(siteGuid, itemObjects, itemType, listType);
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
        public TVPApiModule.Objects.Responses.KeyValuePair[] IsItemExistsInList(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.ItemObj[] itemObjects, TVPPro.SiteManager.TvinciPlatform.Users.ItemType itemType, TVPPro.SiteManager.TvinciPlatform.Users.ListType listType)
        {
            TVPApiModule.Objects.Responses.KeyValuePair[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsItemExistsInList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IsItemExistsInList(siteGuid, itemObjects, itemType, listType);
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
        public string SetUserTypeByUserID(InitializationObject initObj, string siteGUID, int nUserTypeID)
        {
            TVPApiModule.Objects.Responses.ResponseStatus response = TVPApiModule.Objects.Responses.ResponseStatus.OK;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserTypeByUserID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserTypeByUserID(siteGUID, nUserTypeID);
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

        #endregion
    }
}
