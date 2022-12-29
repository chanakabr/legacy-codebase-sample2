using Newtonsoft.Json;
using OTT.Lib.MongoDB;
using System;

namespace ApiObjects.Pricing
{
    [MongoDbIgnoreExternalElements]
    [Serializable]
    public class CampaignHouseholdUsages
    {
        [JsonProperty("campaignId", NullValueHandling = NullValueHandling.Ignore)]
        public long CampaignId { get; set; }

        [JsonProperty("householdId", NullValueHandling = NullValueHandling.Ignore)]
        public long HouseholdId { get; set; }

        [JsonProperty("usageCount", NullValueHandling = NullValueHandling.Ignore)]
        public int UsageCount { get; set; }

        [JsonProperty("expiration", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Expiration { get; set; }

        [JsonProperty("__updated", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime UpdateDate { get; set; }
    }
}
