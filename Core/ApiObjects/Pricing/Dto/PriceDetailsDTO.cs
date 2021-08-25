using System.Collections.Generic;

namespace ApiObjects.Pricing
{
    // dto exist because PriceDetails class is in apilogic dll and not apiobjects
    public class PriceDetailsDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<PriceCodeLocaleDTO> Prices { get; set; }
    }

    public class PriceCodeLocaleDTO
    {
        public double Price { get; set; }
        public long CurrencyId { get; set; }
        public long CountryId { get; set; } // readonly, set in get
        public string CountryCode { get; set; } // set in add / update
    }
}