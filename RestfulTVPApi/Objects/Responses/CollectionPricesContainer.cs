using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class CollectionPricesContainer
    {
        public string collection_code { get; set; }

        public Price price { get; set; }

        public PriceReason price_reason { get; set; }
    }
}
