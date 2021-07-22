using System;
using EventBus.Abstraction;

namespace Core.Api.Modules
{
    [Serializable]
    public class CampaignTriggerEvent : ServiceEvent
    {
        public long DomainId { get; set; }
        public int ApiService { get; set; }
        public int ApiAction { get; set; }
        public Core.Users.DomainDevice EventObject { get; set; }
    }
}
