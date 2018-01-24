using ApiObjects.Response;

namespace Core.Catalog.CatalogManagement
{
    public class AssetFileResponse
    {
        public Status Status { get; set; }

        public AssetFile File { get; set; }

        public AssetFileResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());            
        }
    }
}