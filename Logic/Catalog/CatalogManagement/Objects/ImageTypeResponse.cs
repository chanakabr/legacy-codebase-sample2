using ApiObjects.Response;

namespace Core.Catalog.CatalogManagement
{
    public class ImageTypeResponse
    {
        public Status Status { get; set; }

        public ImageType ImageType { get; set; }

        public ImageTypeResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());            
        }
    }
}
