
using System.Collections.Generic;

namespace ApiObjects.Response
{
    public class GenericListResponse<T>
    {
        public Status Status { get; set; }
        public List<T> Objects { get; set; }
        public int TotalItems { get; set; }

        public GenericListResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Objects = new List<T>();
            TotalItems = 0;
        }
    }
}
