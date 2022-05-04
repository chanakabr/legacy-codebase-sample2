using EventBus.Abstraction;
using FeatureFlag;

namespace IngestHandler.Common.Infrastructure
{
    public sealed class FeatureFlagIngestContext : IFeatureFlagContext
    {
        private readonly IEventContext _eventContext;

        public FeatureFlagIngestContext(IEventContext eventContext)
        {
            _eventContext = eventContext;
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
}
