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
        public SiteMap GetSiteMap(InitializationObject initObj, string ws_User, string ws_Pass)
        {
            SiteMap retSiteMap = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSiteMap", ws_User, ws_Pass, SiteHelper.GetClientIP());

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
                logger.ErrorFormat("GetSiteMap-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return retSiteMap;
        }

        #endregion

        #region Page

        //Get specific page from site map
        [WebMethod(EnableSession = true, Description = "Get specific page from site map")]
        public PageContext GetPage(InitializationObject initObj, string ws_User, string ws_Pass, long ID, bool withMenu, bool withFooter)
        {
            PageContext retPageContext = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPage", ws_User, ws_Pass, SiteHelper.GetClientIP());
            
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
                logger.ErrorFormat("GetPage-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return retPageContext;
        }

        //Get specific page from site map
        [WebMethod(EnableSession = true, Description = "Get specific page from site map")]
        public PageContext GetPageByToken(InitializationObject initObj, string ws_User, string ws_Pass, Pages token, bool withMenu, bool withFooter)
        {
            PageContext retPageContext = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPage", ws_User, ws_Pass, SiteHelper.GetClientIP());

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
                logger.ErrorFormat("GetPageByToken-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return retPageContext;
        }

        [WebMethod(EnableSession = true, Description = "Get site menu")]
        public Menu GetMenu(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
        {
            Menu retMenu = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMenu", ws_User, ws_Pass, SiteHelper.GetClientIP());

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
                logger.ErrorFormat("GetMenu-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return retMenu;
        }

        [WebMethod(EnableSession = true, Description = "Get site footer menu")]
        public Menu GetFooter(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
        {
            Menu retMenu = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetFooter", ws_User, ws_Pass, SiteHelper.GetClientIP());

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
                logger.ErrorFormat("GetFooter-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return retMenu;
        }

        [WebMethod(EnableSession = true, Description = "Get site side galleries")]
        public Profile GetSideProfile(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
        {
            Profile retProfile = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSideProfile", ws_User, ws_Pass, SiteHelper.GetClientIP());

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
                logger.ErrorFormat("GetSideProfile-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return retProfile;
        }

        //Get full bottom profile from site map
        [WebMethod(EnableSession = true, Description = "Get full bottom profile from site map")]
        public Profile GetBottomProfile(InitializationObject initObj, string ws_User, string ws_Pass, long ID)
        {
            Profile retProfile = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetBottomProfile", ws_User, ws_Pass, SiteHelper.GetClientIP());
            
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
                logger.ErrorFormat("GetBottomProfile-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return retProfile;
        }
        #endregion

        #region Galleries

        //Get all page galleries from site map
        [WebMethod(EnableSession = true, Description = "Get all page galleries from site map")]
        public List<PageGallery> GetPageGalleries(InitializationObject initObj, string ws_User, string ws_Pass, long PageID, int pageSize, int start_index)
        {
            List<PageGallery> lstPageGallery = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPageGalleries", ws_User, ws_Pass, SiteHelper.GetClientIP());

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
                logger.ErrorFormat("GetPageGalleries-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return lstPageGallery;
        }

        //Get all page galleries from site map
        [WebMethod(EnableSession = true, Description = "Get all page galleries from site map")]
        public PageGallery GetGallery(InitializationObject initObj, string ws_User, string ws_Pass, long galleryID, long PageID)
        {
            PageGallery retPageGallery = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPageGalleries", ws_User, ws_Pass, SiteHelper.GetClientIP());
            
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
                logger.ErrorFormat("GetGallery-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return retPageGallery;
        }

        //Get all gallery items for a specific gallery
        [WebMethod(EnableSession = true, Description = "Get all gallery items for a specific gallery")]
        public List<GalleryItem> GetGalleryContent(InitializationObject initObj, string ws_User, string ws_Pass, long ID, long PageID, string picSize, int pageSize, int start_index)
        {
            List<GalleryItem> lstGalleryItem = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGalleryContent", ws_User, ws_Pass, SiteHelper.GetClientIP());

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
                logger.ErrorFormat("GetGalleryContent-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return lstGalleryItem;
        }


        //Get content from specific gallery items
        [WebMethod(EnableSession = true, Description = "Get content from specific gallery items")]
        public List<Media> GetGalleryItemContent(InitializationObject initObj, string ws_User, string ws_Pass, long ItemID, long GalleryID, long PageID, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGalleryContent", ws_User, ws_Pass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetGalleryItemContent-> [{0}, {1}], Params:[ItemID: {2}, GalleryID: {3}, PageID: {4}, picSize: {5}, pageSize: {6}, pageIndex: {7}]", groupID, initObj.Platform, ItemID, GalleryID, PageID, picSize, pageSize, pageIndex);

            if (groupID > 0)
            {
                try
                {
                    lstMedia = PageGalleryHelper.GetGalleryItemContent(initObj, PageID, GalleryID, ItemID, picSize, groupID, pageSize, pageIndex);
                }
                catch (Exception ex)
                {
                    logger.Error("GetGalleryItemContent->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetGalleryItemContent-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return lstMedia;
        }

        #endregion

        #region SignIn/Logout
        [WebMethod(EnableSession = true, Description = "Sign-In a user")]
        public string SignIn(InitializationObject initObj, string ws_User, string ws_Pass, string userName, string password)
        {
            string sRet = string.Empty;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediaInfo", ws_User, ws_Pass, SiteHelper.GetClientIP());
            
            logger.InfoFormat("SignIn-> [{0}, {1}], Params:[userName: {2}, password: {3}]", groupID, initObj.Platform, userName, password);

            if (groupID > 0)
            {
                try
                {
                    ConnectionHelper.InitServiceConfigs(groupID, initObj.Platform);
                    sRet = ActionHelper.GetSiteGuid(userName, password, groupID, initObj.Platform);
                }
                catch (Exception ex)
                {
                    logger.Error("SignIn->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SignIn-> 'Unknown group' Username: {0}, Password: {1}", ws_User, ws_Pass);
            }

            return sRet;
        }
        #endregion
    }
}
