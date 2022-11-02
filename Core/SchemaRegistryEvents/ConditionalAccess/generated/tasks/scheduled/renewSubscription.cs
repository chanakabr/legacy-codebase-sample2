namespace Phoenix.Generated.Tasks.Scheduled.renewSubscription
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// renew subscription scheduled task definition.
    ///
    /// A base event for all scheduled tasks
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class RenewSubscription
    {
        [JsonProperty("partnerId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        [JsonProperty("billingGuid", NullValueHandling = NullValueHandling.Ignore)]
        public string BillingGuid { get; set; }

        [JsonProperty("endDate", NullValueHandling = NullValueHandling.Ignore)]
        public long? EndDate { get; set; }

        [JsonProperty("householdId", NullValueHandling = NullValueHandling.Ignore)]
        public long? HouseholdId { get; set; }

        /// <summary>
        /// The process id (the id of the document in the db) of unified renew
        /// </summary>
        [JsonProperty("processId", NullValueHandling = NullValueHandling.Ignore)]
        public long? ProcessId { get; set; }

        /// <summary>
        /// The purchase id of the subscription that need to renew
        /// </summary>
        [JsonProperty("purchaseId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PurchaseId { get; set; }

        /// <summary>
        /// value can be one of : Renew = 0, Reminder = 1, Downgrade = 2, RenewUnifiedTransaction =
        /// 3, RenewalReminder = 4, GiftCardReminder = 5, SubscriptionEnds = 6
        /// </summary>
        [JsonProperty("renewalType", NullValueHandling = NullValueHandling.Ignore)]
        public long? RenewalType { get; set; }

        [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }
}
