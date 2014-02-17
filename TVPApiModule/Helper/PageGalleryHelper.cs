using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.Helper;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Context;
using TVPApiModule.Manager;
using TVPApiModule.Objects;

/// <summary>
/// Summary description for PageGalleryHelper
/// </summary>
/// 

namespace TVPApiModule.Helper
{
    public class PageGalleryHelper
    {
        public PageGalleryHelper()
        {

        }

        //Get all page's galleries
        public static List<PageGallery> GetPageGallerisByPageID(InitializationObject initObj, long ID, int groupID, int pageSize, int startIndex)
        {
            List<PageGallery> retVal = null;
            PageContext page = PageDataHelper.GetPageContextByID(initObj.Platform, groupID, initObj.Locale, ID, false, false);
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
        //public static List<TVPApi.PageGallery> GetPageGallerisByPageToken(InitializationObject initObj, TVPApi.Pages token, int groupID)
        //{
        //    List<TVPApi.PageGallery> retVal = null;
        //    TVPApi.PageContext page = PageDataHelper.GetPageContextByToken(initObj.Platform, groupID, initObj.Locale, token, false, false);
        //    if (page != null)
        //    {
        //        retVal = page.GetGalleries();
        //    }
        //    return retVal;
        //}


        //Get specific gallery ID
        public static PageGallery GetGalleryByID(PlatformType platform, Locale locale, long ID, long PageID, int groupID)
        {
            PageGallery retVal = null;
            TVPApiModule.Objects.SiteMap siteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, platform, locale);
            if (siteMap != null)
            {
                PageContext page = (from pages in siteMap.GetPages()
                                    where pages.ID == PageID
                                    select pages).FirstOrDefault() as PageContext;

                if (page != null)
                {
                    retVal = (from galleries in page.GetGalleries()
                              where galleries.gallery_id == ID
                              select galleries).FirstOrDefault() as PageGallery;
                }
            }
            return retVal;
        }


        //Get Gallery Items for the gallery
        public static List<GalleryItem> GetGalleryContent(PlatformType platform, Locale locale, long PageID, long ID, string picSize, int groupID)
        {
            List<GalleryItem> retVal = new List<GalleryItem>();
            PageGallery gallery = GetGalleryByID(platform, locale, PageID, ID, groupID);
            if (gallery != null)
            {
                retVal = gallery.gallery_items;
            }
            return retVal;
        }

        public static List<Media> GetGalleryItemContent(PlatformType platform, string udid, Locale locale, long PageID, long GalleryID, long ItemID, string picSize, int groupID, int pageSize, int pageIndex, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderObj orderObj)
        {
            List<Media> retVal = new List<Media>();
            PageGallery gallery = GetGalleryByID(platform, locale, GalleryID, PageID, groupID);
            if (gallery != null)
            {
                GalleryItem item = (from items in gallery.gallery_items
                                    where items.item_id == ItemID
                                    select items).FirstOrDefault();
                if (item != null)
                {
                    retVal = new APIChannelMediaLoader((int)item.tvm_channel_id, groupID, platform, udid, SiteHelper.GetClientIP(), pageSize, pageIndex, picSize, locale.LocaleLanguage, orderObj, null, Tvinci.Data.Loaders.TvinciPlatform.Catalog.CutWith.WCF_ONLY_DEFAULT_VALUE)
                    {
                        UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                    }.Execute() as List<Media>;
                }
            }
            return retVal;
        }


    }
}
