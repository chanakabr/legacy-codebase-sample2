using ApiObjects.Response;
using System.Collections.Generic;

namespace Core.Catalog.CatalogManagement
{
    public class ImageListResponse
    {
        public Status Status { get; set; }

        public List<Image> Image { get; set; }

        public int TotalItems { get; set; }

        public ImageListResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Image = new List<Image>();
        }
    }
}
