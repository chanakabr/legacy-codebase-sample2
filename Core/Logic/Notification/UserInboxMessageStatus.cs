using Newtonsoft.Json;
using OTT.Lib.MongoDB;
using System;

namespace Core.Notification
{
    [MongoDbIgnoreExternalElements]
    [Serializable]
    public class UserInboxMessageStatus
    {
        [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
        public int UserId { get; set; }

        [JsonProperty("messageId", NullValueHandling = NullValueHandling.Ignore)]
        public string MessageId { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("expiration", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime Expiration { get; set; }

        [JsonProperty("__updated", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime UpdateDate { get; set; }
    }
}
