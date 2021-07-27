using ApiObjects.Catalog;
using Newtonsoft.Json;
using System;

namespace ApiObjects.Notification
{
    public class TriggerCampaignEvent : CoreObject
    {
        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("campaignId")]
        public long CampaignId { get; set; }

        [JsonProperty("udid")]
        public string Udid { get; set; }

        [JsonProperty("domainId")]
        public long DomainId { get; set; }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        protected override bool DoInsert()
        {
            throw new NotImplementedException();
        }

        protected override bool DoUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
