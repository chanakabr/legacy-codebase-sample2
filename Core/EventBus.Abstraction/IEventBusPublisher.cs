using System;

namespace EventBus.Abstraction
{
    public interface IEventBusPublisher
    {
        void Publish(ServiceEvent serviceEvent);
    }
}
