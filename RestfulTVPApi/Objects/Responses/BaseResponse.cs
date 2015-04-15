using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.Objects.Models;
using Newtonsoft.Json;

namespace RestfulTVPApi.Objects.Responses
{
    public class BaseResponse
    {
        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public BaseResponse(int code, string message)
        {
            Status = new Status(code, message);
        }

        public BaseResponse()
        {
        }
    }
}