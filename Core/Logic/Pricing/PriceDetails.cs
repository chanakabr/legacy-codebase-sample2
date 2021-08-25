using System;
using System.Collections.Generic;

namespace Core.Pricing
{
    [Serializable]
    public class PriceDetails
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<Price> Prices { get; set; }

        public PriceDetails()
        {
        }

        public PriceDetails(PriceDetails pc)
        {
            Name = pc.Name;
            Id = pc.Id;

            Prices = new List<Price>();

            foreach (var item in pc.Prices)
            {
                Prices.Add(new Price(item));
            }
        }
    }
}
