using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiObjects
{
    public class PaymentPartnerConfig
    {
        public List<UnifiedBillingCycleObject> UnifiedBillingCycles { get; set; }

        public bool SetUnchangedProperties(PaymentPartnerConfig oldConfig)
        {
            var needToUpdate = false;
            if (this.UnifiedBillingCycles != null)
            {
                foreach (var unifiedBillingCycle in this.UnifiedBillingCycles)
                {
                    var oldUnifiedBillingCycle = oldConfig.UnifiedBillingCycles.FirstOrDefault(x => x.Equals(unifiedBillingCycle));
                    if (oldUnifiedBillingCycle == null)
                    {
                        needToUpdate = true;
                    }
                    else
                    {
                        var needToUpdateCycle = unifiedBillingCycle.SetUnchangedProperties(oldUnifiedBillingCycle);
                        if (needToUpdateCycle)
                        {
                            needToUpdate = true;
                        }
                    }
                }
            }
            else
            {
                this.UnifiedBillingCycles = oldConfig.UnifiedBillingCycles;
            }

            return needToUpdate;
        }
    }

    [Serializable]
    [JsonObject]
    public class UnifiedBillingCycleObject
    {
        // cannot be null from phoenix api
        [JsonProperty]
        public string Name { get; set; }

        // cannot be null from phoenix api
        [JsonProperty]
        public Duration Duration { get; set; }

        [JsonProperty]
        public int? PaymentGatewayId { get; set; }

        [JsonProperty]
        public bool? IgnorePartialBilling { get; set; }

        internal bool SetUnchangedProperties(UnifiedBillingCycleObject oldUnifiedBillingCycleObject)
        {
            var needToUpdate = false;

            if (!this.PaymentGatewayId.HasValue && oldUnifiedBillingCycleObject.PaymentGatewayId.HasValue)
            {
                needToUpdate = true;
                this.PaymentGatewayId = oldUnifiedBillingCycleObject.PaymentGatewayId;
            }

            if (!this.IgnorePartialBilling.HasValue && oldUnifiedBillingCycleObject.IgnorePartialBilling.HasValue)
            {
                needToUpdate = true;
                this.IgnorePartialBilling = oldUnifiedBillingCycleObject.IgnorePartialBilling;
            }

            return needToUpdate;
        }

        public bool Equals(UnifiedBillingCycleObject other)
        {
            if (this.Duration == null || other.Duration == null)
            {
                return false;
            }

            return this.Duration.Equals(other.Duration);
        }
    }
}