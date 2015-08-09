using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PendingTransactionHandler
{
    [Serializable]
    public class PendingTransactionRequest
    {
        [JsonProperty("group_id")]
        public int GroupID
        {
            get;
            set;
        }

        [JsonProperty("pending_transaction_id")]
        public long PendingTransactionId
        {
            get;
            set;
        }

        [JsonProperty("number_of_retries")]
        public int NumberOfRetries
        {
            get;
            set;
        }

        [JsonProperty("billing_guid")]
        public string BillingGuide
        {
            get;
            set;
        }

        [JsonProperty("payment_gateway_transaction_id")]
        public long PaymengGatewayTransactionId
        {
            get;
            set;
        }

        [JsonProperty("site_guid")]
        public string SiteGuid
        {
            get;
            set;
        }

        [JsonProperty("household_id")]
        public long HouseholdId
        {
            get;
            set;
        }
    }
}
