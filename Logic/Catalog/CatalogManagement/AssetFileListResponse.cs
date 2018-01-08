using ApiObjects.Response;
using System.Collections.Generic;

namespace Core.Catalog.CatalogManagement
{
    public class AssetFileListResponse
    {
        public Status Status { get; set; }

        public List<AssetFile> Files { get; set; }

        public AssetFileListResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Files = new List<AssetFile>();
        }
    }
}