using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.Configuration.PlatformServices;
using System.Web;
using System.Web.SessionState;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.Helper;
using System.Collections;
using TVPPro.SiteManager.TvinciPlatform.Users;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.Services
{
    public class UsersService
    {
        #region Fields
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public const string sessionKey = "UserManagerUserKey";
        private TvinciPlatform.Users.UsersService PlatUserService;

        private string wsUserName;
        private string wsPassword;

        static volatile UsersService instance;
        static object instanceLock = new object();
        private Dictionary<int, string> m_dictCountries = new Dictionary<int, string>();
        private Dictionary<string, RegionData> m_dictCountriesList;
        private Dictionary<string, UserContext> m_UserContext = new Dictionary<string, UserContext>();
        //private UserContext context;
        #endregion

        public struct RegionData
        {
            public string Name;
            public string Code;
            public string ID;
        }

        #region Constructor
        private UsersService()
        {
            PlatUserService = new TVPPro.SiteManager.TvinciPlatform.Users.UsersService();
            PlatUserService.Url = PlatformServicesConfiguration.Instance.Data.UsersService.URL;
            wsUserName = PlatformServicesConfiguration.Instance.Data.UsersService.DefaultUser;
            wsPassword = PlatformServicesConfiguration.Instance.Data.UsersService.DefaultPassword;

            logger.Info("Starting PlatUserService with URL:" + PlatformServicesConfiguration.Instance.Data.UsersService.URL);
        }
        #endregion

        #region Properties
        public UserContext UserContext
        {
            get
            {
                //System.Web.SessionState.HttpSessionState

                UserContext context = HttpContext.Current != null ? HttpContext.Current.Session[sessionKey] as UserContext : null;

                if (context == null)
                {
                    context = new UserContext();
                    HttpContext.Current.Session[sessionKey] = context;
                }

                return context;
            }
            private set
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Session[sessionKey] = value;
            }
        }

        public static UsersService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new UsersService();
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

        #region Public Methods

        public string GetUserSiteGuid(string UserName, string Password)
        {
            string retVal = string.Empty;
            try
            {
                UserResponseObject responseObj = PlatUserService.CheckUserPassword(wsUserName, wsPassword, UserName, Password, false);
                if (responseObj.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                {
                    if (responseObj.m_user == null)
                    {
                        logger.ErrorFormat("Error in CheckUserPassword protocol, user obj null, Parameters : User name, Password : {0}", UserName, Password);
                    }
                    else
                    {
                        retVal = responseObj.m_user.m_sSiteGUID;
                        logger.InfoFormat("Sign in protocol CheckUserPassword, Parameters : User name {0} Password {1}", UserName, Password);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in SignIn module CheckUserPassword, Error : {0} Parameters : User : {1}", ex.Message, UserName);
            }
            return retVal;
        }

        public Enums.eUserOnlineStatus GetCurrentUserOnlineStatus(string UserName, string Password, TVPPro.SiteManager.TvinciPlatform.Users.KeyValuePair[] KeyValuePairs = null)
        {
            UserContext = null; // to be ensure we start a new context
            try
            {
                UserContext.UserResponse = PlatUserService.SignIn(wsUserName, wsPassword, UserName, Password, HttpContext.Current.Session.SessionID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP(), string.Empty, TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.SingleLogin.SupportFeature, KeyValuePairs);
                return UserContext.OnlineStatus;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in SignIn module CheckUserPassword, Error : {0} Parameters : User : {1}", ex.Message, UserName);
                return Enums.eUserOnlineStatus.Error;
            }
        }

        public bool SignIn(string UserName, string Password, TVPPro.SiteManager.TvinciPlatform.Users.KeyValuePair[] KeyValuePairs = null)
        {
            UserContext = null; // to be ensure we start a new context
            try
            {
                UserContext.UserResponse = PlatUserService.SignIn(wsUserName, wsPassword, UserName, Password, HttpContext.Current.Session.SessionID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP(), string.Empty, TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.SingleLogin.SupportFeature, KeyValuePairs);
                return UserContext.OnlineStatus == Enums.eUserOnlineStatus.LoggedIn;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in SignIn module CheckUserPassword, Error : {0} Parameters : User : {1}", ex.Message, UserName);
                return false;
            }
        }

        public bool AutoSignIn(string UserID, string SessionID, string UserIP)
        {
            UserContext = null; // to be ensure we start a new context
            try
            {
                UserContext.UserResponse = PlatUserService.AutoSignIn(wsUserName, wsPassword, UserID, SessionID, UserIP, string.Empty, TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.SingleLogin.SupportFeature);
                return UserContext.OnlineStatus == Enums.eUserOnlineStatus.LoggedIn;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in SignIn module AutoSignIn, Error : {0} Parameters : User : {1}", ex.Message, UserID);
                return false;
            }
        }

        public bool CheckUserPassword(string username, string password)
        {
            bool isSuccess = false;
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    isSuccess = false;
                else
                {
                    UserResponseObject res = instance.PlatUserService.CheckUserPassword(wsUserName, wsPassword, username, password, false);
                    isSuccess = res.m_RespStatus == ResponseStatus.OK;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in CheckUserPassword, Error Message: {0} Parameters :User : {1} ", ex.Message, username);
                isSuccess = false;
            }
            return isSuccess;
        }

        public bool ChangePassword(string userName, string newPassword)
        {
            return instance.ChangePassword(userName, string.Empty, newPassword);
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            try
            {
                UserResponseObject tempRetObj = instance.PlatUserService.ChangeUserPassword(wsUserName, wsPassword, userName, oldPassword, newPassword);

                if (tempRetObj.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                {
                    if (tempRetObj.m_user != null)
                    {
                        UserContext.UserResponse = tempRetObj;
                        logger.InfoFormat("Password changed succesfully, Parameters : UserName {0}: , Password: {1} ", userName, newPassword);
                        return true;
                    }
                    else
                    {
                        logger.ErrorFormat("Error occured in Change Password m_user is null for UserName: {0} ", userName);
                        return false;
                    }
                }
                else
                {
                    logger.ErrorFormat("Error occured in Change Password online status is not OK for UserName: {0} ", userName);
                    return false;
                }

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in SignUp protocol AddNewUser, Error Message: {0} Parameters :User : {1} ", ex.Message, userName);
                return false;
            }
        }


        public List<string> SignUp(string userPassword, TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, bool AutoLogin)
        {
            List<string> ResponseParam = new List<string>();
            try
            {
                //Send user information.
                UserContext.UserResponse = instance.PlatUserService.AddNewUser(wsUserName, wsPassword, userBasicData, userDynamicData, userPassword, string.Empty);

                if (UserContext.UserResponse.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                {

                    if (AutoLogin)
                    {
                        bool res = DomainsService.Instance.CreateDomain();
                        if (res && AutoLogin)
                        {
                            SignIn(UserContext.UserResponse.m_user.m_oBasicData.m_sUserName, userPassword);
                        }
                        logger.InfoFormat("Succes signUp protocol AddNewUser, Parameters : User name : ", userBasicData.m_sUserName);

                        //Get user id.
                        //long TvinciID = 0;
                        //long.TryParse(userContext.m_UserResponse.m_user.m_sSiteGUID, out TvinciID);
                        //userContext.m_TvinciID = TvinciID;
                        ResponseParam.Add("true");
                        ResponseParam.Add(UserContext.UserResponse.m_RespStatus.ToString());
                        ResponseParam.Add(UserContext.UserResponse.m_user.m_oBasicData.m_sUserName.ToString());
                        ResponseParam.Add(UserContext.UserResponse.m_user.m_sSiteGUID);
                    }
                    logger.InfoFormat("Succes signUp protocol AddNewUser, Parameters : User name : ", userBasicData.m_sUserName);

                    //Get user id.
                    //long TvinciID = 0;
                    //long.TryParse(userContext.m_UserResponse.m_user.m_sSiteGUID, out TvinciID);
                    //userContext.m_TvinciID = TvinciID;
                    ResponseParam.Add("true");
                    ResponseParam.Add(UserContext.UserResponse.m_RespStatus.ToString());
                    ResponseParam.Add(UserContext.UserResponse.m_user.m_oBasicData.m_sUserName.ToString());
                    ResponseParam.Add(UserContext.UserResponse.m_user.m_sSiteGUID);
                }
                else
                {
                    ResponseParam.Add("false");
                    ResponseParam.Add(UserContext.UserResponse.m_RespStatus.ToString());
                }
                return ResponseParam;
            }
            catch (Exception ex)
            {
                ResponseParam.Add("false");
                ResponseParam.Add("Generic error");
                logger.ErrorFormat("Error occured in SignUp protocol AddNewUser, Error Message: {0} Parameters :User : {1} ", ex.Message, userBasicData.m_sUserName);
                return ResponseParam;
            }
        }

        public void SignOut()
        {
            try
            {
                if (UserContext.UserResponse != null && UserContext.UserResponse.m_user != null && HttpContext.Current != null)
                    SignOut(UserContext.UserResponse.m_user.m_sSiteGUID, HttpContext.Current.Session.SessionID, UserContext.UserIP);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in SignOut module SignOut, Error : {0}", ex.Message);
            }
        }

        public void SignOut(string siteGUID, string sessionID, string userIP)
        {
            try
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Session[sessionKey] = null;
                PlatUserService.SignOut(wsUserName, wsPassword, siteGUID, sessionID, userIP, string.Empty, TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.SingleLogin.SupportFeature);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in SignOut module SignOut, Error : {0}", ex.Message);
            }
        }

        public string GetUserID()
        {
            if (UserContext.OnlineStatus == Enums.eUserOnlineStatus.LoggedIn)
            {
                return UserContext.UserResponse.m_user.m_sSiteGUID;
            }
            else if (UserContext.OnlineStatus == Enums.eUserOnlineStatus.UserAllreadyLoggedIn)
            {
                if (UserContext.UserResponse.m_user.m_oDynamicData.m_sUserData[0].m_sDataType.Equals("IsAnonymous"))
                    return UserContext.UserResponse.m_user.m_sSiteGUID;
                else
                    return string.Empty;
            }
            else if (UserContext.OnlineStatus == Enums.eUserOnlineStatus.Recognised)
            {
                return GetUserIdOnCookie();
            }
            else
                return string.Empty;
        }

        public int GetUserTypeID()
        {
            int nUserTypeID = 0;

            if (TVPPro.SiteManager.Services.UsersService.Instance.UserContext.UserResponse.m_user != null &&
                TVPPro.SiteManager.Services.UsersService.Instance.UserContext.UserResponse.m_user.m_oBasicData != null &&
                TVPPro.SiteManager.Services.UsersService.Instance.UserContext.UserResponse.m_user.m_oBasicData.m_UserType.ID != null)
            {
                nUserTypeID = TVPPro.SiteManager.Services.UsersService.Instance.UserContext.UserResponse.m_user.m_oBasicData.m_UserType.ID.Value;
            }
            return nUserTypeID;
        }

        public Enums.eUserOnlineStatus GetUserOnlineStatus()
        {
            return UserContext.OnlineStatus;
        }

        public bool IsLogIn()
        {
            return UserContext.OnlineStatus == Enums.eUserOnlineStatus.LoggedIn;
        }

        public bool SentNewPasswordToUser(string UserName)
        {
            return SentNewPasswordToUser(UserName, true);
        }

        public bool SentNewPasswordToUser(string UserName, bool startNewContext)
        {
            if (startNewContext)
                UserContext = null; // to be ensure we start a new context

            try
            {
                UserResponseObject userResponseObject = PlatUserService.ForgotPassword(wsUserName, wsPassword, UserName);

                if (userResponseObject.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
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

        public bool SendChangePasswordMail(string userName)
        {
            bool res = false;

            try
            {
                UserResponseObject userResponse = PlatUserService.ChangePassword(wsUserName, wsPassword, userName);

                if (userResponse.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                {
                    logger.InfoFormat("Sent new temp password protocol SendChangePasswordMail, Parameters : User name {0}: ", userName);

                    res = true;
                }
                else
                {
                    logger.InfoFormat("Can not send temp password protocol SendChangePasswordMail,Parameters : User name : {0}", userName);

                    res = false;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SendChangePasswordMail, Error Message: {0}, Parameters :ws User name : {1} , ws Password {2}: UserName : {3}", ex.Message, wsUserName, wsPassword, userName);

                res = false;
            }

            return res;
        }

        public string CheckTemporaryUserToken(string Token)
        {
            return CheckTemporaryUserToken(Token, false, true);
        }

        public string CheckTemporaryUserToken(string Token, bool autoLogIn, bool autoLogOut)
        {
            if (autoLogOut)
                UserContext = null; // to be ensure we start a new context

            UserContext tempUserContext = new UserContext(); // we use local UserContext variable so the user will not be logged in until he/she clicks on change password button.
            string CurrentUserName = string.Empty;

            try
            {
                tempUserContext.UserResponse = PlatUserService.CheckTemporaryToken(wsUserName, wsPassword, Token);

                if (tempUserContext.UserResponse.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                {
                    logger.InfoFormat("Temporary token is valid Protocol CheckTemporaryToken, Parameters : Token {0}: ", Token);
                    CurrentUserName = tempUserContext.UserResponse.m_user.m_oBasicData.m_sUserName;

                    if (!autoLogOut)
                    {
                        if (UserContext.UserResponse.m_user != null && UserContext.UserResponse.m_user.m_oBasicData.m_sUserName != CurrentUserName)
                        {
                            UserContext = null; // to be ensure we start a new context in case current context is different
                        }
                    }

                    if (autoLogIn && !string.IsNullOrEmpty(tempUserContext.UserResponse.m_user.m_sSiteGUID))
                    {
                        AutoSignIn(tempUserContext.UserResponse.m_user.m_sSiteGUID, HttpContext.Current.Session.SessionID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
                    }
                }
                else
                {
                    logger.InfoFormat("Temporary token is invalid Protocol CheckTemporaryToken,Parameters : User name : {0}", Token);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in CheckTemporaryUserToken protocol CheckTemporaryToken, Error Message: {0} Parameters :Token : {1} ", ex.Message, Token);
            }
            return CurrentUserName;
        }

        public bool RenewPassword(string UserName, string Password)
        {
            return RenewPassword(UserName, Password, false, true);
        }

        public bool RenewPassword(string UserName, string Password, bool autoLogOut)
        {
            return RenewPassword(UserName, Password, false, autoLogOut);
        }

        public bool RenewPassword(string UserName, string Password, bool autoLogIn, bool autoLogOut)
        {
            if (autoLogOut)
                UserContext = null; // to be ensure we start a new context

            UserContext tempUserContext = new UserContext(); // we use local UserContext variable so the user will not be logged in until he/she clicks on change password button.

            try
            {
                tempUserContext.UserResponse = PlatUserService.RenewUserPassword(wsUserName, wsPassword, UserName, Password);

                if (tempUserContext.UserResponse.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                {
                    if (autoLogIn)
                    {
                        SignIn(UserName, Password);
                    }

                    logger.InfoFormat("Password was renewed successfuly Protocol RenewUserPassword, Parameters : UserName {0}: , Password {1}: ", UserName, Password);
                    return true;
                }
                else
                {
                    logger.InfoFormat("Can not renew password Protocol RenewUserPassword,Parameters : User name : {0} , Password {1}: ", UserName, Password);

                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error renew password Protocol RenewUserPassword, Error Message: {0} Parameters :User name : {1} , Password {2}: ", ex.Message, UserName, Password);
                return false;
            }
        }

        public bool ResentWelcomeMail(string UserName, string Password)
        {
            //try sent again welcome mail to the user 
            try
            {
                if (PlatUserService.ResendWelcomeMail(wsUserName, wsPassword, UserName, Password))
                {
                    logger.InfoFormat("Welcome mail was resent successfuly Protocol ResentWelcomeMail, Parameters : UserName {0}: , Password {1}: ", UserName, Password);
                    return true;
                }
                else
                {
                    logger.InfoFormat("Can not resent welcome mail Protocol ResentWelcomeMail,Parameters : User name : {0} , Password {1}: ", UserName, Password);

                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error resent welcome mail Protocol ResentWelcomeMail, Error Message: {0} Parameters :User name : {1} , Password: {2} ", ex.Message, UserName, Password);
                return false;
            }

        }

        public bool IsUserActivated()
        {
            try
            {
                return PlatUserService.IsUserActivated(wsUserName, wsPassword, int.Parse(GetUserID()));
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error checking user exist Protocol IsUserActivated, Error Message: {0} Parameters :siteGuid : {1} ", ex.Message, GetUserID());
                return false;
            }
        }

        public bool ActivateAccount(string UserName, string UserToken, bool AutoLogIn)
        {
            UserContext = null; // to be ensure we start a new context

            try
            {
                //Get user object on activation
                var usersResponse = PlatUserService.ActivateAccount(wsUserName, wsPassword, UserName, UserToken);
                if (usersResponse != null)
                {
                    UserContext.UserResponse = usersResponse.user;
                }

                if (UserContext.UserResponse.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                {
                    if (UserContext.UserResponse.m_user != null)
                    {
                        //if AutoLogin parameter set as true change user status
                        if (AutoLogIn)
                        {
                            AutoSignIn(UserContext.UserResponse.m_user.m_sSiteGUID, HttpContext.Current.Session.SessionID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
                        }
                        logger.InfoFormat("Account was activated successfuly Protocol ActivateAccount, Parameters : UserName {0}: , UserToken: {1} ", UserName, UserToken);
                        return true;
                    }
                    else
                    {
                        logger.InfoFormat("Error log in user Protocol ActivateAccount, Parameters : UserName {0}: , UserToken: {1} ", UserName, UserToken);
                        return false;
                    }
                }
                else
                {
                    logger.InfoFormat("Can not activate user protocol ActivateAccount, Parameters : User name: {0}, UserToken : {0}", UserName, UserToken);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error activate account Protocol ActivateAccount, Error Message: {0} Parameters :User name : {1} , UserToken {2}: ", ex.Message, UserName, UserToken);
                return false;
            }
        }

        public bool UserNameAlreadyExist(string UserName)
        {
            try
            {
                if (PlatUserService.DoesUserNameExists(wsUserName, wsPassword, UserName))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error checking user exist Protocol DoesUserNameExists, Error Message: {0} Parameters :User name : {1} ", ex.Message, UserName);
                return false;
            }
        }

        public ResponseStatus GetExistingFacebookUser(string FacbookId, bool AutoLogIn)
        {
            UserContext = null; // to be ensure we start a new context

            try
            {
                if (!string.IsNullOrEmpty(FacbookId))
                {
                    UserContext.UserResponse = PlatUserService.GetUserByFacebookID(wsUserName, wsPassword, FacbookId);

                    //User with the current facebbok id exist on the database
                    if (UserContext.UserResponse.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                    {
                        if (AutoLogIn)
                        {
                            AutoSignIn(UserContext.UserResponse.m_user.m_sSiteGUID, HttpContext.Current.Session.SessionID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
                            if (UserContext.UserResponse.m_RespStatus != ResponseStatus.OK)
                                return UserContext.UserResponse.m_RespStatus;
                        }
                        //Update user details
                        logger.InfoFormat("Facebook user is exist Protocol GetUserByFacebookID, Parameters : FacebookId {0}: ", FacbookId);
                        return ResponseStatus.OK;
                    }
                    //User is not exist on our database
                    else if (UserContext.UserResponse.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.UserDoesNotExist)
                    {
                        //new user will be created on submit
                        logger.InfoFormat("Facebook user is exist Protocol GetUserByFacebookID, Parameters : FacebookId {0}: ", FacbookId);
                        return UserContext.UserResponse.m_RespStatus;
                    }
                    else
                    {
                        logger.InfoFormat("Facebook user is not exist Protocol GetUserByFacebookID, Parameters : FacebookId {0}: ", FacbookId);
                        return UserContext.UserResponse.m_RespStatus;
                    }
                }
                else
                    return ResponseStatus.UserDoesNotExist;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured recive facebook user protocol GetUserByFacebookID, Error Message: {0} Parameters :FaceBook Id : {1} ", ex.Message, FacbookId);
                return ResponseStatus.UserDoesNotExist;
            }
        }

        public bool CheckFacebookUserExist(string FacbookId)
        {
            try
            {
                UserResponseObject CurrentUser = PlatUserService.GetUserByFacebookID(wsUserName, wsPassword, FacbookId);

                if (CurrentUser != null && CurrentUser.m_RespStatus == ResponseStatus.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured recive facebook user protocol GetUserByFacebookID, Error Message: {0} Parameters :FaceBook Id : {1} ", ex.Message, FacbookId);
                return false;
            }
        }

        /// <summary>
        /// The list should contain FacebookID,FacebookImage and IsFacebookImagePermitted
        /// The user name is the same user name as the user has on facebook
        /// </summary>
        /// <param name="FacebookFields"></param>
        /// <returns></returns>
        public bool MergeWithFacebookAccount(string FacebookUserId, string UserPic, bool AutoLogin, string UserName)
        {
            UserContext = null; // to be ensure we start a new context
            bool IsMerged = false;

            try
            {
                if (!string.IsNullOrEmpty(FacebookUserId))
                {

                    UserContext.UserResponse = PlatUserService.GetUserByUsername(wsUserName, wsPassword, UserName);

                    if (UserContext.UserResponse.m_RespStatus == TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK)
                    {
                        UserContext.UserResponse.m_user.m_oBasicData.m_sFacebookID = FacebookUserId;
                        if (!string.IsNullOrEmpty(UserPic))
                        {
                            UserContext.UserResponse.m_user.m_oBasicData.m_sFacebookImage = UserPic;
                        }

                        UserContext.UserResponse.m_user.m_oBasicData.m_bIsFacebookImagePermitted = true;


                        UserContext.UserResponse = PlatUserService.SetUserData(wsUserName, wsPassword, UserContext.UserResponse.m_user.m_sSiteGUID,
                            UserContext.UserResponse.m_user.m_oBasicData, UserContext.UserResponse.m_user.m_oDynamicData);


                        if (AutoLogin)
                        {
                            //Set user status;
                            if (CookiesHelper.Enabled())
                            {
                                CookiesHelper cookie = new CookiesHelper("RememberUser") { Expires = DateTime.MaxValue };
                                cookie.SetValue("UserId", UserContext.UserResponse.m_user.m_sSiteGUID);
                                cookie.SetValue("UserName", UserContext.UserResponse.m_user.m_oBasicData.m_sUserName);
                            }
                            UserContext.OnlineStatus = Enums.eUserOnlineStatus.Recognised;
                        }

                        IsMerged = true;
                    }
                }


            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error merge facebook user protocol SetUserData, Error Message: {0} Parameters :FaceBook Id : {1} ", ex.Message, FacebookUserId);
            }

            return IsMerged;
        }

        public bool IsUserLinkedWithFacebook(string username)
        {
            try
            {
                UserResponseObject userObj = PlatUserService.GetUserByUsername(wsUserName, wsPassword, username);
                return !string.IsNullOrEmpty(userObj.m_user.m_oBasicData.m_sFacebookID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error IsUserLinkedWithFacebook, Error Message: {0} Parameters :FaceBook Id : {1} ", ex.Message, username);
            }

            return false;
        }

        public bool MergeSigninUserToFacebookAccount(string FacebookUserId, string UserPic)
        {
            try
            {
                if (UserContext != null && UserContext.UserResponse != null)
                {
                    if (!string.IsNullOrEmpty(FacebookUserId))
                        UserContext.UserResponse.m_user.m_oBasicData.m_sFacebookID = FacebookUserId;
                    if (!string.IsNullOrEmpty(UserPic))
                        UserContext.UserResponse.m_user.m_oBasicData.m_sFacebookImage = UserPic;

                    PlatUserService.SetUserData(wsUserName, wsPassword, UserContext.UserResponse.m_user.m_sSiteGUID, UserContext.UserResponse.m_user.m_oBasicData, UserContext.UserResponse.m_user.m_oDynamicData);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error merge facebook user protocol SetUserData, Error Message: {0} Parameters :FaceBook Id : {1} ", ex.Message, FacebookUserId);
                return false;
            }
        }

        public UserResponseObject SetUserData(string sSiteGUID, UserBasicData userBasicData, UserDynamicData userDynamicData)
        {
            UserResponseObject uroRet = null;
            try
            {
                uroRet = PlatUserService.SetUserData(wsUserName, wsPassword, sSiteGUID, userBasicData, userDynamicData);
                if (uroRet != null && uroRet.m_RespStatus == ResponseStatus.OK)
                {
                    UserContext.UserResponse = uroRet;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error protocol SetUserData, Error Message: {0} ", ex.Message);
            }

            return uroRet;
        }

        public Dictionary<int, string> GetCountriesDictionary()
        {
            try
            {
                //Bring countries list from ws only when country oject is empty
                if (m_dictCountries != null && m_dictCountries.Count == 0)
                {
                    TvinciPlatform.Users.Country[] Countries = PlatUserService.GetCountryList(wsUserName, wsPassword);
                    if (Countries != null)
                    {
                        m_dictCountries = Countries.ToDictionary(c => c.m_nObjecrtID,
                            c => c.m_sCountryName.ToString());
                    }
                }

                return m_dictCountries;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive countries list Protocol GetCountryList, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}: ", ex.Message, wsUserName, wsPassword);
            }

            return null;
        }

        public Dictionary<string, RegionData> GetCountriesList()
        {
            try
            {
                //Bring countries list from ws only when country oject is empty
                if (m_dictCountriesList == null || m_dictCountriesList.Count == 0)
                {
                    m_dictCountriesList = new Dictionary<string, RegionData>();

                    TvinciPlatform.Users.Country[] Countries = PlatUserService.GetCountryList(wsUserName, wsPassword);

                    if (Countries != null)
                    {
                        foreach (Country country in Countries)
                        {
                            if (!m_dictCountriesList.Keys.Contains(country.m_sCountryName))
                            {
                                RegionData data = new RegionData();
                                data.Name = country.m_sCountryName;
                                data.Code = country.m_sCountryCode;
                                data.ID = country.m_nObjecrtID.ToString();

                                m_dictCountriesList.Add(data.Name, data);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive countries code Protocol GetCountryList, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}: ", ex.Message, wsUserName, wsPassword);
            }

            return m_dictCountriesList;
        }

        /// <summary>
        /// return Dictionary with key as CountryName and List of states RegionData for each from CountryID
        /// </summary>
        /// <param name="sCountryName"></param>
        /// <returns></returns>
        public List<RegionData> GetStatesList(int iCountryID)
        {
            List<RegionData> lstRet = new List<RegionData>();

            try
            {
                TvinciPlatform.Users.State[] States = PlatUserService.GetStateList(wsUserName, wsPassword, iCountryID);

                if (States != null)
                {
                    foreach (State state in States)
                    {
                        RegionData data = new RegionData();
                        data.Name = state.m_sStateName;
                        data.Code = state.m_sStateCode;
                        data.ID = state.m_nObjecrtID.ToString();

                        if (!lstRet.Contains(data))
                        {
                            lstRet.Add(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive states code Protocol GetStateList, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}: ", ex.Message, wsUserName, wsPassword);
            }

            return lstRet;
        }

        public Country GetIPToCountry(string ip)
        {
            try
            {
                return PlatUserService.GetIPToCountry(wsUserName, wsPassword, ip);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive states list Protocol GetStateList, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, ip {3} ", ex.Message, wsUserName, wsPassword, ip);
            }
            return null;
        }

        public bool IsOfflineModeEnabled()
        {
            var offlineMode = UsersService.Instance.GetUserData(UsersService.Instance.GetUserID(), TVPPro.SiteManager.Helper.SiteHelper.GetClientIP()).m_user.m_oDynamicData.m_sUserData.Where(x => x.m_sDataType == "IsOfflineMode" && x.m_sValue == "true").FirstOrDefault();

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

        public void ToggleOfflineMode()
        {
            if (!IsOfflineModeEnabled())
            {
                var userData = UsersService.Instance.GetUserData(UsersService.Instance.GetUserID(), TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
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

                UsersService.Instance.SetUserData(UsersService.Instance.GetUserID(), userData.m_user.m_oBasicData, newDynamicData);
            }
            else
            {
                var userData = UsersService.Instance.GetUserData(UsersService.Instance.GetUserID(), TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
                var curDynamicData = userData.m_user.m_oDynamicData;
                var newDynamicData = cloneDynamicData(curDynamicData, false);

                newDynamicData.m_sUserData.Where(x => x.m_sDataType == "IsOfflineMode").First().m_sValue = "false";
                UsersService.Instance.SetUserData(UsersService.Instance.GetUserID(), userData.m_user.m_oBasicData, newDynamicData);
            }
        }

        public string GetUserNickName()
        {
            string NickName = string.Empty;
            if (UserContext != null)
            {
                if (UserContext.OnlineStatus == Enums.eUserOnlineStatus.LoggedIn)
                {
                    if (UserContext.UserResponse != null && UserContext.UserResponse.m_user != null && UserContext.UserResponse.m_user.m_oDynamicData != null
                        && UserContext.UserResponse.m_user.m_oDynamicData.m_sUserData != null)
                    {
                        NickName = (from tt in UsersService.Instance.UserContext.UserResponse.m_user.m_oDynamicData.m_sUserData
                                    where tt.m_sDataType == "NickName"
                                    select tt.m_sValue).FirstOrDefault();
                    }
                }
                else if (UserContext.OnlineStatus == Enums.eUserOnlineStatus.Recognised)
                {
                    GetCookieByKey("NickName");
                }
                else
                    return string.Empty;
            }

            return NickName;
        }


        //public List<string> GetStatesList(int CountryId)
        //{
        //    List<string> StatesList = new List<string>();
        //    try
        //    {
        //        TvinciPlatform.Users.State[] States = PlatUserService.GetStateList(wsUserName, wsPassword, CountryId);
        //        if (States != null && States.Length > 0)
        //        {
        //            StatesList = (from s in States
        //                          select s.m_sStateName).ToList<string>();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error recive states list Protocol GetStateList, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, Country {3} ", ex.Message, wsUserName, wsPassword, CountryId.ToString());
        //    }

        //    return StatesList;
        //}

        public UserResponseObject GetUserData(string sSiteGUID, string sUserIP)
        {
            UserResponseObject response = null;
            try
            {
                response = PlatUserService.GetUserData(wsUserName, wsPassword, sSiteGUID, sUserIP);
                //UserContext.UserResponse = response;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUserData, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, sSiteGUID);
            }

            return response;
        }

        public UserResponseObject[] GetUsersData(string[] sSiteGUID)
        {
            UserResponseObject[] response = null;
            try
            {
                response = PlatUserService.GetUsersData(wsUserName, wsPassword, sSiteGUID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUsersData, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, sSiteGUID);
            }

            return response;
        }

        public bool SetUserDynamicData(string sSiteGUID, string key, string value)
        {
            bool response = false;
            try
            {
                response = PlatUserService.SetUserDynamicData(wsUserName, wsPassword, sSiteGUID, key, value);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUserDynamicData, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, sSiteGUID);
            }

            return response;
        }

        public UserResponseObject GetUserDataByUsernamePassword(string UserName, string Password)
        {
            UserResponseObject response = null;
            try
            {
                response = PlatUserService.CheckUserPassword(wsUserName, wsPassword, UserName, Password, false);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in SignIn module CheckUserPassword, Error : {0} Parameters : User : {1}", ex.Message, UserName);
            }
            return response;
        }

        public void Hit()
        {
            try
            {
                PlatUserService.Hit(wsUserName, wsPassword, GetUserID());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol Hit, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, GetUserID());
            }
        }

        public FavoritObject[] GetUserFavorite(string sMediaType, int DomainId, string DeviceId)
        {
            FavoritObject[] UserFavorites = null;
            try
            {
                var res = PlatUserService.GetUserFavorites(wsUserName, wsPassword, GetUserID(), DomainId, DeviceId, sMediaType);
                UserFavorites = res.Favorites;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUserFavorites, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, GetUserID());
            }

            return UserFavorites;
        }

        public bool AddToUserFavorite(string sMediaType, string sMediaID, string sExtraData, string DeviceId)
        {
            bool IsAdded = false;

            try
            {
                var res = PlatUserService.AddUserFavorit(wsUserName, wsPassword, GetUserID(), GetDomainID(), DeviceId, sMediaType, sMediaID, sExtraData);
                if (res != null && res.Code == 0) // TVPApiModule.Objects.Responses.eStatus ( 0 = OK ) 
                {
                    IsAdded = true;
                }
                else
                {
                    IsAdded = false;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol AddUserFavorit, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, GetUserID());
            }

            return IsAdded;
        }

        public void ClearOfflineList()
        {
            try
            {
                PlatUserService.ClearUserOfflineAssets(wsUserName, wsPassword, GetUserID());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol ClearOfflineList, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, GetUserID());
            }
        }

        public bool AddOfflineAssets(string mediaID)
        {
            bool res = false;

            try
            {
                res = PlatUserService.AddUserOfflineAsset(wsUserName, wsPassword, GetUserID(), mediaID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol AddOfflineAssets, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, GetUserID());
            }

            return res;
        }

        public bool RemoveOfflineAsset(string mediaID)
        {
            bool res = false;

            try
            {
                res = PlatUserService.RemoveUserOfflineAsset(wsUserName, wsPassword, GetUserID(), mediaID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol RemoveOfflineAsset, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, GetUserID());
            }

            return res;
        }

        public bool AddChannelToUserFavorite(string sChannelID)
        {
            bool IsAdded = false;

            try
            {
                IsAdded = PlatUserService.AddChannelMediaToFavorites(wsUserName, wsPassword, GetUserID(), GetDomainID(), string.Empty, "0", sChannelID, string.Empty);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol AddChannelToUserFavorite, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, GetUserID());
            }

            return IsAdded;
        }

        public bool RemoveChannelFromFavorite(string sChannelID)
        {
            bool IsRemoved = false;

            try
            {
                PlatUserService.RemoveChannelMediaUserFavorit(wsUserName, wsPassword, GetUserID(), new int[] { int.Parse(sChannelID) });
                IsRemoved = true;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol AddUserFavorit, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, GetUserID());
            }

            return IsRemoved;
        }

        public bool RemoveUserFavorite(int iFavoriteID)
        {
            bool IsRemoved = false;
            int[] FavoriteItems = new int[1] { iFavoriteID };

            try
            {
                PlatUserService.RemoveUserFavorit(wsUserName, wsPassword, GetUserID(), FavoriteItems);
                IsRemoved = true;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol AddUserFavorit, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, GetUserID());
            }

            return IsRemoved;
        }

        public bool RemoveUserFavoriteItems(int[] iMediaIds)
        {
            bool IsRemoved = true;

            try
            {
                PlatUserService.RemoveUserFavorit(wsUserName, wsPassword, GetUserID(), iMediaIds);
            }
            catch (Exception ex)
            {
                IsRemoved = false;
                logger.ErrorFormat("Error recive user data Protocol AddUserFavorit, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, GetUserID());
            }

            return IsRemoved;
        }


        public User[] GetUsersLikedMedia(int iSiteGuid, int iMediaID, int iPlatform, bool bOnlyFriends, int iStartIndex, int iPageSize)
        {
            User[] users = null;
            try
            {
                users = PlatUserService.GetUsersLikedMedia(wsUserName, wsPassword, iSiteGuid, iMediaID, iPlatform, bOnlyFriends, iStartIndex, iPageSize);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetUsersLikedMedia, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword, iSiteGuid);
            }

            return users;
        }


        public TVPPro.SiteManager.TvinciPlatform.Users.UserType[] GetGroupUserTypes(string sWSUserName, string sWSPassword)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserType[] userTypes = null;
            try
            {
                userTypes = PlatUserService.GetGroupUserTypes(sWSUserName, sWSPassword);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user data Protocol GetGroupUserTypes, Error Message: {0} Parameters :ws User name : {1} , ws Password {2}:, SiteGUID {3} ", ex.Message, wsUserName, wsPassword);
            }

            return userTypes;


        }


        public void RefreshUserData()
        {
            if (UserContext.UserResponse != null && UserContext.UserResponse.m_user != null && !string.IsNullOrEmpty(UserContext.UserResponse.m_user.m_sSiteGUID))
                UserContext.UserResponse = GetUserData(UserContext.UserResponse.m_user.m_sSiteGUID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
        }

        public UserGroupRuleResponse CheckParentalPINToken(string sChangePinToken)
        {
            UserGroupRuleResponse res = null;
            try
            {
                res = PlatUserService.CheckParentalPINToken(wsUserName, wsPassword, sChangePinToken);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CheckParentalPINToken, Error Message: {0}, Parameters :ws User name : {1} , ws Password {2}: ChangePinToken : {3}", ex.Message, wsUserName, wsPassword, sChangePinToken);
            }
            return res;
        }

        public virtual ResponseStatus SendChangedPinMail(string sSiteGuid, int nUserRuleID)
        {
            ResponseStatus res = default(ResponseStatus);
            try
            {
                res = PlatUserService.SendChangedPinMail(wsUserName, wsPassword, sSiteGuid, nUserRuleID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SendChangedPinMail, Error Message: {0}, Parameters :ws User name : {1} , ws Password {2}: SiteGuid : {3} ,  UserRuleID : {4} ", ex.Message, wsUserName, wsPassword, sSiteGuid, nUserRuleID);
            }
            return res;
        }

        public virtual UserGroupRuleResponse ChangeParentalPInCodeByToken(string sSiteGuid, int nUserRuleID, string sChangePinToken, string sCode)
        {
            UserGroupRuleResponse res = null;
            try
            {
                res = PlatUserService.ChangeParentalPInCodeByToken(wsUserName, wsPassword, sSiteGuid, nUserRuleID, sChangePinToken, sCode);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChangeParentalPInCodeByToken, Error Message: {0}, Parameters :ws User name : {1} , ws Password {2}: SiteGuid : {3} ,  UserRuleID : {4} ,  ChangePinToken: {5} , Code: {6}  ", ex.Message, wsUserName, wsPassword, sSiteGuid, nUserRuleID, sChangePinToken, sCode);
            }
            return res;

        }

        public bool SendPasswordMail(string userName)
        {
            bool res = false;
            try
            {
                res = PlatUserService.SendPasswordMail(wsUserName, wsPassword, userName);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SendPasswordMail, Error Message: {0}, Parameters :ws User name : {1} , ws Password {2}: UserName : {3}", ex.Message, wsUserName, wsPassword, userName);
            }
            return res;
        }

        public bool SignInWithToken(string sToken, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins)
        {
            bool res = false;
            try
            {
                UserContext.UserResponse = PlatUserService.SignInWithToken(wsUserName, wsPassword, sToken, sSessionID, sIP, sDeviceID, bPreventDoubleLogins);
                res = UserContext.OnlineStatus == Enums.eUserOnlineStatus.LoggedIn;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SignInWithToken, Error Message: {0}, Parameters : ws User name : {1}, ws Password: {2}, token: {3}", ex.Message, wsUserName, wsPassword, sToken);
            }
            return res;
        }

        #endregion

        #region Private Methods
        private bool CreateNewUser()
        {
            bool result = true;
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("Accounts");

            try
            {
                long TvinciID = 0;
                long.TryParse(UserContext.UserResponse.m_user.m_sSiteGUID, out TvinciID);

                if (TvinciID != 0)
                {
                    DateTime CurrentTime = DateTime.Now;

                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TvinciID", "=", TvinciID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ACCOUNT_TYPE", "=", 1);

                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", CurrentTime);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", CurrentTime);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("HIT_DATE", "=", CurrentTime);

                    if (!insertQuery.Execute())
                    {
                        result = false;
                        logger.ErrorFormat("SignUp - Error, TvinciID={0}", TvinciID.ToString());
                    }
                }
                else
                    result = false;
            }
            catch
            {
                result = false;
            }
            finally
            {
                insertQuery.Finish();
                insertQuery = null;
            }

            return result;
        }

        private string GetUserIdOnCookie()
        {
            CookiesHelper cookie = new CookiesHelper("RememberUser");
            string sGuid = cookie.GetValue("UserId");
            if (!string.IsNullOrEmpty(sGuid))
            {
                //Login the user in case he is RememberMe & Anonymous
                if (UserContext.OnlineStatus == Enums.eUserOnlineStatus.Recognised)
                {
                    UserContext.UserResponse = GetUserData(sGuid, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
                    if (UserContext.UserResponse.m_user.m_oDynamicData.m_sUserData != null &&
                        UserContext.UserResponse.m_user.m_oDynamicData.m_sUserData.Length > 0 &&
                        UserContext.UserResponse.m_user.m_oDynamicData.m_sUserData[0].m_sDataType.Equals("IsAnonymous"))
                    {
                        AutoSignIn(sGuid, HttpContext.Current.Session.SessionID, TVPPro.SiteManager.Helper.SiteHelper.GetClientIP());
                    }
                }
                return cookie.GetValue("UserId");
            }
            else
            {
                return "0";
            }
        }

        public void SetUserIdOnCookie(string userId, string userName, DateTime exp)
        {
            CookiesHelper cookie = new CookiesHelper("RememberUser") { Expires = exp };
            cookie.SetValue("UserId", userId);
            cookie.SetValue("UserName", userName);
        }

        private string GetCookieByKey(string key)
        {
            CookiesHelper cookie = new CookiesHelper("RememberUser");
            if (!string.IsNullOrEmpty(cookie.GetValue(key)))
            {
                return cookie.GetValue(key);
            }
            else
            {
                return string.Empty;
            }
        }

        public int GetDomainID()
        {
            if (UserContext.OnlineStatus == Enums.eUserOnlineStatus.LoggedIn)
            {
                return UserContext.UserResponse.m_user.m_domianID;
            }
            //else if (UserContext.OnlineStatus == Enums.eUserOnlineStatus.Recognised)
            //{
            //    return GetUserIdOnCookie();
            //}
            else
                return 0;
        }
        #endregion
    }

    #region UserContext
    [Serializable]
    public class UserContext
    {

        private string m_UserIP;

        #region Constructor
        public UserContext()
        {
            if (!string.IsNullOrEmpty(GetUserIdOnCookie()))
            {
                OnlineStatus = Enums.eUserOnlineStatus.Recognised;
            }
            else
            {
                OnlineStatus = Enums.eUserOnlineStatus.LoggedOut;
            }

            if (string.IsNullOrEmpty(m_UserIP) && HttpContext.Current != null)
                m_UserIP = TVPPro.SiteManager.Helper.SiteHelper.GetClientIP();

            UserResponse = new TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject();

        }
        #endregion

        #region Properties
        private Enums.eUserOnlineStatus m_OnlineStatus;
        internal Enums.eUserOnlineStatus OnlineStatus
        {
            get
            {
                return m_OnlineStatus;
            }
            set
            {
                m_OnlineStatus = value;
            }
        }

        public string UserIP
        {
            get
            {
                return m_UserIP;
            }
        }

        private TvinciPlatform.Users.UserResponseObject m_UserResponse;
        public TvinciPlatform.Users.UserResponseObject UserResponse
        {
            get
            {
                return m_UserResponse;
            }

            internal set
            {
                m_UserResponse = value;
                switch (value.m_RespStatus)
                {
                    case TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK:
                        if (value.m_user != null)
                            OnlineStatus = Enums.eUserOnlineStatus.LoggedIn;
                        else if (OnlineStatus != Enums.eUserOnlineStatus.Recognised)
                            OnlineStatus = Enums.eUserOnlineStatus.Error;
                        break;
                    case TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.UserNotActivated:
                        OnlineStatus = Enums.eUserOnlineStatus.NotActive;
                        break;
                    case TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.InsideLockTime:
                        OnlineStatus = Enums.eUserOnlineStatus.Locked;
                        break;
                    case TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.WrongPasswordOrUserName:
                        OnlineStatus = Enums.eUserOnlineStatus.NotValidInfo;
                        break;
                    case TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.UserDoesNotExist:
                        OnlineStatus = Enums.eUserOnlineStatus.UserDoesNotExist;
                        break;
                    case TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.UserDoubleLogIn:
                        OnlineStatus = Enums.eUserOnlineStatus.UserAllreadyLoggedIn;
                        break;
                    default:
                        if (OnlineStatus != Enums.eUserOnlineStatus.Recognised)
                            OnlineStatus = Enums.eUserOnlineStatus.Error;
                        break;
                }
            }
        }

        private PermittedMediaContainer[] m_UserPurchasedMedias = new PermittedMediaContainer[] { };
        public PermittedMediaContainer[] UserPurchasedMedias
        {
            get
            {
                return m_UserPurchasedMedias;
            }
        }
        #endregion

        private string GetUserIdOnCookie()
        {
            CookiesHelper cookie = new CookiesHelper("RememberUser");
            if (!string.IsNullOrEmpty(cookie.GetValue("UserId")))
            {
                return cookie.GetValue("UserId");
            }
            else
            {
                return string.Empty;
            }
        }


    }
    #endregion
}
