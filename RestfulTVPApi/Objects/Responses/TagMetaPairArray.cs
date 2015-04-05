using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects.Responses
{
    public struct TagMetaPairArray
    {
        public string key { get; set; }

        public string[] values { get; set; }
    }
}
