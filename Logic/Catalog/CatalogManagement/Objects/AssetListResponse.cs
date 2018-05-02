using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class AssetListResponse
    {
        public Status Status { get; set; }

        public List<Asset> Assets { get; set; }

        public AssetListResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Assets = new List<Asset>();
        }
    }
}
