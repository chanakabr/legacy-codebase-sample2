using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    public class CampaignUserEvent : ServiceEvent
    {
        public long CampaignId { get; set; }
        public int DomainId { get; set; }
        public CoreObject EventObject { get; set; }
    }
}