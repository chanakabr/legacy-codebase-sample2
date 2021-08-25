using System;
using System.Collections.Generic;

namespace ApiObjects.Pricing
{
    // TODO move to DAL/DTO
    public class DiscountDTO
    {
        public double Price { get; }
        public double Percentage { get; }
        public int CurrencyId { get; }
        public string CountryId { get; }

        public DiscountDTO(double price, double percentage, int сurrencyId, string countryId)
        {
            CountryId = countryId;
            CurrencyId = сurrencyId;
            Percentage = percentage;
            Price = price;
        }
    }

    public class DiscountDetailsDTO
    {
        public string Name { get; set; }
        public List<DiscountDTO> Discounts { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public WhenAlgoType WhenAlgoType { get; set; }
        public int WhenAlgoTimes { get; set; }
        
    }
}
