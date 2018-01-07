using ApiObjects.Response;
using Core.Catalog.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class AssetResponse
    {
        public Status Status { get; set; }
        public Asset Asset { get; set; }

        public AssetResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Asset = null;
        }
    }
}
