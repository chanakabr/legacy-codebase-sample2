using System.Collections.Generic;

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
        public long? ProductId { get; set; }
        public eTransactionType? ProductType { get; set; }
        public long? CreateDate { get; set; }

    }
}