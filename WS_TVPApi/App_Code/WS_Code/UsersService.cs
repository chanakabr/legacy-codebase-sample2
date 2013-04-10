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
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class UsersService : System.Web.Services.WebService, IUsersService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(BillingService));

        #region public methods

        [WebMethod(EnableSession = true, Description = "Change user password")]
        public UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("ChangeUserPassword-> [{0}, {1}], Params:[siteGuid: {2} sUN: {3}]", groupID, initObj.Platform, initObj.SiteGuid, sUN);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ChangeUserPassword(sUN, sOldPass, sPass);
                }
                catch (Exception ex)
                {
                    logger.Error("ChangeUserPassword->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("ChangeUserPassword-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Renew user password")]
        public UserResponseObject RenewUserPassword(InitializationObject initObj, string sUN, string sPass)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RenewUserPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("RenewUserPassword-> [{0}, {1}], Params:[siteGuid: {2} sUN: {3}]", groupID, initObj.Platform, initObj.SiteGuid, sUN);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RenewUserPassword(sUN, sPass);
                }
                catch (Exception ex)
                {
                    logger.Error("RenewUserPassword->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("RenewUserPassword-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get user facebook ID")]
        public UserResponseObject GetUserByFacebookID(InitializationObject initObj, string facebookId)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserByFacebookID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUserByFacebookID-> [{0}, {1}], Params:[facebookId: {2}]", groupID, initObj.Platform, facebookId);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserByFacebookID(facebookId);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserByFacebookID->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetUserByFacebookID-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get user by username")]
        public UserResponseObject GetUserByUsername(InitializationObject initObj, string userName)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserByUsername", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUserByUsername-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, userName);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserByUsername(userName);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserByUsername->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetUserByUsername-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Logout")]
        public void Logout(InitializationObject initObj, string sSiteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "Logout", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("Logout-> [{0}, {1}], Params:[sSiteGuid: {2}]", groupID, initObj.Platform, sSiteGuid);

            if (groupID > 0)
            {
                try
                {
                    string siteGuid = string.IsNullOrEmpty(sSiteGuid) ? initObj.SiteGuid : sSiteGuid;
                    new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).Logout(siteGuid);
                }
                catch (Exception ex)
                {
                    logger.Error("Logout->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("Logout-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
        }

        [WebMethod(EnableSession = true, Description = "Activate user account")]
        public UserResponseObject ActivateAccount(InitializationObject initObj, string sUserName, string sToken)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ActivateAccount", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("ActivateAccount-> [{0}, {1}], Params:[sUserName: {2}]", groupID, initObj.Platform, sUserName);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ActivateAccount(sUserName, sToken);
                }
                catch (Exception ex)
                {
                    logger.Error("ActivateAccount->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("ActivateAccount-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Resend activation mail")]
        public bool ResendActivationMail(InitializationObject initObj, string sUserName, string sNewPassword)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ResendActivationMail", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("ResendActivationMail-> [{0}, {1}], Params:[sUserName: {2}]", groupID, initObj.Platform, sUserName);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ResendActivationMail(sUserName, sNewPassword);
                }
                catch (Exception ex)
                {
                    logger.Error("ResendActivationMail->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("ResendActivationMail-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "SignIn with encrypted password")]
        public TVPApiModule.Services.ApiUsersService.LogInResponseData SignInSecure(InitializationObject initObj, string sUsername, string sEncryptedPassword)
        {
            ApiUsersService.LogInResponseData response = new ApiUsersService.LogInResponseData();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignInSecure", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SignInSecure-> [{0}, {1}], Params:[sUserName: {2}]", groupID, initObj.Platform, sUsername);

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
                    logger.Error("SignInSecure->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("ResendActivationMail-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "GetCountriesList")]
        public Country[] GetCountriesList(InitializationObject initObj)
        {
            Country[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetCountriesList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetCountriesList-> [{0}, {1}]", groupID, initObj.Platform);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetCountriesList();
                }
                catch (Exception ex)
                {
                    logger.Error("GetCountriesList->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetCountriesList-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Check temporary token")]
        public string CheckTemporaryToken(InitializationObject initObj, string sToken)
        {
            string response = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CheckTemporaryToken", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("CheckTemporaryToken-> [{0}, {1}], Params:[sToken: {2}]", groupID, initObj.Platform, sToken);

            if (groupID > 0)
            {
                try
                {
                    UserResponseObject userResponseObject = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).CheckTemporaryToken(sToken);

                    if (userResponseObject != null && userResponseObject.m_RespStatus == ResponseStatus.OK)
                    {
                        logger.InfoFormat("Temporary token is valid Protocol CheckTemporaryToken, Parameters : Token {0}: ", sToken);

                        response = userResponseObject.m_user.m_oBasicData.m_sUserName;
                    }
                    else
                    {
                        logger.InfoFormat("Temporary token is invalid Protocol CheckTemporaryToken,Parameters : Token : {0}", sToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("CheckTemporaryToken->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("CheckTemporaryToken-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        #endregion
    }
}
