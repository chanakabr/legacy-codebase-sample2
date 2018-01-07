using ApiObjects.Response;
using System.Collections.Generic;

namespace Core.Catalog.CatalogManagement
{
    public class RatioListResponse
    {
        public Status Status { get; set; }

        public List<Ratio> Ratios { get; set; }

        public int TotalItems { get; set; }

        public RatioListResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Ratios = new List<Ratio>();
        }
    }
}
