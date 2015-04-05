using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class DiscountModule : PriceCode
    {
        public double percent { get; set; }

        public RelationTypes relation_type { get; set; }

        public System.DateTime start_date { get; set; }

        public System.DateTime end_date { get; set; }

        public WhenAlgo when_algo { get; set; }
    }
}
