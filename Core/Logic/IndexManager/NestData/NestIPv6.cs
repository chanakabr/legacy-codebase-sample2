using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.IndexManager.NestData
{
    public class NestIPv6

    {
        [Text()]
        public string ipv6_from { get; set; }
        [Text()]
        public string ipv6_to { get; set; }
        [Number()]
        public int country_id { get; set; }
        [Text()]
        public string code { get; set; }
        [Text()]
        public string name { get; set; }
        [Ignore()]
        public string id { get; set; }

        public NestIPv6(ApiObjects.IPV6 source)
        {
            this.ipv6_from = source.ipv6_from;
            this.ipv6_to = source.ipv6_to;
            this.country_id = source.countryId;
            this.code = source.code;
            this.name = source.name;
            this.id = source.id;
        }
    }
}
