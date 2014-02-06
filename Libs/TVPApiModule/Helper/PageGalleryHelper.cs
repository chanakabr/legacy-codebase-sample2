using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.Helper;

/// <summary>
/// Summary description for PageGalleryHelper
/// </summary>
/// 

namespace TVPApi
{
    public class PageGalleryHelper
    {
        public PageGalleryHelper()
        {

        }

        //Get all page's galleries
        public static List<TVPApi.PageGallery> GetPageGallerisByPageID(InitializationObject initObj, long ID, int groupID, int pageSize, int startIndex)
        {
            List<TVPApi.PageGallery> retVal = null;
            TVPApi.PageContext page = PageDataHelper.GetPageContextByID(initObj, groupID, ID, false, false);
            if (page != null)
            {
                retVal = page.GetGalleries();
                if (retVal != null && retVal.Count > (startIndex + pageSize))
                {
                    retVal = (retVal.Skip(startIndex).Take(pageSize)).ToList<PageGallery>();
                }
            }
            return retVal;
        }


        //Get all page galleries by page token
        public static List<TVPApi.PageGallery> GetPageGallerisByPageToken(InitializationObject initObj, TVPApi.Pages token, int groupID)
        {
            List<TVPApi.PageGallery> retVal = null;
            TVPApi.PageContext page = PageDataHelper.GetPageContextByToken(initObj, groupID, token, false, false);
            if (page != null)
            {
                retVal = page.GetGalleries();
            }
            return retVal;
        }


        //Get specific gallery ID
        public static TVPApi.PageGallery GetGalleryByID(InitializationObject initObj, long ID, long PageID, int groupID)
        {
            PageGallery retVal = null;
            TVPApi.SiteMap siteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            if (siteMap != null)
            {
                PageContext page = (from pages in siteMap.GetPages()
                                    where pages.ID == PageID
                                    select pages).FirstOrDefault() as PageContext;

                if (page != null)
                {
                    retVal = (from galleries in page.GetGalleries()
                              where galleries.GalleryID == ID
                              select galleries).FirstOrDefault() as PageGallery;
                }
            }
            return retVal;
        }


        //Get Gallery Items for the gallery
        public static List<GalleryItem> GetGalleryContent(InitializationObject initObj, long PageID, long ID, string picSize, int groupID)
        {
            List<GalleryItem> retVal = new List<GalleryItem>();
            PageGallery gallery = GetGalleryByID(initObj, PageID, ID, groupID);
            if (gallery != null)
            {
                retVal = gallery.GalleryItems;
            }
            return retVal;
        }

        public static List<Media> GetGalleryItemContent(InitializationObject initObj, long PageID, long GalleryID, long ItemID, string picSize, int groupID, int pageSize, int pageIndex, OrderBy orderBy)
        {
            List<Media> retVal = new List<Media>();
            PageGallery gallery = GetGalleryByID(initObj, GalleryID, PageID, groupID);
            if (gallery != null)
            {
                GalleryItem item = (from items in gallery.GalleryItems
                                    where items.ItemID == ItemID
                                    select items).FirstOrDefault();
                if (item != null)
                {
                    retVal = MediaHelper.GetMediaList(initObj, item.TVMUser, item.TVMPass, item.TVMChannelID, picSize, pageSize, pageIndex, groupID, MediaHelper.LoaderType.Channel, orderBy);
                }
            }
            return retVal;
        }


    }
}
