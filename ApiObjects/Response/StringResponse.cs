using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Response
{
    public class StringResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public string Value { get; set; }

        public StringResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }

    }
}
