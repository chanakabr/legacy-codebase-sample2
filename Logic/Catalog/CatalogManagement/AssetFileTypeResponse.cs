using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class AssetFileTypeResponse
    {
        public Status Status { get; set; }
        public MediaFileType AssetFileType { get; set; }

        public AssetFileTypeResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            AssetFileType = null;
        }
    }
}
