using ApiObjects;
using ApiObjects.Pricing;
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
        public WhenAlgoType WhenAlgoType { get; set; }
        public int WhenAlgoTimes { get; set; }

        public DiscountDetails()
        {
            this.MultiCurrencyDiscounts = new List<Discount>();
        }

        public DiscountDetails(DiscountDetails ddToCopy)
        {
            this.Name = ddToCopy.Name;
            this.Id = ddToCopy.Id;
            this.StartDate = ddToCopy.StartDate;
            this.EndDate = ddToCopy.EndDate;
            this.MultiCurrencyDiscounts = new List<Discount>(ddToCopy.MultiCurrencyDiscounts);
            this.WhenAlgoType = ddToCopy.WhenAlgoType;
            this.WhenAlgoTimes = ddToCopy.WhenAlgoTimes;
        }

        public static List<DiscountDTO> ConvertToDtos(List<Discount> MultiCurrencyDiscounts)
        {
            List<DiscountDTO> Discounts = new List<DiscountDTO>();

            MultiCurrencyDiscounts.ForEach(discount =>
            {
                Discounts.Add(new DiscountDTO(discount.m_dPrice, discount.Percentage, discount.m_oCurrency.m_nCurrencyID, discount.countryId));
            });

            return Discounts;
        }
    }

    [Serializable]
    public class Discount : Price
    {
        public double Percentage { get; set; }
    }

}
