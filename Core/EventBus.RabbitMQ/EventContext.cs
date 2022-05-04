using EventBus.Abstraction;

namespace EventBus.RabbitMQ
{
    internal sealed class EventContext : IEventContext
    {
        public string RequestId { get; private set; }
        
        public long? GroupId { get; private set; }
        
        public long? UserId { get; private set; }
        
        internal void PopulateFromServiceEvent(ServiceEvent serviceEvent)
        {
            RequestId = serviceEvent.RequestId;
            GroupId = serviceEvent.GroupId;
            UserId = serviceEvent.UserId;
        }
    }
}
