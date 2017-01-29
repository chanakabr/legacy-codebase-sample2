using ApiObjects;
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

        [JsonProperty("billing_guid")]
        public string BillingGuid
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

        [JsonProperty("purchase_id")]
        public long PurchaseId
        {
            get;
            set;
        }

        [JsonProperty("end_date")]
        public long EndDate
        {
            get;
            set;
        }

        [JsonProperty("task_type")]
        public eSubscriptionRenewRequestType? Type
        {
            get;
            set;
        }
    }
}