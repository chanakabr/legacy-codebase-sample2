using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class PreviewModule
    {
        public long id { get; set; }

        public string name { get; set; }

        public int full_life_cycle { get; set; }

        public int non_renew_period { get; set; }
    }
}
