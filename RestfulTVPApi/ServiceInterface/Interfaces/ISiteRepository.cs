using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Context;
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{
    public interface ISiteRepository
    {
        Menu GetFooter(GetFooterRequest request);

        PageGallery GetGallery(GetGalleryRequest request);

        List<GalleryItem> GetGalleryContent(GetGalleryContentRequest request);

        List<Media> GetGalleryItemContent(GetGalleryItemContentRequest request);

        Menu GetMenu(GetMenuRequest request);

        PageContext GetPage(GetPageRequest request);

        
    }
}