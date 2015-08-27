using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubscriptionRenewHandler
{
    [Serializable]
    public class SubscriptionRenewRequest
    {
        [JsonProperty("group_id")]
        public int GroupID
        {
            get;
            set;
        }

        [JsonProperty("billing_method")]
        public int BillingMethod
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

        [JsonProperty("price")]
        public double Price
        {
            get;
            set;
        }

        [JsonProperty("currency")]
        public string Currency
        {
            get;
            set;
        }

        [JsonProperty("subscription_code")]
        public string SubscriptionCode
        {
            get;
            set;
        }

        [JsonProperty("extra_params")]
        public string ExtraParamas
        {
            get;
            set;
        }

        [JsonProperty("purchase_id")]
        public int PurchaseId
        {
            get;
            set;
        }

        [JsonProperty("payment_number")]
        public int PaymentNumber
        {
            get;
            set;
        }

        [JsonProperty("number_of_payments")]
        public int NumberOfPayments
        {
            get;
            set;
        }

        [JsonProperty("total_number_of_payments")]
        public int TotalNumberOfPayments
        {
            get;
            set;
        }

        // End Date?

        // Purchased with preview module?

        [JsonProperty("billing_provider")]
        public int BillingProvider
        {
            get;
            set;
        }

        [JsonProperty("country_code")]
        public string CountryCode
        {
            get;
            set;
        }

        [JsonProperty("device_name")]
        public string DeviceName
        {
            get;
            set;
        }

        [JsonProperty("language_code")]
        public string LanguageCode
        {
            get;
            set;
        }
    }
}