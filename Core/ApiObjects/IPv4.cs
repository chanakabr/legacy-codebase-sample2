using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects
{
    public class IPV4
    {
        public long ip_from;
        public long ip_to;
        public int country_id;
        public string code;
        public string name;
        public string id;

        public IPV4(string id, int countryId, string code, string name, long ipFrom, long ipTo)
        {
            this.id = id;
            this.ip_from = ipFrom;
            this.ip_to = ipTo;
            this.country_id = countryId;
            this.code = code;
            this.name = name;
        }
    }
}
