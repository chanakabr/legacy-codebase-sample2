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
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.TvinciPlatform.Social;
using System.Configuration;
using TVPApiModule.Objects;
using TVPApiModule.Helper;
using System.Web.UI;


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

        private readonly ILog logger = LogManager.GetLogger(typeof(SiteService));

        #region SiteMap

        //Get complete user site map - retrieve on first time from DB for each new groupID. Next calls will get ready site map
        [WebMethod(EnableSession=true, Description="Get complete user site map - retrieve on first time from DB for each new groupID. Next calls will get ready site map")]
        public SiteMap GetSiteMap(InitializationObject initObj)
        {
            SiteMap retSiteMap = null;
            
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSiteMap", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetSiteMap-> [{0}, {1}]", groupID, initObj.Platform);

            if (groupID > 0)
            {
                try
                {
                    retSiteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
                }
                catch (Exception ex)
                {
                    logger.Error("GetSiteMap->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetSiteMap-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
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
            
            logger.InfoFormat("GetPage-> [{0}, {1}], Params:[ID: {2}, withMenu: {3}, withFooter: {4}]", groupID, initObj.Platform, ID, withMenu, withFooter);

            if (groupID > 0)
            {
                try
                {
                    retPageContext = PageDataHelper.GetPageContextByID(initObj, groupID, ID, withMenu, withFooter);
                }
                catch (Exception ex)
                {
                    logger.Error("GetPage->", ex);
                }
            }
            else
            {
                //TODO: Logger.Logger.Log("GetPage", "Unknown group " + "Username : " + ws_User + " Password :" + ws_Pass, "TVPApiExcpeions");
                logger.ErrorFormat("GetPage-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retPageContext;
        }

        //Get specific page from site map
        [WebMethod(EnableSession = true, Description = "Get specific page from site map")]
        public PageContext GetPageByToken(InitializationObject initObj, Pages token, bool withMenu, bool withFooter)
        {
            PageContext retPageContext = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPage", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetPageByToken-> [{0}, {1}], Params:[token: {2}, withMenu: {3}, withFooter: {4}]", groupID, initObj.Platform, token, withMenu, withFooter);

            if (groupID > 0)
            {
                try
                {
                    retPageContext = PageDataHelper.GetPageContextByToken(initObj, groupID, token, withMenu, withFooter);
                }
                catch (Exception ex)
                {
                    logger.Error("GetPageByToken->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetPageByToken-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retPageContext;
        }

        [WebMethod(EnableSession = true, Description = "Get site menu")]
        public Menu GetMenu(InitializationObject initObj, long ID)
        {
            Menu retMenu = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMenu", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetMenu-> [{0}, {1}], Params:[ID: {2}]", groupID, initObj.Platform, ID);

            Logger.Logger.Log("GetMenu", groupID.ToString() + initObj.Platform.ToString() + " ID :" + ID.ToString(), "TVPApi");
            if (groupID > 0)
            {
                try
                {
                    retMenu = MenuHelper.GetMenuByID(initObj, ID, groupID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetMenu->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetMenu-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retMenu;
        }

        [WebMethod(EnableSession = true, Description = "Get site footer menu")]
        public Menu GetFooter(InitializationObject initObj, long ID)
        {
            Menu retMenu = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetFooter", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetFooter-> [{0}, {1}], Params:[ID: {2}]", groupID, initObj.Platform, ID);

            if (groupID > 0)
            {
                try
                {
                    retMenu = MenuHelper.GetFooterByID(initObj, ID, groupID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetFooter->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetFooter-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retMenu;
        }

        [WebMethod(EnableSession = true, Description = "Get site side galleries")]
        public Profile GetSideProfile(InitializationObject initObj, long ID)
        {
            Profile retProfile = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSideProfile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetSideProfile-> [{0}, {1}], Params:[ID: {2}]", groupID, initObj.Platform, ID);
            
            if (groupID > 0)
            {
                try
                {
                    retProfile = ProfileHelper.GetSideProfile(initObj, ID, groupID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetSideProfile->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetSideProfile-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retProfile;
        }

        //Get full bottom profile from site map
        [WebMethod(EnableSession = true, Description = "Get full bottom profile from site map")]
        public Profile GetBottomProfile(InitializationObject initObj, long ID)
        {
            Profile retProfile = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetBottomProfile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            logger.InfoFormat("GetBottomProfile-> [{0}, {1}], Params:[ID: {2}]", groupID, initObj.Platform, ID);

            if (groupID > 0)
            {
                try
                {
                    retProfile = ProfileHelper.GetBottomProfile(initObj, ID, groupID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetBottomProfile->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetBottomProfile-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
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

            logger.InfoFormat("GetPageByToken-> [{0}, {1}], Params:[PageID: {2}, pageSize: {3}, start_index: {4}]", groupID, initObj.Platform, PageID, pageSize, start_index);

            if (groupID > 0)
            {
                try
                {
                    lstPageGallery = PageGalleryHelper.GetPageGallerisByPageID(initObj, PageID, groupID, pageSize, start_index);
                }
                catch (Exception ex)
                {
                    logger.Error("GetPageGalleries->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetPageGalleries-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstPageGallery;
        }

        //Get all page galleries from site map
        [WebMethod(EnableSession = true, Description = "Get all page galleries from site map")]
        public PageGallery GetGallery(InitializationObject initObj, long galleryID, long PageID)
        {
            PageGallery retPageGallery = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPageGalleries", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            logger.InfoFormat("GetGallery-> [{0}, {1}], Params:[galleryID: {2}, PageID: {3}]", groupID, initObj.Platform, PageID, galleryID, PageID);

            if (groupID > 0)
            {
                try
                {
                    ODBCWrapper.Connection.GetDefaultConnectionStringMethod = ConnectionHelper.GetClientConnectionString;
                    retPageGallery = PageGalleryHelper.GetGalleryByID(initObj, galleryID, PageID, groupID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetGallery->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetGallery-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retPageGallery;
        }

        //Get all gallery items for a specific gallery
        [WebMethod(EnableSession = true, Description = "Get all gallery items for a specific gallery")]
        public List<GalleryItem> GetGalleryContent(InitializationObject initObj, long ID, long PageID, string picSize, int pageSize, int start_index)
        {
            List<GalleryItem> lstGalleryItem = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGalleryContent", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetGalleryContent-> [{0}, {1}], Params:[ID: {2}, PageID: {3}, picSize: {4}, pageSize: {5}, start_index: {6}]", groupID, initObj.Platform, ID, PageID, picSize, pageSize, start_index);

            if (groupID > 0)
            {
                try
                {
                    lstGalleryItem = PageGalleryHelper.GetGalleryContent(initObj, ID, PageID, picSize, groupID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetGalleryContent->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetGalleryContent-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return lstGalleryItem;
        }


        //Get content from specific gallery items
        [WebMethod(EnableSession = true, Description = "Get content from specific gallery items")]
        public List<Media> GetGalleryItemContent(InitializationObject initObj, long ItemID, long GalleryID, long PageID, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGalleryContent", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetGalleryItemContent-> [{0}, {1}], Params:[ItemID: {2}, GalleryID: {3}, PageID: {4}, picSize: {5}, pageSize: {6}, pageIndex: {7}]", groupID, initObj.Platform, ItemID, GalleryID, PageID, picSize, pageSize, pageIndex);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = PageGalleryHelper.GetGalleryItemContent(initObj, PageID, GalleryID, ItemID, picSize, groupID, pageSize, pageIndex, orderBy);
                }
                catch (Exception ex)
                {
                    logger.Error("GetGalleryItemContent->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetGalleryItemContent-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
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

            logger.InfoFormat("GetGroupRules-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetGroupRules();
                }
                catch (Exception ex)
                {
                    logger.Error("GetGroupRules->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetGroupRules-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get User Group Rules")]
        public TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetUserGroupRules(InitializationObject initObj)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUserGroupRules-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetUserGroupRules(initObj.SiteGuid);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserGroupRules->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetUserGroupRules-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Set User Group Rule")]
        public bool SetUserGroupRule(InitializationObject initObj, int ruleID, string PIN, int isActive)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SetUserGroupRule-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetUserGroupRule(initObj.SiteGuid, ruleID, PIN, isActive);
                }
                catch (Exception ex)
                {
                    logger.Error("SetUserGroupRule->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SetUserGroupRule-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Check Parental PIN")]
        public bool CheckParentalPIN(InitializationObject initObj, int ruleID, string PIN)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CheckParentalPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("CheckParentalPIN-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).CheckParentalPIN(initObj.SiteGuid, ruleID, PIN);
                }
                catch (Exception ex)
                {
                    logger.Error("CheckParentalPIN->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("CheckParentalPIN-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }        

        [WebMethod(EnableSession = true, Description = "Get Secured SiteGuid")]
        public string GetSecuredSiteGuid(InitializationObject initObj)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSecuredSiteGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetSecuredSiteGuid-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

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
                    logger.Error("GetSecuredSiteGuid->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetSecuredSiteGuid-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Get Group Operators")]
        public TVPPro.SiteManager.TvinciPlatform.api.GroupOperator[] GetGroupOperators(InitializationObject initObj, string scope)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupOperator[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupOperators", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetGroupOperators-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetGroupOperators(scope);
                }
                catch (Exception ex)
                {
                    logger.Error("GetGroupOperators->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetGroupOperators-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "SSO Signin")]
        public UserResponseObject SSOSignIn(InitializationObject initObj, string userName, string password, int providerID)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SSOSignIn", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SSOSignIn-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SSOSignIn(userName, password, providerID, string.Empty, SiteHelper.GetClientIP(), initObj.UDID, false);
                }
                catch (Exception ex)
                {
                    logger.Error("SSOSignIn->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SSOSignIn-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Check SSO Login")]
        public UserResponseObject SSOCheckLogin(InitializationObject initObj, string userName, int providerID)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SSOCheckLogin", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SSOCheckLogin-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SSOCheckLogin(userName, providerID);
                }
                catch (Exception ex)
                {
                    logger.Error("SSOCheckLogin->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SSOCheckLogin-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get user data by co-guid")]
        public UserResponseObject GetUserDataByCoGuid(InitializationObject initObj, string coGuid, int operatorID)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserDataByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUserDataByCoGuid-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserDataByCoGuid(coGuid, operatorID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserDataByCoGuid->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetUserDataByCoGuid-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get from Secured SiteGuid")]
        public string GetSiteGuidFromSecured(InitializationObject initObj, string encSiteGuid)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSiteGuidFromSecured", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetSiteGuidFromSecured-> [{0}, {1}], Params:[userName: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

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
                    logger.Error("GetSiteGuidFromSecured->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetSiteGuidFromSecured-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return sRet;
        }

        [WebMethod(EnableSession = true, Description = "Validate user")]
        public string GetSiteGuid(InitializationObject initObj, string userName, string password)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.User sRet = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ValidateUser", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("ValidateUser-> [{0}, {1}], Params:[userName: {2}, password: {3}]", groupID, initObj.Platform, userName, password);

            if (groupID > 0)
            {
                try
                {
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    sRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).ValidateUser(userName, password, isSingleLogin).m_user;
                }
                catch (Exception ex)
                {
                    logger.Error("ValidateUser->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("ValidateUser-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return sRet != null ? sRet.m_sSiteGUID : string.Empty;
        }

        [WebMethod(EnableSession = true, Description = "Sign-In a user")]
        public TVPApiModule.Services.ApiUsersService.LogInResponseData SignIn(InitializationObject initObj, string userName, string password)
        {
            TVPApiModule.Services.ApiUsersService.LogInResponseData responseData = new TVPApiModule.Services.ApiUsersService.LogInResponseData();
            
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            logger.InfoFormat("SignIn-> [{0}, {1}], Params:[userName: {2}, password: {3}]", groupID, initObj.Platform, userName, password);

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
                    logger.Error("SignIn->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SignIn-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return responseData;
        }

        [WebMethod(EnableSession = true, Description = "Has user connected to FB")]
        public bool IsFacebookUser(InitializationObject initObj)
        {
            bool bRes = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsFacebookUser", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("IsFacebookUser-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    UserResponseObject userObj = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserData(initObj.SiteGuid);
                    bRes = !string.IsNullOrEmpty(userObj.m_user.m_oBasicData.m_sFacebookID);
                }
                catch (Exception ex)
                {
                    logger.Error("IsFacebookUser->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("IsFacebookUser-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return bRes;
        }

        [WebMethod(EnableSession = true, Description = "Sign-Up a new user")]
        public UserResponseObject SignUp(InitializationObject initObj, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, 
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignUp", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SignUp-> [{0}, {1}], Params:[userName: {2}, password: {3}]", groupID, initObj.Platform, userBasicData.m_sUserName, sPassword);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SignUp(userBasicData, userDynamicData, sPassword, sAffiliateCode);
                }
                catch (Exception ex)
                {
                    logger.Error("SignUp->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SignUp-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Sign-Out a user")]
        public void SignOut(InitializationObject initObj)
        {            
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SignOut-> [{0}, {1}], Params:[siteGuid: {2}, password: {3}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    //XXX: Do the UDID empty stuff
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SignOut(initObj.SiteGuid, initObj.UDID, string.Empty, isSingleLogin);
                }
                catch (Exception ex)
                {
                    logger.Error("SignOut->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SignOut-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
        }

        [WebMethod(EnableSession = true, Description = "Check if user is signed in")]
        public bool IsUserSignedIn(InitializationObject initObj)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "IsUserSignedIn", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            logger.InfoFormat("IsUserSignedIn-> [{0}, {1}], Params:[siteGuid: {2}, password: {3}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    //XXX: Do the UDID empty stuff
                    bool isSingleLogin = TVPApi.ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.SingleLogin.SupportFeature;
                    bRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).IsUserLoggedIn(initObj.SiteGuid, initObj.UDID, string.Empty, SiteHelper.GetClientIP(), isSingleLogin);
                }
                catch (Exception ex)
                {
                    logger.Error("IsUserSignedIn->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("IsUserSignedIn-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Edit user details info")]
        public UserResponseObject SetUserData(InitializationObject initObj, string sSiteGuid, TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData userBasicData, 
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData userDynamicData)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SignIn-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, sSiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserData(sSiteGuid, userBasicData, userDynamicData);
                }
                catch (Exception ex)
                {
                    logger.Error("SetUserData->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SetUserData-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            
            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get user details info")]
        public UserResponseObject GetUserData(InitializationObject initObj, string sSiteGuid)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUserData-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, sSiteGuid);

            if (groupID > 0)
            {
                try
                {
                    string siteGuid = (string.IsNullOrEmpty(sSiteGuid)) ? initObj.SiteGuid : sSiteGuid;
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserData(siteGuid);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserData->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetUserData-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get users details info")]
        public UserResponseObject[] GetUsersData(InitializationObject initObj, string sSiteGuid)
        {
            UserResponseObject[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUsersData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUsersData-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, sSiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUsersData(sSiteGuid);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserData->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetUserData-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get user CA status")]
        public TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus GetUserCAStatus(InitializationObject initObj)
        {
            TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus response = TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.Annonymus;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserCAStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUserCAStatus-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiConditionalAccessService(groupID, initObj.Platform).GetUserCAStatus(initObj.SiteGuid);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserCAStatus->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetUserCAStatus-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Forgot password")]
        public bool SendNewPassword(InitializationObject initObj, string sUserName)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SendNewPassword", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SendNewPassword-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    bRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SentNewPasswordToUser(sUserName);
                }
                catch (Exception ex)
                {
                    logger.Error("SendNewPassword->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SendNewPassword-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return bRet;
        }

        [WebMethod(EnableSession = true, Description = "Set specific dynamic user key data")]
        public bool SetUserDynamicData(InitializationObject initObj, string sKey, string sValue)
        {
            bool bRet = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserDynamicData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SetUserDynamicData-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    bRet = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserDynamicData(initObj.SiteGuid, sKey, sValue);
                }
                catch (Exception ex)
                {
                    logger.Error("SetUserDynamicData->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SetUserDynamicData-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return bRet;
        }
        #endregion
        
        #region XXXX
        
        [WebMethod(EnableSession = true, Description = "Do Social Action")]
        public string DoSocialAction(InitializationObject initObj, int mediaID, SocialAction socialAction, SocialPlatform socialPlatform, string actionParam)
        {
            string sRes = SocialActionResponseStatus.UNKNOWN.ToString();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "DoSocialAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("DoSocialAction-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupID, initObj.Platform);
                    SocialActionResponseStatus response = service.DoSocialAction(mediaID, initObj.SiteGuid, socialAction, socialPlatform, actionParam);

                    if (response == SocialActionResponseStatus.OK || response == SocialActionResponseStatus.INVALID_ACCESS_TOKEN)
                        sRes = response.ToString();
                    else
                        sRes = SocialActionResponseStatus.ERROR.ToString();

                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : DoSocialAction, Error Message: {0} Parameters: udid: {1}", ex.Message, initObj.UDID);
                }
            }

            return sRes;
        }

        [WebMethod(EnableSession = true, Description = "Get user social actions")]
        public UserSocialActionObject[] GetUserSocialActions(InitializationObject initObj, SocialAction socialAction, SocialPlatform socialPlatform, bool isOnlyFriends, int startIndex, int numOfItems)
        {
            UserSocialActionObject[] res = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserSocialActions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetUserSocialActions-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiSocialService service = new TVPApiModule.Services.ApiSocialService(groupID, initObj.Platform);
                    res = service.GetUserSocialActions(initObj.SiteGuid, socialAction, socialPlatform, isOnlyFriends, startIndex, numOfItems);

                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : GetUserSocialActions, Error Message: {0} Parameters: udid: {1}", ex.Message, initObj.UDID);
                }
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Do Post Reg Action")]
        public string PostRegAction(InitializationObject initObj, string actionName)
        {
            string retVal = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "DoSocialAction", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("DoSocialAction-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

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
                    logger.ErrorFormat("Error calling webservice protocol : DoSocialAction, Error Message: {0} Parameters: udid: {1}", ex.Message, initObj.UDID);
                }
            }
                        
            return retVal;
        }
        #endregion

        #region Translation

        [WebMethod(EnableSession = true, Description = "Get translations for all active languages")]
        public Pair[] GetTranslations(InitializationObject initObj)
        {
            Pair[] retTranslations = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetBottomProfile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetTranslations-> [{0}, {1}]", groupID, initObj.Platform);

            if (groupID > 0)
            {
                try
                {
                    retTranslations = TranslationHelper.GetTranslations(groupID, initObj.Platform);
                }
                catch (Exception ex)
                {
                    logger.Error("GetTranslations->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetTranslations-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return retTranslations;
        }

        #endregion
    }
}
