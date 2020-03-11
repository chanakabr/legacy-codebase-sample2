using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class KSQLChannelResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public KSQLChannel Channel { get; set; }

        public KSQLChannelResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            Channel = new KSQLChannel();
        }
    }
}
