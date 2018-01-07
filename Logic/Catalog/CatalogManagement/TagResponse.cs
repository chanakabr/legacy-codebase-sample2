using ApiObjects.Response;
using System.Collections.Generic;

namespace Core.Catalog.CatalogManagement
{
    public class TagResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<ApiObjects.SearchObjects.TagValue> TagValues { get; set; }

        public int TotalItems { get; set; }

        public TagResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            TagValues = new List<ApiObjects.SearchObjects.TagValue>();
            TotalItems = 0;
        }
    }
}
