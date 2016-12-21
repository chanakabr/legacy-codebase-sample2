using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPApiModule.Interfaces;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.TvinciPlatform.Social;
using System.Configuration;
using TVPApiModule.Objects;
using TVPApiModule.Helper;
using System.Web.UI;
using System.Web;
using TVPApiModule.Objects;
using TVPApiModule.Manager;
using TVPApiModule.Objects.Authorization;
using TVPApiModule.Objects.Responses;
using KLogMonitor;
using System.Reflection;


namespace TVPApiServices
{
    /// <summary>
    /// Summary description for Service
    /// </summary>
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class SiteService : System.Web.Services.WebService, ISiteService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region SiteMap

        //Get complete user site map - retrieve on first time from DB for each new groupID. Next calls will get ready site map
        [WebMethod(EnableSession = true, Description = "Get complete user site map - retrieve on first time from DB for each new groupID. Next calls will get ready site map")]
        public TVPApi.SiteMap GetSiteMap(InitializationObject initObj)
        {
            TVPApi.SiteMap retSiteMap = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSiteMap", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retSiteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
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

            return retSiteMap;
        }

        #endregion

        #region Page

        //Get specific page from site map
        [WebMethod(EnableSession = true, Description = "Get specific page from site map")]
        public PageContext GetPage(InitializationObject initObj, long ID, bool withMenu, bool withFooter)
        {
            PageContext retPageContext = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPage", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retPageContext = PageDataHelper.GetPageContextByID(initObj, groupID, ID, withMenu, withFooter);
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

            return retPageContext;
        }

        //Get specific page from site map
        [WebMethod(EnableSession = true, Description = "Get specific page from site map")]
        public PageContext GetPageByToken(InitializationObject initObj, Pages token, bool withMenu, bool withFooter)
        {
            PageContext retPageContext = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPage", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retPageContext = PageDataHelper.GetPageContextByToken(initObj, groupID, token, withMenu, withFooter);
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

            return retPageContext;
        }

        [WebMethod(EnableSession = true, Description = "Get site menu")]
        public Menu GetMenu(InitializationObject initObj, long ID)
        {
            Menu retMenu = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMenu", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retMenu = MenuHelper.GetMenuByID(initObj, ID, groupID);
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

            return retMenu;
        }

        [WebMethod(EnableSession = true, Description = "Get site footer menu")]
        public Menu GetFooter(InitializationObject initObj, long ID)
        {
            Menu retMenu = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetFooter", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retMenu = MenuHelper.GetFooterByID(initObj, ID, groupID);
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

            return retMenu;
        }

        [WebMethod(EnableSession = true, Description = "Get site side galleries")]
        public Profile GetSideProfile(InitializationObject initObj, long ID)
        {
            Profile retProfile = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSideProfile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retProfile = ProfileHelper.GetSideProfile(initObj, ID, groupID);
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

            return retProfile;
        }

        //Get full bottom profile from site map
        [WebMethod(EnableSession = true, Description = "Get full bottom profile from site map")]
        public Profile GetBottomProfile(InitializationObject initObj, long ID)
        {
            Profile retProfile = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetBottomProfile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retProfile = ProfileHelper.GetBottomProfile(initObj, ID, groupID);
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

            return retProfile;
        }
        #endregion

        #region Galleries

        //Get all page galleries from site map
        [WebMethod(EnableSession = true, Description = "Get all page galleries from site map")]
        public List<PageGallery> GetPageGalleries(InitializationObject initObj, long PageID, int pageSize, int start_index)
        {
            List<PageGallery> lstPageGallery = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPageGalleries", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstPageGallery = PageGalleryHelper.GetPageGallerisByPageID(initObj, PageID, groupID, pageSize, start_index);
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

            return lstPageGallery;
        }

        //Get all page galleries from site map
        [WebMethod(EnableSession = true, Description = "Get all page galleries from site map")]
        public PageGallery GetGallery(InitializationObject initObj, long galleryID, long PageID)
        {
            PageGallery retPageGallery = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPageGalleries", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    ODBCWrapper.Connection.GetDefaultConnectionStringMethod = ConnectionHelper.GetClientConnectionString;
                    retPageGallery = PageGalleryHelper.GetGalleryByID(initObj, galleryID, PageID, groupID);
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

            return retPageGallery;
        }

