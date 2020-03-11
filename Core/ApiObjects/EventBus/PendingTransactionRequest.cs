using EventBus.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class PendingTransactionRequest : ServiceEvent
    {
        [JsonProperty("payment_gateway_pending_Id")]
        public long PaymentGatewayPendingId { get; set; }

        [JsonProperty("number_of_retries")]
        public int NumberOfRetries { get; set; }

        [JsonProperty("billing_guid")]
        public string BillingGuide { get; set; }

        [JsonProperty("payment_gateway_transaction_id")]
        public long PaymentGatewayTransactionId { get; set; }

        [JsonProperty("site_guid")]
        public string SiteGuid { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("product_type")]
        public int ProductType { get; set; }

        public override string ToString()
        {
            return $"[PaymentGatewayPendingId:{PaymentGatewayPendingId},NumberOfRetries:{NumberOfRetries},BillingGuide:{BillingGuide},PaymentGatewayTransactionId:{PaymentGatewayTransactionId},SiteGuid:{SiteGuid},ProductId:{ProductId},ProductType:{ProductType}]";
        }
    }
}
