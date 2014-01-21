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

        public HttpResult Get(GetFooterRequest request)
        {
            var response = _repository.GetFooter(request.InitObj, request.footer_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetGalleryRequest request)
        {
            var response = _repository.GetGallery(request.InitObj, request.gallery_id, request.page_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetGalleryContentRequest request)
        {
            var response = _repository.GetGalleryContent(request.InitObj, request.gallery_id, request.page_id, request.pic_size, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetGalleryItemContentRequest request)
        {
            var response = _repository.GetGalleryItemContent(request.InitObj, request.item_id, request.gallery_id, request.page_id, request.pic_size, request.page_size, request.page_number, request.order_by);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetMenuRequest request)
        {
            var response = _repository.GetMenu(request.InitObj, request.menu_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetPageRequest request)
        {
            var response = _repository.GetPage(request.InitObj, request.page_id, request.with_menu, request.with_footer);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
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
