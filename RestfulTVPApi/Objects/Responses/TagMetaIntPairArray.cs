using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects.Responses
{
    public class TagMetaIntPairArray
    {
        public string key { get; set; }
        public List<int> values { get; set; }
    }
}