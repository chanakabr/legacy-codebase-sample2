using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Context;
using TVPApiModule.Helper;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class SiteRepository : ISiteRepository
    {
        public Menu GetFooter(GetFooterRequest request)
        {
            Menu retMenu = null;

            retMenu = MenuHelper.GetFooterByID(request.InitObj.Platform, request.InitObj.Locale, request.footer_id, request.GroupID);
            
            return retMenu;
        }

        public PageGallery GetGallery(GetGalleryRequest request)
        {
            PageGallery retPageGallery = null;
            
            //Ofir - tell irena to change this
            //ODBCWrapper.Connection.GetDefaultConnectionStringMethod = ConnectionHelper.GetClientConnectionString;
            //retPageGallery = PageGalleryHelper.GetGalleryByID(request.InitObj.Platform, request.InitObj.Locale, request.gallery_id, request.page_id, request.GroupID);            

            return retPageGallery;
        }

        public List<GalleryItem> GetGalleryContent(GetGalleryContentRequest request)
        {
            return PageGalleryHelper.GetGalleryContent(request.InitObj.Platform, request.InitObj.Locale, request.gallery_id, request.page_id, request.pic_size, request.GroupID);
        }

        public List<Media> GetGalleryItemContent(GetGalleryItemContentRequest request)
        {
            List<Media> lstMedia = null;

            //Ofir = What??????
                //XXX: Patch for ximon
                if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("v1_6/") && request.GroupID == 109 && request.InitObj.Platform == PlatformType.iPad)
                    request.page_number = request.page_number / request.page_size;

                lstMedia = PageGalleryHelper.GetGalleryItemContent(request.InitObj.Platform, request.InitObj.UDID, request.InitObj.Locale, request.page_id, request.gallery_id, request.item_id, request.pic_size, request.GroupID, request.page_size, request.page_number, null);//request.orderBy);
            
            return lstMedia;
        }

        public Menu GetMenu(GetMenuRequest request)
        {
            Menu retMenu = null;

            //retMenu = MenuHelper.GetMenuByID(request.InitObj, request.menu_id, request.GroupID);            

            return retMenu;
        }

        public PageContext GetPage(GetPageRequest request)
        {
            PageContext retPageContext = null;

            retPageContext = PageDataHelper.GetPageContextByID(request.InitObj.Platform, request.GroupID, request.InitObj.Locale, request.page_id, request.with_menu, request.with_footer);
            
            return retPageContext;
        }
    }
}