using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    public class CampaignTriggerEvent : ServiceEvent
    {
        public long CampaignId { get; set; }
        public int DomainId { get; set; }
        public CoreObject EventObject { get; set; }
    }
}