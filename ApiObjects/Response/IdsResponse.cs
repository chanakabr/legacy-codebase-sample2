using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Response
{
    public class IdsResponse
    {
         public ApiObjects.Response.Status resp { get; set; }
        public List<int> ids { get; set; }

        public IdsResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            ids = new List<int>();
        }

        public IdsResponse(ApiObjects.Response.Status resp, List<int> ids)
        {
            this.resp = resp;
            this.ids = ids;
        }
    }
}