        //Get all gallery items for a specific gallery
        [WebMethod(EnableSession = true, Description = "Get all gallery items for a specific gallery")]
        public List<GalleryItem> GetGalleryContent(InitializationObject initObj, long ID, long PageID, string picSize, int pageSize, int start_index)
        {
            List<GalleryItem> lstGalleryItem = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGalleryContent", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    lstGalleryItem = PageGalleryHelper.GetGalleryContent(initObj, ID, PageID, picSize, groupID);
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

            return lstGalleryItem;
        }


        //Get content from specific gallery items
        [WebMethod(EnableSession = true, Description = "Get content from specific gallery items")]
        public List<Media> GetGalleryItemContent(InitializationObject initObj, long ItemID, long GalleryID, long PageID, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGalleryContent", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //XXX: Patch for ximon
                    if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("v1_6/") && groupID == 109 && initObj.Platform == PlatformType.iPad)
                        pageIndex = pageIndex / pageSize;

                    lstMedia = PageGalleryHelper.GetGalleryItemContent(initObj, PageID, GalleryID, ItemID, picSize, groupID, pageSize, pageIndex, orderBy);
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

            return lstMedia;
        }

        #endregion

        #region User
        [WebMethod(EnableSession = true, Description = "Get Group Rules")]
        public TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetGroupRules(InitializationObject initObj)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetGroupRules();
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

        [WebMethod(EnableSession = true, Description = "Get User Group Rules")]
        public TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetUserGroupRules(InitializationObject initObj)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetUserGroupRules(initObj.SiteGuid);
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

        [WebMethod(EnableSession = true, Description = "Set User Group Rule")]
        [PrivateMethod]
        public bool SetUserGroupRule(InitializationObject initObj, int ruleID, string PIN, int isActive, string siteGuid)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, siteGuid, 0, null, groupID, initObj.Platform))
                {
                    return false;
                }
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetUserGroupRule(initObj.SiteGuid, ruleID, PIN, isActive);
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

        [WebMethod(EnableSession = true, Description = "Check Parental PIN")]
        [PrivateMethod]
        public bool CheckParentalPIN(InitializationObject initObj, int ruleID, string PIN)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CheckParentalPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).CheckParentalPIN(initObj.SiteGuid, ruleID, PIN);
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

        [WebMethod(EnableSession = true, Description = "Get Secured SiteGuid")]
        [PrivateMethod]
        public string GetSecuredSiteGuid(InitializationObject initObj)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSecuredSiteGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string privateKey = ConfigurationManager.AppSettings["SecureSiteGuidKey"];
                    string IV = ConfigurationManager.AppSettings["SecureSiteGuidIV"];
                    sRet = SecurityHelper.EncryptSiteGuid(privateKey, IV, initObj.SiteGuid);
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

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Get Group Operators")]
        public TVPPro.SiteManager.TvinciPlatform.api.GroupOperator[] GetGroupOperators(InitializationObject initObj, string scope)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupOperator[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupOperators", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetGroupOperators(scope);
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

        [WebMethod(EnableSession = true, Description = "Get Operators")]
        public TVPPro.SiteManager.TvinciPlatform.api.GroupOperator[] GetOperators(InitializationObject initObj, int[] operators)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupOperator[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupOperators", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetOperators(operators, initObj.Platform);
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

        [WebMethod(EnableSession = true, Description = "SSO Signin")]
        public TVPApiModule.Services.ApiUsersService.LogInResponseData SSOSignIn(InitializationObject initObj, string userName, string password, int providerID)
        {
            TVPApiModule.Services.ApiUsersService.LogInResponseData response = default(TVPApiModule.Services.ApiUsersService.LogInResponseData);

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SSOSignIn", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SSOSignIn(userName, password, providerID, string.Empty, SiteHelper.GetClientIP(), initObj.UDID, false);

                    // if sign in successful and tokenization enabled - generate access token and add it to headers
                    AuthorizationManager.Instance.AddTokenToHeadersForValidNotAdminUser(response, groupID, initObj.UDID, initObj.Platform);
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

        [WebMethod(EnableSession = true, Description = "Check SSO Login")]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject SSOCheckLogin(InitializationObject initObj, string userName, int providerID)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SSOCheckLogin", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SSOCheckLogin(userName, providerID);
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

        [WebMethod(EnableSession = true, Description = "Get user data by co-guid")]
        [PrivateMethod]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject GetUserDataByCoGuid(InitializationObject initObj, string coGuid, int operatorID)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserDataByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserDataByCoGuid(coGuid, operatorID);
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

        [WebMethod(EnableSession = true, Description = "Get from Secured SiteGuid")]
        [PrivateMethod]
        public string GetSiteGuidFromSecured(InitializationObject initObj, string encSiteGuid)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSiteGuidFromSecured", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string privateKey = ConfigurationManager.AppSettings["SecureSiteGuidKey"];
                    string IV = ConfigurationManager.AppSettings["SecureSiteGuidIV"];
                    sRet = SecurityHelper.DecryptSiteGuid(privateKey, IV, encSiteGuid);

                    // Tokenization: validate siteGuid
                    if (AuthorizationManager.IsTokenizationEnabled() &&
                        !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sRet, 0, null, groupID, initObj.Platform))
                    {
                        return null;
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

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Validate user")]
        public string GetSiteGuid(InitializationObject initObj, string userName, string password)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.User sRet = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ValidateUser", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    sRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ValidateUser(userName, password, isSingleLogin).m_user;
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

            return sRet != null ? sRet.m_sSiteGUID : string.Empty;
        }

        [WebMethod(EnableSession = true, Description = "Sign-In a user")]
        public TVPApiModule.Services.ApiUsersService.LogInResponseData SignIn(InitializationObject initObj, string userName, string password)
        {
            TVPApiModule.Services.ApiUsersService.LogInResponseData responseData = new TVPApiModule.Services.ApiUsersService.LogInResponseData();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignIn", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

                if (groupID > 0)
            {
                try
                {
                    //XXX: Do the UDID empty stuff
                    IImplementation impl = WSUtils.GetImplementation(groupID, initObj);
                    responseData = impl.SignIn(userName, password);

                    // if sign in successful and tokenization enabled - generate access token and add it to headers
                    AuthorizationManager.Instance.AddTokenToHeadersForValidNotAdminUser(responseData, groupID,initObj.UDID, initObj.Platform);
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

            return responseData;
        }



        [WebMethod(EnableSession = true, Description = "Sign-In a user")]
        public TVPApiModule.Services.ApiUsersService.LogInResponseData SignInWithToken(InitializationObject initObj, string token)
        {
            TVPApiModule.Services.ApiUsersService.LogInResponseData responseData = new TVPApiModule.Services.ApiUsersService.LogInResponseData();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    responseData = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SignInWithToken(token, HttpContext.Current.Session.SessionID, SiteHelper.GetClientIP(), initObj.UDID, isSingleLogin);

                    // if sign in successful and tokenization enabled - generate access token and add it to headers
                    AuthorizationManager.Instance.AddTokenToHeadersForValidNotAdminUser(responseData, groupID, initObj.UDID, initObj.Platform);
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

            return responseData;
        }

        [WebMethod(EnableSession = true, Description = "Has user connected to FB")]
        [PrivateMethod]
        public bool IsFacebookUser(InitializationObject initObj)
        {
            bool bRes = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsFacebookUser", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject userObj = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserData(initObj.SiteGuid);
                    bRes = !string.IsNullOrEmpty(userObj.m_user.m_oBasicData.m_sFacebookID);
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

            return bRes;
        }

        [WebMethod(EnableSession = true, Description = "Sign-Up a new user")]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject SignUp(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData,
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject response = new TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignUp", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SignUp(userBasicData, userDynamicData, sPassword, sAffiliateCode);
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

        [WebMethod(EnableSession = true, Description = "Sign-Out a user")]
        [PrivateMethod]
        public void SignOut(InitializationObject initObj)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignOut", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //XXX: Do the UDID empty stuff
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SignOut(initObj.SiteGuid, initObj.UDID, string.Empty, isSingleLogin);

                    AuthorizationManager.Instance.DeleteAccessToken(initObj.Token);
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

        [WebMethod(EnableSession = true, Description = "Check if user is signed in")]
        [PrivateMethod]
        public bool IsUserSignedIn(InitializationObject initObj)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsUserSignedIn", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //XXX: Do the UDID empty stuff
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    bRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IsUserLoggedIn(initObj.SiteGuid, System.Web.HttpContext.Current.Session.SessionID, initObj.UDID, SiteHelper.GetClientIP(), isSingleLogin);
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

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Edit user details info")]
        [PrivateMethod]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject SetUserData(InitializationObject initObj, string sSiteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData,
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject response = new TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sSiteGuid, 0, null, groupID, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserData(sSiteGuid, userBasicData, userDynamicData);
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

        [WebMethod(EnableSession = true, Description = "Get user details info")]
        [PrivateMethod]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject GetUserData(InitializationObject initObj, string sSiteGuid)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject response = new TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sSiteGuid, 0, null, groupID, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    string siteGuid = (string.IsNullOrEmpty(sSiteGuid)) ? initObj.SiteGuid : sSiteGuid;
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserData(siteGuid);
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

        [WebMethod(EnableSession = true, Description = "Get users details info")]
        [PrivateMethod]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject[] GetUsersData(InitializationObject initObj, string sSiteGuid)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUsersData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {

                try
                {
                    string[] siteGuids = sSiteGuid.Split(';');
                    // Tokenization: validate multiple siteGuids
                    if (AuthorizationManager.IsTokenizationEnabled() &&
                        !AuthorizationManager.Instance.ValidateMultipleSiteGuids(initObj.SiteGuid, siteGuids, groupID, initObj.Platform))
                    {
                        return null;
                    }

                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUsersData(siteGuids);
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

        [WebMethod(EnableSession = true, Description = "Get user CA status")]
        public TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus GetUserCAStatus(InitializationObject initObj)
        {
            TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus response = TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.Annonymus;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserCAStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiConditionalAccessService(groupID, initObj.Platform).GetUserCAStatus(initObj.SiteGuid);
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

        [WebMethod(EnableSession = true, Description = "Forgot password")]
        public bool SendNewPassword(InitializationObject initObj, string sUserName)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SendNewPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    bRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SentNewPasswordToUser(sUserName);
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

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Set specific dynamic user key data")]
        [PrivateMethod]
        public bool SetUserDynamicData(InitializationObject initObj, string sKey, string sValue)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserDynamicData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    bRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserDynamicData(initObj.SiteGuid, sKey, sValue);
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

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Clean User History")]
        [PrivateMethod]
        public bool CleanUserHistory(InitializationObject initObj, int[] mediaIDs)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CleanUserHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    bRet = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).CleanUserHistory(initObj.SiteGuid, mediaIDs);
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

            return bRet;
        }
        #endregion

        #region XXXX

        [WebMethod(EnableSession = true, Description = "Do Social Action")]
        [PrivateMethod]
        public DoSocialActionResponse DoSocialAction(InitializationObject initObj, int mediaID, eUserAction socialAction, SocialPlatform socialPlatform, string actionParam)
        {
            DoSocialActionResponse sRes = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "DoSocialAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupID, initObj.Platform);
                    DoSocialActionResponse response = service.DoSocialAction(mediaID, initObj.SiteGuid, initObj.UDID, socialAction, socialPlatform, actionParam);
                    sRes = response;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return sRes;
        }

        [WebMethod(EnableSession = true, Description = "Get user social actions")]
        [PrivateMethod]
        public SocialActivityDoc[] GetUserSocialActions(InitializationObject initObj, eUserAction socialAction, SocialPlatform socialPlatform, bool isOnlyFriends, int startIndex, int numOfItems)
        {
            SocialActivityDoc[] res = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserSocialActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupID, initObj.Platform);
                    res = service.GetUserSocialActions(initObj.SiteGuid, socialAction, socialPlatform, isOnlyFriends, startIndex, numOfItems);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Do Post Reg Action")]
        public string PostRegAction(InitializationObject initObj, string actionName)
        {
            string retVal = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "DoSocialAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string actionSubCode = ConfigurationManager.AppSettings[string.Concat(groupID, "_PostReg", actionName)];
                    TVPApiModule.Services.ApiConditionalAccessService service = new TVPApiModule.Services.ApiConditionalAccessService(groupID, initObj.Platform);

                    retVal = service.DummyChargeUserForSubscription(0, string.Empty, actionSubCode, string.Empty, SiteHelper.GetClientIP(), initObj.SiteGuid, string.Empty, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return retVal;
        }

        [WebMethod(EnableSession = true, Description = "Set User Rule State")]
        public bool SetRuleState(InitializationObject initObj, int ruleID, int isActive, string siteGuid)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetRuleState", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, siteGuid, 0, null, groupID, initObj.Platform))
                {
                    return false;
                }
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetRuleState(initObj.SiteGuid, initObj.DomainID, ruleID, isActive);
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

        [WebMethod(EnableSession = true, Description = "Get Domain Group Rules")]
        [PrivateMethod]
        public TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetDomainGroupRules(InitializationObject initObj)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetDomainGroupRules(initObj.DomainID);
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

        [WebMethod(EnableSession = true, Description = "Set Domain Group Rule")]
        [PrivateMethod]
        public bool SetDomainGroupRule(InitializationObject initObj, int ruleID, string PIN, int isActive)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDomainGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupID, initObj.Platform))
                {
                    return false;
                }
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetDomainGroupRule(initObj.DomainID, ruleID, PIN, isActive);
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
        #endregion

        #region Translation

        [WebMethod(EnableSession = true, Description = "Get translations for all active languages")]
        public Pair[] GetTranslations(InitializationObject initObj)
        {
            Pair[] retTranslations = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetTranslations", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retTranslations = TranslationHelper.GetTranslations(groupID, initObj.Platform);
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

            return retTranslations;
        }

        [WebMethod(EnableSession = true, Description = "Get IP To Country")]
        public string GetIPToCountry(InitializationObject initObj, string IP)
        {
            string res = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetIPToCountry", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    res = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IpToCountry(IP);
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

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get EPG program rules")]
        public TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetEPGProgramRules(InitializationObject initObj, int MediaId, int programId, string IP)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetEPGProgramRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (int.TryParse(initObj.SiteGuid, out siteGuid))
                        response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetEPGProgramRules(MediaId, programId, siteGuid, IP, initObj.UDID);
                    else
                        throw new Exception("Site guid is not a valid number");
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

        [WebMethod(EnableSession = true, Description = "Get user started watching medias")]
        [PrivateMethod]
        public string[] GetUserStartedWatchingMedias(InitializationObject initObj, int numOfItems)
        {
            string[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserStartedWatchingMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetUserStartedWatchingMedias(initObj.SiteGuid, numOfItems);
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


        [WebMethod(EnableSession = true, Description = "Record All")]
        [PrivateMethod]
        public TVPApiModule.yes.tvinci.ITProxy.RecordAllResult RecordAll(InitializationObject initObj, string accountNumber, string channelCode, string recordDate, string recordTime, string versionId, string serialNumber)
        {
            TVPApiModule.yes.tvinci.ITProxy.RecordAllResult response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RecordAll", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    IImplementation impl = WSUtils.GetImplementation(groupID, initObj);
                    response = impl.RecordAll(accountNumber, channelCode, recordDate, recordTime, versionId, serialNumber);
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

        [WebMethod(EnableSession = true, Description = "Get Account STBs")]
        [PrivateMethod]
        public TVPApiModule.yes.tvinci.ITProxy.STBData[] GetAccountSTBs(InitializationObject initObj, string accountNumber, string serviceAddressId)
        {
            TVPApiModule.yes.tvinci.ITProxy.STBData[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetAccountSTBs", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    IImplementation impl = WSUtils.GetImplementation(groupID, initObj);
                    response = impl.GetMemirDetails(accountNumber, serviceAddressId);
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

        #endregion

        //[WebMethod(EnableSession = true, Description = "Generates the temporary device token")]
        //public string GenerateDeviceToken(InitializationObject initObj, string appId)
        //{
        //    string response = null;

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GenerateDeviceToken", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            response = AuthorizationManager.Instance.GenerateDeviceToken(initObj.UDID, appId);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items["Error"] = ex;
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items["Error"] = "Unknown group";
        //    }

        //    return response;
        //}

        //[WebMethod(EnableSession = true, Description = "Exchanges the temporary device token with an access token")]
        //public object ExchangeDeviceToken(InitializationObject initObj, string appId, string appSecret, string deviceToken)
        //{
        //    object response = null;

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "ExchangeDeviceToken", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            response = AuthorizationManager.Instance.ExchangeDeviceToken(initObj.UDID, appId, appSecret, deviceToken);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items["Error"] = ex;
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items["Error"] = "Unknown group";
        //    }

        //    return response;
        //}

        [WebMethod(EnableSession = true, Description = "Refreshes the access token using refresh token")]
        public object RefreshAccessToken(InitializationObject initObj, string refreshToken)
        {
            object response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RefreshAccessToken", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = AuthorizationManager.Instance.RefreshAccessToken(refreshToken, initObj.Token, groupID, initObj.Platform, initObj.UDID, initObj.SiteGuid);
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


        [WebMethod(EnableSession = true, Description = "Changes the siteGuid assigned to the accessToken")]
        [PrivateMethod]
        public object ChangeUser(InitializationObject initObj, string siteGuid)
        {
            object response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeUser", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                if (AuthorizationManager.IsTokenizationEnabled() && (!AuthorizationManager.IsSwitchingUsersAllowed(groupID) ||
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, siteGuid, 0, null, groupID, initObj.Platform)))
                {
                    AuthorizationManager.Instance.returnError(403);
                    return null;
                }
                try
                {
                    response = AuthorizationManager.Instance.UpdateUserInToken(initObj.Token, siteGuid, groupID, initObj.UDID);
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

        // Delete later
        //[WebMethod(EnableSession = true, Description = "Generate Application Credentials")]
        //public AppCredentials GenerateAppCredentials(InitializationObject initObj)
        //{
        //    AppCredentials response = null;

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GenerateAppCredentials", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            response = AuthorizationManager.Instance.GenerateAppCredentials(groupID);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items["Error"] = ex;
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items["Error"] = "Unknown group";
        //    }

        //    return response;
        //}

        [WebMethod(EnableSession = true, Description = "Get regions: if regionIds supplied by the ids, if not returns all group regions")]
        public RegionsResponse GetRegions(InitializationObject initObj, string[] region_ids)
        {
            RegionsResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetRegions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetRegions(region_ids);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new RegionsResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new RegionsResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Admin sign in using TVM users")]
        public AdminUserResponse AdminSignIn(InitializationObject initObj, string username, string password)
        {
            AdminUserResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AdminSignIn", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).AdminSignIn(username, password);
                    // if sign in successful - generate access token
                    if (response.Status.Code == (int)eStatus.OK && response.AdminUser != null)
                    {
                        var accessToken = AuthorizationManager.Instance.GenerateAccessToken(response.AdminUser.Id.ToString(), groupID, true, false, initObj.UDID, initObj.Platform);
                        HttpContext.Current.Response.Headers.Add("access_token", string.Format("{0}|{1}", accessToken.AccessToken, accessToken.AccessTokenExpiration));
                        HttpContext.Current.Response.Headers.Add("refresh_token", string.Format("{0}|{1}", accessToken.RefreshToken, accessToken.RefreshTokenExpiration));
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new AdminUserResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new AdminUserResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        #region Parental Rules

        /// <summary>
        /// Gets all of the parental rules for the account.
        /// Includes specification of what of which is the default rule/s for the account
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <returns>List of parental rules</returns>
        [WebMethod(EnableSession = true, Description = "Get Group Parental Rules")]
        public ParentalRulesResponse GetParentalRules(InitializationObject initObj)
        {
            ParentalRulesResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetParentalRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetParentalRules();
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

        /// <summary>
        /// Gets the parental rules that applies for the domain
        /// Includes distinction if rule was defined at account or HH level
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <returns>List of parental rules</returns>        
        [WebMethod(EnableSession = true, Description = "Get Domain Parental Rules")]
        public ParentalRulesResponse GetDomainParentalRules(InitializationObject initObj)
        {
            ParentalRulesResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainParentalRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetDomainParentalRule(initObj.DomainID);
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

        /// <summary>
        /// Gets the parental rules that applies for the domain
        /// Includes distinction if rule was defined at account, HH or user level
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">The user which the rules are applied for</param>
        /// <returns>List of parental rules</returns>        
        [WebMethod(EnableSession = true, Description = "Get User Parental Rules")]
        public ParentalRulesResponse GetUserParentalRules(InitializationObject initObj, string siteGuid)
        {
            ParentalRulesResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserParentalRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string userGuid = !string.IsNullOrEmpty(siteGuid) ? siteGuid : initObj.SiteGuid;

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetUserParentalRules(userGuid, initObj.DomainID);
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

        /// <summary>
        /// Enable or disable a parental rule for the user
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">The user which the rule is applied for</param>
        /// <param name="ruleId">Rule identification</param>
        /// <param name="isActive">Whether the rule is enabled or disabled</param>
        /// <returns>Success / Fail and reason</returns>
        public TVPApiModule.Objects.Responses.Status SetUserParentalRules(InitializationObject initObj, string siteGuid, long ruleId, int isActive)
        {
            TVPApiModule.Objects.Responses.Status response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserParentalRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetParentalRules(siteGuid, initObj.DomainID, ruleId, isActive);
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

        /// <summary>
        /// Enable or disable a parental rule for the domain
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="ruleId">Rule identification</param>
        /// <param name="isActive">Whether the rule is enabled or disabled</param>
        /// <returns>Success / Fail and reason</returns>
        public TVPApiModule.Objects.Responses.Status SetDomainParentalRules(InitializationObject initObj, long ruleId, int isActive)
        {
            TVPApiModule.Objects.Responses.Status response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDomainParentalRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetParentalRules(string.Empty, initObj.DomainID, ruleId, isActive);
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

        /// <summary>
        /// Retrieve the parental PIN that applies for the domain or user. Includes specification of where the PIN was defined at – account, household or user.
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">Optional - if we want to retrieve parental PIN of user and not of domain</param>
        /// <returns>The parental PIN of the HH/User</returns>
        [WebMethod(EnableSession = true, Description = "Gets Parental PIN")]
        public PinResponse GetParentalPIN(InitializationObject initObj, string siteGuid)
        {
            PinResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetParentalPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string siteUserGuid = !string.IsNullOrEmpty(siteGuid) ? siteGuid : initObj.SiteGuid;

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetParentalPIN(initObj.DomainID, siteUserGuid);
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

        /// <summary>
        /// Sets the parental PIN for the domain or user
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">Optional - if we want to set parental PIN of user and not of domain</param>
        /// <param name="pin">The new PIN</param>
        /// <returns>Success / Fail and reason</returns>
        [WebMethod(EnableSession = true, Description = "Sets Parental PIN")]
        public TVPApiModule.Objects.Responses.Status SetParentalPIN(InitializationObject initObj, string siteGuid, string pin)
        {
            TVPApiModule.Objects.Responses.Status response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetParentalPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetParentalPIN(siteGuid, initObj.DomainID, pin);
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

        /// <summary>
        /// Retrieve the purchase settings that applies for the domain or user. Includes specification of where the setting was defined at – account, household or user.
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">Optional - if we want to retrieve purchase settings of user and not of domain</param>
        /// <returns>The purchase settings of the HH/User</returns>
        [WebMethod(EnableSession = true, Description = "Gets Purchase Settings")]
        public PurchaseSettingsResponse GetPurchaseSettings(InitializationObject initObj, string siteGuid)
        {
            PurchaseSettingsResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPurchaseSettings", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string siteUserGuid = !string.IsNullOrEmpty(siteGuid) ? siteGuid : initObj.SiteGuid;

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetPurchaseSettings(initObj.DomainID, siteUserGuid);
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

        /// <summary>
        ///  Sets the purchase settings for the domain or user.
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">Optional - if we want to set purhase settings of user and not of domain</param>
        /// <param name="setting">The new purchase settings</param>
        /// <returns>Fail / Success and reason</returns>
        [WebMethod(EnableSession = true, Description = "Sets Purchase Settings")]
        public TVPApiModule.Objects.Responses.Status SetPurchaseSettings(InitializationObject initObj, string siteGuid, int setting)
        {
            TVPApiModule.Objects.Responses.Status response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetPurchaseSettings", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetPurchaseSettings(initObj.DomainID, siteGuid, setting);
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

        /// <summary>
        /// Retrieve the Purchase PIN that applies for the domain or user. Includes specification of where the PIN was defined at – account, household or user.
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">Optional - if we want to retrieve Purchase PIN of user and not of domain</param>
        /// <returns>The Purchase PIN of the HH/User</returns>
        [WebMethod(EnableSession = true, Description = "Gets Purchase PIN")]
        public PurchaseSettingsResponse GetPurchasePIN(InitializationObject initObj, string siteGuid)
        {
            PurchaseSettingsResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPurchasePIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string siteUserGuid = !string.IsNullOrEmpty(siteGuid) ? siteGuid : initObj.SiteGuid;

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetPurchasePIN(initObj.DomainID, siteUserGuid);
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

        /// <summary>
        /// Sets the Purchase PIN for the domain or user
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">Optional - if we want to set Purchase PIN of user and not of domain</param>
        /// <param name="pin">The new PIN</param>
        /// <returns>Success / Fail and reason</returns>
        [WebMethod(EnableSession = true, Description = "Sets Purchase PIN")]
        public TVPApiModule.Objects.Responses.Status SetPurchasePIN(InitializationObject initObj, string siteGuid, string pin)
        {
            TVPApiModule.Objects.Responses.Status response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetPurchasePIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetPurchasePIN(initObj.DomainID, siteGuid, pin);
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

        /// <summary>
        /// Validate that a given parental PIN for a user is valid.
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">The user we validate its PIN</param>
        /// <param name="pin">The given PIN</param>
        /// <returns>Success if PINs match, fail if otherwise</returns>
        [WebMethod(EnableSession = true, Description = "Validates Parental PIN")]
        public TVPApiModule.Objects.Responses.Status ValidateParentalPIN(InitializationObject initObj, string siteGuid, string pin)
        {
            TVPApiModule.Objects.Responses.Status response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ValidateParentalPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string siteUserGuid = !string.IsNullOrEmpty(siteGuid) ? siteGuid : initObj.SiteGuid;

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).ValidateParentalPIN(initObj.DomainID, siteUserGuid, pin);
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

        /// <summary>
        /// Validate that a given purhcase PIN for a user is valid.
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">The user we validate its PIN</param>
        /// <param name="pin">The given PIN</param>
        /// <returns>Success if PINs match, fail if otherwise</returns>
        [WebMethod(EnableSession = true, Description = "Validates Purchase PIN")]
        public TVPApiModule.Objects.Responses.Status ValidatePurchasePIN(InitializationObject initObj, string siteGuid, string pin)
        {
            TVPApiModule.Objects.Responses.Status response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ValidatePurchasePIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string siteUserGuid = !string.IsNullOrEmpty(siteGuid) ? siteGuid : initObj.SiteGuid;

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).ValidatePurchasePIN(initObj.DomainID, siteUserGuid, pin);
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

        /// <summary>
        /// Retrieve all the parental rules that applies for a specific media and a specific user according to the user parental settings.
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">The user we retrieve its rules</param>
        /// <param name="mediaId">The media which the rules are applied for</param>
        /// <returns>List of rules that are applied for this media and user</returns>
        [WebMethod(EnableSession = true, Description = "Gets Parental rules that apply to media")]
        public ParentalRulesResponse GetParentalMediaRules(InitializationObject initObj, string siteGuid, long mediaId)
        {
            ParentalRulesResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetParentalMediaRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string userGuid = !string.IsNullOrEmpty(siteGuid) ? siteGuid : initObj.SiteGuid;

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetParentalMediaRules(userGuid, mediaId, initObj.DomainID);
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

        /// <summary>
        /// Retrieve all the parental rules that applies for a specific program and a specific user according to the user parental settings.
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">The user we retrieve its rules</param>
        /// <param name="mediaId">The program which the rules are applied for</param>
        /// <returns>List of rules that are applied for this program and user</returns>
        [WebMethod(EnableSession = true, Description = "Gets Parental rules that apply to program")]
        public ParentalRulesResponse GetParentalEPGRules(InitializationObject initObj, string siteGuid, long epgId)
        {
            ParentalRulesResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetParentalEPGRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string userGuid = !string.IsNullOrEmpty(siteGuid) ? siteGuid : initObj.SiteGuid;

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetParentalEPGRules(userGuid, epgId, initObj.DomainID);
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

        /// <summary>
        /// Disable the default parental rule for the domain or user
        /// </summary>
        /// <param name="initObj">Initialization object</param>
        /// <param name="siteGuid">Optional - if we want to disable the default rule of a user and not of domain</param>
        /// <returns>Success / Fail and reason</returns>
        public TVPApiModule.Objects.Responses.Status DisableDefaultParentalRule(InitializationObject initObj, string siteGuid)
        {
            TVPApiModule.Objects.Responses.Status response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "DisableDefaultParentalRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).DisableDefaultParentalRule(siteGuid, initObj.DomainID);
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

        /// <summary>
        /// Retrieve all the rules (parental, geo, device or user-type) that applies for this user and media 
        /// </summary>
        [WebMethod(EnableSession = true, Description = "Gets rules that apply to media")]
        public GenericRulesResponse GetMediaRules(InitializationObject initObj, string siteGuid, long mediaId)
        {
            GenericRulesResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string userGuid = !string.IsNullOrEmpty(siteGuid) ? siteGuid : initObj.SiteGuid;

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetMediaRules(userGuid, mediaId, initObj.DomainID, SiteHelper.GetClientIP(), initObj.UDID);
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

        /// <summary>
        /// Retrieve all the rules (parental) that applies for this EPG program 
        /// </summary>
        [WebMethod(EnableSession = true, Description = "Gets rules that apply to media")]
        public GenericRulesResponse GetEpgRules(InitializationObject initObj, string siteGuid, long epgId, long channelMediaId)
        {
            GenericRulesResponse response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetEpgRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string userGuid = !string.IsNullOrEmpty(siteGuid) ? siteGuid : initObj.SiteGuid;

                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetEpgRules(userGuid, epgId, channelMediaId, initObj.DomainID, SiteHelper.GetClientIP(), initObj.UDID);
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

        #endregion

    }
}
