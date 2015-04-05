using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class NetworkResponseObject
    {
        public bool IsSuccess { get; set; }

        public NetworkResponseStatus Reason { get; set; }
    }
}
