using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.IndexManager.NestData
{
    public class IPv4
    {
        [Number()]
        public long ip_from { get; set; }
        [Number()]
        public long ip_to { get; set; }
        [Number()]
        public int country_id { get; set; }
        [Text()]
        public string code { get; set; }
        [Text()]
        public string name { get; set; }
        [Text()]
        public string id { get; set; }

        public IPv4(ApiObjects.IPV4 source)
        {
            this.ip_from = source.ip_from;
            this.ip_to = source.ip_to;
            this.country_id = source.country_id;
            this.code = source.code;
            this.name = source.name;
            this.id = source.id;
        }
    }
}
