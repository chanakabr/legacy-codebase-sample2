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
using log4net;
//using TVPPro.SiteManager.TvinciPlatform.Users;
//using TVPPro.SiteManager.TvinciPlatform.Social;
using System.Configuration;
using TVPApiModule.Objects;
using TVPApiModule.Helper;
using System.Web.UI;
using System.Web;
using TVPPro.SiteManager.TvinciPlatform.Social;


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
    public class SiteService : System.Web.Services.WebService//, ISiteService
    {

        private readonly ILog logger = LogManager.GetLogger(typeof(SiteService));

        #region SiteMap

        //// Deprecated
        ////Get complete user site map - retrieve on first time from DB for each new groupID. Next calls will get ready site map
        //[WebMethod(EnableSession = true, Description = "Get complete user site map - retrieve on first time from DB for each new groupID. Next calls will get ready site map")]
        //public TVPApi.SiteMap GetSiteMap(InitializationObject initObj)
        //{
        //    TVPApi.SiteMap retSiteMap = null;

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSiteMap", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            retSiteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items.Add("Error", ex);
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items.Add("Error", "Unknown group");
        //    }

        //    return retSiteMap;
        //}

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
                    retPageContext = PageDataHelper.GetPageContextByID(initObj.Platform, groupID, initObj.Locale, ID, withMenu, withFooter);
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

            return retPageContext;
        }

        ////Get specific page from site map - Deprecated!
        //[WebMethod(EnableSession = true, Description = "Get specific page from site map")]
        //public PageContext GetPageByToken(InitializationObject initObj, Pages token, bool withMenu, bool withFooter)
        //{
        //    PageContext retPageContext = null;

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPage", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            retPageContext = PageDataHelper.GetPageContextByToken(initObj, groupID, token, withMenu, withFooter);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items.Add("Error", ex);
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items.Add("Error", "Unknown group");
        //    }

        //    return retPageContext;
        //}

        [WebMethod(EnableSession = true, Description = "Get site menu")]
        public Menu GetMenu(InitializationObject initObj, long ID)
        {
            Menu retMenu = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMenu", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retMenu = MenuHelper.GetMenuByID(initObj.Platform, initObj.Locale, ID, groupID);
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
                    retMenu = MenuHelper.GetFooterByID(initObj.Platform, initObj.Locale, ID, groupID);
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

            return retMenu;
        }

        //// Deprecated!
        //[WebMethod(EnableSession = true, Description = "Get site side galleries")]
        //public Profile GetSideProfile(InitializationObject initObj, long ID)
        //{
        //    Profile retProfile = null;

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSideProfile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            retProfile = ProfileHelper.GetSideProfile(initObj, ID, groupID);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items.Add("Error", ex);
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items.Add("Error", "Unknown group");
        //    }

        //    return retProfile;
        //}

        ////Get full bottom profile from site map - Deprecated!
        //[WebMethod(EnableSession = true, Description = "Get full bottom profile from site map")]
        //public Profile GetBottomProfile(InitializationObject initObj, long ID)
        //{
        //    Profile retProfile = null;

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetBottomProfile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            retProfile = ProfileHelper.GetBottomProfile(initObj, ID, groupID);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items.Add("Error", ex);
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items.Add("Error", "Unknown group");
        //    }

        //    return retProfile;
        //}
        #endregion

        #region Galleries

        ////Get all page galleries from site map - Deprecated!
        //[WebMethod(EnableSession = true, Description = "Get all page galleries from site map")]
        //public List<PageGallery> GetPageGalleries(InitializationObject initObj, long PageID, int pageSize, int start_index)
        //{
        //    List<PageGallery> lstPageGallery = null;

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPageGalleries", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            lstPageGallery = PageGalleryHelper.GetPageGallerisByPageID(initObj, PageID, groupID, pageSize, start_index);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items.Add("Error", ex);
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items.Add("Error", "Unknown group");
        //    }

        //    return lstPageGallery;
        //}

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
                    retPageGallery = PageGalleryHelper.GetGalleryByID(initObj.Platform, initObj.Locale, galleryID, PageID, groupID);
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
                    lstGalleryItem = PageGalleryHelper.GetGalleryContent(initObj.Platform, initObj.Locale, ID, PageID, picSize, groupID);
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

                    lstMedia = PageGalleryHelper.GetGalleryItemContent(initObj.Platform, initObj.UDID, initObj.Locale, PageID, GalleryID, ItemID, picSize, groupID, pageSize, pageIndex, orderBy);
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

            return lstMedia;
        }

        #endregion

        #region User
        [WebMethod(EnableSession = true, Description = "Get Group Rules")]
        public IEnumerable<TVPApiModule.Objects.Responses.GroupRule> GetGroupRules(InitializationObject initObj)
        {
            IEnumerable<TVPApiModule.Objects.Responses.GroupRule> response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetGroupRules();
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

        [WebMethod(EnableSession = true, Description = "Get User Group Rules")]
        public IEnumerable<TVPApiModule.Objects.Responses.GroupRule> GetUserGroupRules(InitializationObject initObj, string siteGuid)
        {
            IEnumerable<TVPApiModule.Objects.Responses.GroupRule> response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetUserGroupRules(siteGuid);
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

        
        [WebMethod(EnableSession = true, Description = "Set User Group Rule")]
        public bool SetUserGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN, int isActive)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetUserGroupRule(siteGuid, ruleID, PIN, isActive);
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

        [WebMethod(EnableSession = true, Description = "Check Parental PIN")]
        public bool CheckParentalPIN(InitializationObject initObj, string siteGuid, int ruleID, string PIN)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CheckParentalPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).CheckParentalPIN(siteGuid, ruleID, PIN);
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

        [WebMethod(EnableSession = true, Description = "Get Secured SiteGuid")]
        public string GetSecuredSiteGuid(InitializationObject initObj, string siteGuid)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSecuredSiteGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    string privateKey = ConfigurationManager.AppSettings["SecureSiteGuidKey"];
                    string IV = ConfigurationManager.AppSettings["SecureSiteGuidIV"];
                    sRet = SecurityHelper.EncryptSiteGuid(privateKey, IV, siteGuid);
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

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Get Group Operators")]
        public IEnumerable<TVPApiModule.Objects.Responses.GroupOperator> GetGroupOperators(InitializationObject initObj, string scope)
        {
            IEnumerable<TVPApiModule.Objects.Responses.GroupOperator> response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupOperators", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetGroupOperators(scope);
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

        [WebMethod(EnableSession = true, Description = "Get Operators")]
        public IEnumerable<TVPApiModule.Objects.Responses.GroupOperator> GetOperators(InitializationObject initObj, int[] operators)
        {
            IEnumerable<TVPApiModule.Objects.Responses.GroupOperator> response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupOperators", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetOperators(operators);
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

        [WebMethod(EnableSession = true, Description = "SSO Signin")]
        public TVPApiModule.Objects.Responses.UserResponseObject SSOSignIn(InitializationObject initObj, string userName, string password, int providerID)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SSOSignIn", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SSOSignIn(userName, password, providerID, string.Empty, SiteHelper.GetClientIP(), initObj.UDID, false);
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

        [WebMethod(EnableSession = true, Description = "Check SSO Login")]
        public TVPApiModule.Objects.Responses.UserResponseObject SSOCheckLogin(InitializationObject initObj, string userName, int providerID)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SSOCheckLogin", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SSOCheckLogin(userName, providerID);
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

        [WebMethod(EnableSession = true, Description = "Get user data by co-guid")]
        public TVPApiModule.Objects.Responses.UserResponseObject GetUserDataByCoGuid(InitializationObject initObj, string coGuid, int operatorID)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserDataByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserDataByCoGuid(coGuid, operatorID);
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

        [WebMethod(EnableSession = true, Description = "Get from Secured SiteGuid")]
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

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Validate user")]
        public string GetSiteGuid(InitializationObject initObj, string userName, string password)
        {
            TVPApiModule.Objects.Responses.User sRet = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ValidateUser", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    sRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ValidateUser(userName, password, isSingleLogin).user;
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

            return sRet != null ? sRet.site_guid : string.Empty;
        }

        [WebMethod(EnableSession = true, Description = "Sign-In a user")]        
        public TVPApiModule.Services.ApiUsersService.LogInResponseData SignIn(InitializationObject initObj, string userName, string password)
        {
            TVPApiModule.Services.ApiUsersService.LogInResponseData responseData = new TVPApiModule.Services.ApiUsersService.LogInResponseData();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //XXX: Do the UDID empty stuff
                    IImplementation impl = WSUtils.GetImplementation(groupID, initObj);
                    responseData = impl.SignIn(userName, password);
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

            return responseData;
        }

        [WebMethod(EnableSession = true, Description = "Has user connected to FB")]
        public bool IsFacebookUser(InitializationObject initObj, string siteGuid)
        {
            bool bRes = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsFacebookUser", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    TVPApiModule.Objects.Responses.UserResponseObject userObj = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserData(siteGuid);
                    bRes = !string.IsNullOrEmpty(userObj.user.basic_data.facebook_id);
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

            return bRes;
        }

        [WebMethod(EnableSession = true, Description = "Sign-Up a new user")]
        public TVPApiModule.Objects.Responses.UserResponseObject SignUp(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData,
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = new TVPApiModule.Objects.Responses.UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignUp", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SignUp(userBasicData, userDynamicData, sPassword, sAffiliateCode);
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

        [WebMethod(EnableSession = true, Description = "Sign-Out a user")]
        public void SignOut(InitializationObject initObj, string siteGuid)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //XXX: Do the UDID empty stuff
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SignOut(siteGuid, initObj.UDID, string.Empty, isSingleLogin);
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

        [WebMethod(EnableSession = true, Description = "Check if user is signed in")]
        public bool IsUserSignedIn(InitializationObject initObj, string siteGuid)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsUserSignedIn", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    //XXX: Do the UDID empty stuff
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    bRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IsUserLoggedIn(siteGuid, initObj.UDID, string.Empty, SiteHelper.GetClientIP(), isSingleLogin);
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

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Edit user details info")]
        public TVPApiModule.Objects.Responses.UserResponseObject SetUserData(InitializationObject initObj, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData)
        {
            TVPApiModule.Objects.Responses.UserResponseObject response = new TVPApiModule.Objects.Responses.UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserData(siteGuid, userBasicData, userDynamicData);
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

        //// Deprecated!
        //[WebMethod(EnableSession = true, Description = "Get user details info")]
        //public UserResponseObject GetUserData(InitializationObject initObj, string sSiteGuid)
        //{
        //    UserResponseObject response = new UserResponseObject();

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            string siteGuid = (string.IsNullOrEmpty(sSiteGuid)) ? initObj.SiteGuid : sSiteGuid;
        //            response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserData(siteGuid);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items.Add("Error", ex);
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items.Add("Error", "Unknown group");
        //    }

        //    return response;
        //}

        [WebMethod(EnableSession = true, Description = "Get users details info")]
        public IEnumerable<TVPApiModule.Objects.Responses.UserResponseObject> GetUsersData(InitializationObject initObj, string siteGuid)
        {
            IEnumerable<TVPApiModule.Objects.Responses.UserResponseObject> response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUsersData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUsersData(siteGuid);
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

        //// Deprecated!
        //[WebMethod(EnableSession = true, Description = "Get user CA status")]
        //public TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus GetUserCAStatus(InitializationObject initObj)
        //{
        //    TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus response = TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.Annonymus;

        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserCAStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            response = new TVPApiModule.Services.ApiConditionalAccessService(groupID, initObj.Platform).GetUserCAStatus(initObj.SiteGuid);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items.Add("Error", ex);
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items.Add("Error", "Unknown group");
        //    }

        //    return response;
        //}

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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Set specific dynamic user key data")]
        public bool SetUserDynamicData(InitializationObject initObj, string siteGuid, string sKey, string sValue)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserDynamicData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    bRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserDynamicData(siteGuid, sKey, sValue);
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

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Clean User History")]
        public bool CleanUserHistory(InitializationObject initObj, string siteGuid, int[] mediaIDs)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CleanUserHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    bRet = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).CleanUserHistory(siteGuid, mediaIDs);
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

            return bRet;
        }
        #endregion

        #region XXXX

        [WebMethod(EnableSession = true, Description = "Do Social Action")]
        public string DoSocialAction(InitializationObject initObj, string siteGuid, int mediaID, TVPPro.SiteManager.TvinciPlatform.Social.eUserAction socialAction, SocialPlatform socialPlatform, string actionParam)
        {
            string sRes = SocialActionResponseStatus.UNKNOWN.ToString();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "DoSocialAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupID, initObj.Platform);
                    TVPApiModule.Objects.Responses.SocialActionResponseStatus response = (TVPApiModule.Objects.Responses.SocialActionResponseStatus)service.DoSocialAction(mediaID, siteGuid, initObj.UDID, socialAction, socialPlatform, actionParam);
                    sRes = response.ToString();
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return sRes;
        }

        [WebMethod(EnableSession = true, Description = "Get user social actions")]
        public IEnumerable<TVPApiModule.Objects.Responses.UserSocialActionObject> GetUserSocialActions(InitializationObject initObj, string siteGuid, eUserAction socialAction, SocialPlatform socialPlatform, bool isOnlyFriends, int startIndex, int numOfItems)
        {
            IEnumerable<TVPApiModule.Objects.Responses.UserSocialActionObject> res = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserSocialActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupID, initObj.Platform);
                    res = service.GetUserSocialActions(siteGuid, socialAction, socialPlatform, isOnlyFriends, startIndex, numOfItems);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return retVal;
        }

        [WebMethod(EnableSession = true, Description = "Set User Rule State")]
        public bool SetRuleState(InitializationObject initObj, int ruleID, int isActive)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetRuleState", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetRuleState(initObj.SiteGuid, initObj.DomainID, ruleID, isActive);
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

        [WebMethod(EnableSession = true, Description = "Get Domain Group Rules")]
        public IEnumerable<TVPApiModule.Objects.Responses.GroupRule> GetDomainGroupRules(InitializationObject initObj)
        {
            IEnumerable<TVPApiModule.Objects.Responses.GroupRule> response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetDomainGroupRules(initObj.DomainID);
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

        [WebMethod(EnableSession = true, Description = "Set Domain Group Rule")]
        public bool SetDomainGroupRule(InitializationObject initObj, int ruleID, string PIN, int isActive)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDomainGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetDomainGroupRule(initObj.DomainID, ruleID, PIN, isActive);
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

        #region Translation

        [WebMethod(EnableSession = true, Description = "Get translations for all active languages")]
        public Pair[] GetTranslations(InitializationObject initObj)
        {
            Pair[] retTranslations = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetBottomProfile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    retTranslations = TranslationHelper.GetTranslations(groupID, initObj.Platform);
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get EPG program rules")]
        public IEnumerable<TVPApiModule.Objects.Responses.GroupRule> GetEPGProgramRules(InitializationObject initObj, int MediaId, int programId, string IP)
        {
            IEnumerable<TVPApiModule.Objects.Responses.GroupRule> response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetEPGProgramRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetEPGProgramRules(MediaId, programId, initObj.SiteGuid, IP, initObj.UDID);
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

        [WebMethod(EnableSession = true, Description = "Get user started watching medias")]
        public IEnumerable<string> GetUserStartedWatchingMedias(InitializationObject initObj, string siteGuid, int numOfItems)
        {
            IEnumerable<string> response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserStartedWatchingMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetUserStartedWatchingMedias(siteGuid, numOfItems);
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
