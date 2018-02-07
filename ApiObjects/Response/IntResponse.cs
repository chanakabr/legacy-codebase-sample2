using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Response
{
    public class IntResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public int Value { get; set; }

        public IntResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

    }
}
