using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using System.Collections.Generic;
using RestfulTVPApi.ServiceModel;
using System.Linq;
using TVPApi;
using ServiceStack;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using System;

namespace RestfulTVPApi.ServiceInterface
{

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class SiteService : Service
    {
        public ISiteRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public object Get(GetFooterRequest request)
        {
            return _repository.GetFooter(request.InitObj, request.footer_id);
        }

        public object Get(GetGalleryRequest request)
        {
            return _repository.GetGallery(request.InitObj, request.gallery_id, request.page_id);
        }

        public object Get(GetGalleryContentRequest request)
        {
            return _repository.GetGalleryContent(request.InitObj, request.gallery_id, request.page_id, request.pic_size, request.page_size, request.page_number);
        }

        public object Get(GetGalleryItemContentRequest request)
        {
            return _repository.GetGalleryItemContent(request.InitObj, request.item_id, request.gallery_id, request.page_id, request.pic_size, request.page_size, request.page_number, request.order_by);
        }

        public object Get(GetMenuRequest request)
        {
            return _repository.GetMenu(request.InitObj, request.menu_id);
        }

        public object Get(GetPageRequest request)
        {
            return _repository.GetPage(request.InitObj, request.page_id, request.with_menu, request.with_footer);
        }

        #endregion

        #region PUT
        #endregion

        #region POST
        #endregion

        #region DELETE
        #endregion
        
    }
}
