using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    public class CampaignTriggerEvent : ServiceEvent
    {
        public long DomainId { get; set; }
        public int ApiService { get; set; }
        public int ApiAction { get; set; }
        public CoreObject EventObject { get; set; }
    }
}