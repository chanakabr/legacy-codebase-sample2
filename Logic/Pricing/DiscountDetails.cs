using ApiObjects;
using System;
using System.Collections.Generic;

namespace Core.Pricing
{
    [Serializable]
    public class DiscountDetails
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public List<Discount> MultiCurrencyDiscounts { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class Discount : Price
    {
        public double Percentage { get; set; }
    }

}
