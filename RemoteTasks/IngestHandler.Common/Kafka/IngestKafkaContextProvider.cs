using EventBus.Abstraction;
using OTT.Lib.Kafka;

namespace IngestHandler.Common.Kafka
{
    public class EventBusKafkaContextProvider : IKafkaContextProvider
    {
        private readonly IEventContext _eventContext;

        public EventBusKafkaContextProvider(IEventContext eventContext)
        {
            _eventContext = eventContext;
        }

        public string GetRequestId()
        {
            return _eventContext.RequestId;
        }

        public long? GetPartnerId()
        {
            return _eventContext.GroupId;
        }

        public long? GetUserId()
        {
            return _eventContext.UserId;
        }
    }

    public class ManualKafkaContextProvider : IKafkaContextProvider, IEventContext
    {
        public string RequestId { get; set; }
        public long? PartnerId { get; set; }    
        public long? UserId { get; set; }

        public long? GroupId => PartnerId;

        public ManualKafkaContextProvider() { }
       
        public long? GetPartnerId() => PartnerId;
        public string GetRequestId() => RequestId;
        public long? GetUserId() => UserId;
    }
}
