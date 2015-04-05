using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    [Serializable]
    public class Country
    {
        public int object_id { get; set; }

        public string country_name { get; set; }

        public string country_code { get; set; }
    }
}
