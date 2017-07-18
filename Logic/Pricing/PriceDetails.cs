using ApiObjects;
using System;
using System.Collections.Generic;

namespace Core.Pricing
{
    [Serializable]
    public class PriceDetails
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public List<Price> Prices { get; set; }
    }

    public class PriceDetailsResponse
    {
        public List<PriceDetails> PriceCodes { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
