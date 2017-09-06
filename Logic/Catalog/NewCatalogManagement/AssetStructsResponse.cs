using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.NewCatalogManagement
{
    public class AssetStructsResponse
    {

        public Status Status;

        public List<AssetStruct> AssetStructs { get; set; }

        public AssetStructsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            AssetStructs = new List<AssetStruct>();
        }

    }
}
