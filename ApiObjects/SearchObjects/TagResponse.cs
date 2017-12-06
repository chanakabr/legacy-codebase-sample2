using ApiObjects.Response;
using System.Collections.Generic;


namespace ApiObjects.SearchObjects
{
    public class TagResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<TagValue> TagValues { get; set; }

        public int TotalItems { get; set; }

        public TagResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            TagValues = new List<TagValue>();
            TotalItems = 0;
        }
    }
}
