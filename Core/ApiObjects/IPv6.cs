using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects
{
    public class IPV6
    {
        public int countryId;
        public string code;
        public string name;
        public string ipv6_to;
        public string ipv6_from;
        [JsonIgnore]
        public string id;

        public IPV6(Tuple<string, string> tuple, int countryId, string code, string name)
        {
            ipv6_from = tuple.Item1;
            ipv6_to = tuple.Item2;

            this.countryId = countryId;
            this.code = code;
            this.name = name;
        }
    }
}
