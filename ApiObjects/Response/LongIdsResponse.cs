using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Response
{
    public class LongIdsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<long> Ids { get; set; }

        public LongIdsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            Ids = new List<long>();
        }

        public LongIdsResponse(ApiObjects.Response.Status status, List<long> ids)
        {
            this.Status = status;
            this.Ids = ids;
        }
    }
}
