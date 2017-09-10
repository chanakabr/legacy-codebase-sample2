using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.NewCatalogManagement
{
    public class AssetStructListResponse
    {

        public Status Status { get; set; }

        public List<AssetStruct> AssetStructs { get; set; }

        public AssetStructListResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            AssetStructs = new List<AssetStruct>();
        }

    }
}
