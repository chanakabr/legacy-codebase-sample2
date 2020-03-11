using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledPurchaseHandler
{
    [Serializable]
    public class ScheduledPurchaseTaskRequest
    {

        [JsonProperty("group_id", Required = Required.Always)]
        public int GroupId
        {
            get;
            set;
        }

        [JsonProperty("siteguid", Required = Required.Always)]
        public string Siteguid
        {
            get;
            set;
        }

        [JsonProperty("household", Required = Required.Always)]
        public long Household
        {
            get;
            set;
        }

        [JsonProperty("price", Required = Required.Always)]
        public double Price
        {
            get;
            set;
        }

        [JsonProperty("currency", Required = Required.Always)]
        public string Currency
        {
            get;
            set;
        }

        [JsonProperty("content_id", Required = Required.Always)]
        public int ContentId
        {
            get;
            set;
        }

        [JsonProperty("product_id", Required = Required.Always)]
        public int ProductId
        {
            get;
            set;
        }

        [JsonProperty("transaction_type", Required = Required.Always)]
        public eTransactionType TransactionType
        {
            get;
            set;
        }

        [JsonProperty("coupon", Required = Required.Always)]
        public string Coupon
        {
            get;
            set;
        }

        [JsonProperty("userIp", Required = Required.Always)]
        public string UserIp
        {
            get;
            set;
        }

        [JsonProperty("device_name", Required = Required.Always)]
        public string DeviceName
        {
            get;
            set;
        }

        [JsonProperty("paymentGwId", Required = Required.Always)]
        public int PaymentGwId
        {
            get;
            set;
        }

        [JsonProperty("paymentMethodId", Required = Required.Always)]
        public int PaymentMethodId
        {
            get;
            set;
        }

        [JsonProperty("adapterData", Required = Required.Always)]
        public string adapterData
        {
            get;
            set;
        }

    }
}
