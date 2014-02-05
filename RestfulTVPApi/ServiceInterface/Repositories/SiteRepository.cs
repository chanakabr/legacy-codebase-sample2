using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Context;
using TVPApiModule.Helper;
using TVPApiModule.Interfaces;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class SiteRepository : ISiteRepository
    {
        public Menu GetFooter(InitializationObject initObj, long ID)
        {
            Menu retMenu = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetFooter", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retMenu = MenuHelper.GetFooterByID(initObj.Platform, initObj.Locale, ID, groupID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retMenu;
        }

        public PageGallery GetGallery(InitializationObject initObj, long galleryID, long PageID)
        {
            PageGallery retPageGallery = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPageGalleries", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                //Ofir - tell irena to change this
                //ODBCWrapper.Connection.GetDefaultConnectionStringMethod = ConnectionHelper.GetClientConnectionString;
                //retPageGallery = PageGalleryHelper.GetGalleryByID(initObj.Platform, initObj.Locale, galleryID, PageID, groupID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retPageGallery;
        }

        public List<GalleryItem> GetGalleryContent(InitializationObject initObj, long ID, long PageID, string picSize, int pageSize, int start_index)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGalleryContent", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return PageGalleryHelper.GetGalleryContent(initObj.Platform, initObj.Locale, ID, PageID, picSize, groupID);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<Media> GetGalleryItemContent(InitializationObject initObj, long ItemID, long GalleryID, long PageID, string picSize, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGalleryContent", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                //Ofir = What??????
                //XXX: Patch for ximon
                if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("v1_6/") && groupID == 109 && initObj.Platform == PlatformType.iPad)
                    pageIndex = pageIndex / pageSize;

                lstMedia = PageGalleryHelper.GetGalleryItemContent(initObj.Platform, initObj.UDID, initObj.Locale, PageID, GalleryID, ItemID, picSize, groupID, pageSize, pageIndex, orderBy);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        public Menu GetMenu(InitializationObject initObj, long ID)
        {
            Menu retMenu = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMenu", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                //retMenu = MenuHelper.GetMenuByID(initObj, ID, groupID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retMenu;
        }

        public PageContext GetPage(InitializationObject initObj, long ID, bool withMenu, bool withFooter)
        {
            PageContext retPageContext = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPage", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                retPageContext = PageDataHelper.GetPageContextByID(initObj.Platform, groupID, initObj.Locale, ID, withMenu, withFooter);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retPageContext;
        }
    }
}