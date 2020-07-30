using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
                needToUpdate = true;
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
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public Duration Duration { get; set; }

        [JsonProperty]
        public int? PaymentGatewayId { get; set; }

        [JsonProperty]
        public bool? IgnorePartialBilling { get; set; }
    }
}
