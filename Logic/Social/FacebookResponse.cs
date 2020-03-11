using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Social
{
    public class FacebookResponse
    {
        public FacebookResponseObject ResponseData { get; set; }

        public ApiObjects.Response.Status Status { get; set; }

        public FacebookResponse()
        {
            this.ResponseData = new FacebookResponseObject();
            this.Status = new ApiObjects.Response.Status();
        }
    }
}
