using System.Collections.Generic;
using System;

namespace ApiObjects.Notification
{
    public class InboxMessage
    {
        public string Id { get; set; }
        public long UserId { get; set; }
        public string Message { get; set; }
        public eMessageCategory Category { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public long CreatedAtSec { get; set; }
        public long UpdatedAtSec { get; set; }
        public eMessageState State { get; set; }
        public long? CampaignId { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    public class CampaignInboxMessageMap
    {
        //Key: CampaignId, Value: InboxMessageTTL
        public Dictionary<long, CampaignMessageDetails> Campaigns { get; set; }

        public CampaignInboxMessageMap()
        {
            this.Campaigns = new Dictionary<long, CampaignMessageDetails>();
        }
    }

    public class CampaignMessageDetails
    {
        public string MessageId { get; set; }
        public long ExpiredAt { get; set; }
        public eCampaignType Type { get; set; }

        /// <summary>
        /// SubscriptionId to CreateDate of the use for SubscriptionId
        /// </summary>
        public Dictionary<long, long> SubscriptionUses { get; set; }

        public List<string> Devices { get; set; }

        public CampaignMessageDetails()
        {
            this.SubscriptionUses = new Dictionary<long, long>();
            this.Devices = new List<string>();
        }
    }

    public class DeviceTriggerCampaignsUses
    {
        /// <summary>
        /// campaignId to usage date
        /// </summary>
        public Dictionary<long, long> Uses { get; set; }
        public string Udid { get; set; }

        public DeviceTriggerCampaignsUses()
        {
            this.Uses = new Dictionary<long, long>();
        }
    }
}