using ApiObjects.Response;

namespace Core.Catalog.CatalogManagement
{
    public class ImageResponse
    {
        public Status Status { get; set; }

        public Image Image { get; set; }

        public ImageResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());            
        }
    }
}
