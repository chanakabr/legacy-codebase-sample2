using ApiObjects;
using EventBus.Abstraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class SubscriptionRenewRequest : DelayedServiceEvent
    {
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

        [JsonProperty("household_id")]
        public long HouseholdId
        {
            get;
            set;
        }

        [JsonProperty("process_id")]
        public long ProcessId
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"{{{nameof(BillingGuid)}={BillingGuid}, {nameof(SiteGuid)}={SiteGuid}, {nameof(PurchaseId)}={PurchaseId}, {nameof(EndDate)}={EndDate}, {nameof(Type)}={Type}, {nameof(HouseholdId)}={HouseholdId}, {nameof(ProcessId)}={ProcessId}, {nameof(ETA)}={ETA}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}