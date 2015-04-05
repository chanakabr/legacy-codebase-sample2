using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class WhenAlgo
    {
        public WhenAlgoType algo_type { get; set; }

        public int n_times { get; set; }
    }
}
