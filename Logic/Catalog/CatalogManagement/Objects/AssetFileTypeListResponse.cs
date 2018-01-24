using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class AssetFileTypeListResponse
    {

        public Status Status { get; set; }

        public List<MediaFileType> Types { get; set; }

        public AssetFileTypeListResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Types = new List<MediaFileType>();
        }

    }
}
