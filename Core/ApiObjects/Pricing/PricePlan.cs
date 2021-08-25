using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Pricing
{
    [Serializable]
    public class PricePlan
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public int? FullLifeCycle { get; set; }
        public int? ViewLifeCycle { get; set; }
        public int? MaxViewsNumber { get; set; }
        public bool? IsRenewable { get; set; }
        public long? PriceDetailsId { get; set; }
        public long? DiscountId { get; set; }
        public int? RenewalsNumber { get; set; }
        public bool? IsOfflinePlayBack { get; set; }
        public bool? IsWaiverEnabled { get; set; }
        public int? WaiverPeriod { get; set; }

        public bool IsNeedToUpdate(PricePlan oldPricePlan)
        {
            bool shouldUpdate = false;

            if (!string.IsNullOrEmpty(Name) && !string.Equals(Name, oldPricePlan.Name))
            {
                shouldUpdate = true;
            }
            else
            {
                Name = oldPricePlan.Name;
            }

            if (DiscountId.HasValue && DiscountId.Value != oldPricePlan.DiscountId)
            {
                shouldUpdate = true;
            }
            else
            {
                DiscountId = oldPricePlan.DiscountId;
            }

            if (PriceDetailsId.HasValue && PriceDetailsId.Value != oldPricePlan.PriceDetailsId)
            {
                shouldUpdate = true;
            }
            else
            {
                PriceDetailsId = oldPricePlan.PriceDetailsId;
            }

            if (MaxViewsNumber.HasValue && MaxViewsNumber.Value != oldPricePlan.MaxViewsNumber)
            {
                shouldUpdate = true;
            }
            else
            {
                MaxViewsNumber = oldPricePlan.MaxViewsNumber;
            }

            if (IsRenewable.HasValue && IsRenewable.Value != oldPricePlan.IsRenewable)
            {
                shouldUpdate = true;
            }
            else
            {
                IsRenewable = oldPricePlan.IsRenewable;
            }

            if (RenewalsNumber.HasValue && RenewalsNumber.Value != oldPricePlan.RenewalsNumber)
            {
                shouldUpdate = true;
            }
            else
            {
                RenewalsNumber = oldPricePlan.RenewalsNumber;
            }

            if (FullLifeCycle.HasValue && FullLifeCycle.Value != oldPricePlan.FullLifeCycle)
            {
                shouldUpdate = true;
            }
            else
            {
                FullLifeCycle = oldPricePlan.FullLifeCycle;
            }

            if (ViewLifeCycle.HasValue && ViewLifeCycle.Value != oldPricePlan.ViewLifeCycle)
            {
                shouldUpdate = true;
            }
            else
            {
                ViewLifeCycle = oldPricePlan.ViewLifeCycle;
            }
            if (IsOfflinePlayBack.HasValue && IsOfflinePlayBack.Value != oldPricePlan.IsOfflinePlayBack)
            {
                shouldUpdate = true;
            }
            else
            {
                IsOfflinePlayBack = oldPricePlan.IsOfflinePlayBack;
            }
            if (WaiverPeriod.HasValue && WaiverPeriod.Value != oldPricePlan.WaiverPeriod)
            {
                shouldUpdate = true;
            }
            else
            {
                WaiverPeriod = oldPricePlan.WaiverPeriod;
            }
            if (IsWaiverEnabled.HasValue && IsWaiverEnabled.Value != oldPricePlan.IsWaiverEnabled)
            {
                shouldUpdate = true;
            }
            else
            {
                IsWaiverEnabled = oldPricePlan.IsWaiverEnabled;
            }

            return shouldUpdate;
        }
    }
}
