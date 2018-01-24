using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class MediaFileTypeResponse
    {
        public Status Status { get; set; }
        public MediaFileType MediaFileType { get; set; }

        public MediaFileTypeResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            MediaFileType = null;
        }
    }
}
