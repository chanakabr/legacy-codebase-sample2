using Newtonsoft.Json;
using System;

namespace ApiObjects.ConditionalAccess
{
    [JsonObject()]
    public class DomainSubscriptionPurchase
    {
        [JsonProperty()]
        public long PurchaseId
        {
            get;
            set;
        }

        [JsonProperty()]
        public bool IsRecurringStatus
        {
            get;
            set;
        }

        [JsonProperty()]
        public long PurchasingUserId
        {
            get;
            set;
        }

        [JsonProperty()]
        public long UnifiedProcessId
        {
            get;
            set;
        }

        [JsonProperty()]
        public DateTime? EndDate
        {
            get;
            set;
        }
    }
}
