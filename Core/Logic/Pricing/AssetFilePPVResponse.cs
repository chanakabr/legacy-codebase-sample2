using ApiObjects.Response;
using System.Collections.Generic;

namespace Core.Pricing
{
    public class AssetFilePPVResponse
    {
        public AssetFilePpv AssetFilePPV { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
        public AssetFilePPVResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }
    }

    public class AssetFilePPVListResponse
    {
        public List<AssetFilePpv> AssetFilePpvs { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
        public AssetFilePPVListResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }
    }
}
