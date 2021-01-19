using Core.Pricing;
using System.Collections.Generic;

namespace Core.ConditionalAccess.Response
{
    public class EntitlementPriceDetails
    {
        public Price FullPrice { get; set; }

        public List<EntitlementDiscountDetails> DiscountDetails { get; set; }

        public void AddDiscountDetails(EntitlementDiscountDetails entitlementDiscountDetails)
        {
            if (DiscountDetails == null)
            {
                DiscountDetails = new List<EntitlementDiscountDetails>();
            }

            DiscountDetails.Add(entitlementDiscountDetails);
        }
    }
}