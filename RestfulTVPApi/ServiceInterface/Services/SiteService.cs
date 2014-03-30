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
            return _repository.GetFooter(request);
        }

        public object Get(GetGalleryRequest request)
        {
            return _repository.GetGallery(request);
        }

        public object Get(GetGalleryContentRequest request)
        {
            return _repository.GetGalleryContent(request);
        }

        public object Get(GetGalleryItemContentRequest request)
        {
            return _repository.GetGalleryItemContent(request);
        }

        public object Get(GetMenuRequest request)
        {
            return _repository.GetMenu(request);
        }

        public object Get(GetPageRequest request)
        {
            return _repository.GetPage(request);
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
