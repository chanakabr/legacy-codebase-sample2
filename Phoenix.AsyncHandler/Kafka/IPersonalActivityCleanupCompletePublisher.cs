using Phoenix.AsyncHandler.Pricing;
using Phoenix.Generated.Api.Events.Logical.PersonalActivityCleanupComplete;

namespace Phoenix.AsyncHandler.Kafka
{
    public interface IPersonalActivityCleanupCompletePublisher
    {
        void Publish(long partnerId, PersonalActivityCleanupStatus status, string description);

    }
}