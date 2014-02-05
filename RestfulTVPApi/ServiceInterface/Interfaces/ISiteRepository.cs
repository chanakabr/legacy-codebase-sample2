using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Context;

namespace RestfulTVPApi.ServiceInterface
{
    public interface ISiteRepository
    {
        Menu GetFooter(InitializationObject initObj, long ID);

        PageGallery GetGallery(InitializationObject initObj, long galleryID, long PageID);

        List<GalleryItem> GetGalleryContent(InitializationObject initObj, long ID, long PageID, string picSize, int pageSize, int start_index);

        List<Media> GetGalleryItemContent(InitializationObject initObj, long ItemID, long GalleryID, long PageID, string picSize, int pageSize, int pageIndex, OrderBy orderBy);

        Menu GetMenu(InitializationObject initObj, long ID);

        PageContext GetPage(InitializationObject initObj, long ID, bool withMenu, bool withFooter);

        
    }
}