using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Response
{
    public class IdsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<int> Ids { get; set; }

        public IdsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            Ids = new List<int>();
        }

        public IdsResponse(ApiObjects.Response.Status status, List<int> ids)
        {
            this.Status = status;
            this.Ids = ids;
        }
    }
}
