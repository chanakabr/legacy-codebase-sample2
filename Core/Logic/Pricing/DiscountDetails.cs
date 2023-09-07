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
        public RelationTypes RelationType;

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

        public bool IsUpdateNedded(DiscountDetails oldPDiscountDetail)
        {
            bool shouldUpdate = false;

            if (StartDate > DateTimeOffset.FromUnixTimeSeconds(0) && !StartDate.Equals(oldPDiscountDetail.StartDate))
            {
                 shouldUpdate = true;
            }
            else
            {
                StartDate = oldPDiscountDetail.StartDate;
            }

            if (EndDate > DateTimeOffset.FromUnixTimeSeconds(0) && !EndDate.Equals(oldPDiscountDetail.EndDate))
            {
                shouldUpdate = true;
            }
            else
            {
                EndDate = oldPDiscountDetail.EndDate;
            }

            if (!string.IsNullOrEmpty(Name) && !Name.Equals(oldPDiscountDetail.Name))
            {
                shouldUpdate = true;
            }
            else
            {
                Name = oldPDiscountDetail.Name;
            }

            if (WhenAlgoTimes != 0 && WhenAlgoTimes != oldPDiscountDetail.WhenAlgoTimes)
            {
                shouldUpdate = true;
            }
            else
            {
                WhenAlgoTimes = oldPDiscountDetail.WhenAlgoTimes;
            }

            if (WhenAlgoType != 0 && WhenAlgoType != oldPDiscountDetail.WhenAlgoType)
            {
                shouldUpdate = true;
            }
            else
            {
                WhenAlgoType = oldPDiscountDetail.WhenAlgoType;
            }
            return shouldUpdate;
        }
    }

    [Serializable]
    public class Discount : Price
    {
        public double Percentage { get; set; }
        public bool IsDeafult { get; set; }
    }

}
