using ApiObjects.Response;
using System.Collections.Generic;

namespace Core.Catalog.CatalogManagement
{
    public class ImageTypeListResponse
    {
        public Status Status { get; set; }

        public List<Topic> ImageTypes { get; set; }

        public ImageTypeListResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            ImageTypes = new List<Topic>();
        }
    }
}
