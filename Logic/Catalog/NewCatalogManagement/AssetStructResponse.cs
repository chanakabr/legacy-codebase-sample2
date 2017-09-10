using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.NewCatalogManagement
{
    public class AssetStructResponse
    {
        public Status Status { get; set; }
        public AssetStruct AssetStruct { get; set; }        

        public AssetStructResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            AssetStruct = null;
        }
    }
}
