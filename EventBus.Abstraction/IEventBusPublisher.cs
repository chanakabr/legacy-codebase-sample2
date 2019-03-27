using System;

namespace EventBus.Abstraction
{
    public interface IEventBusPublisher : IDisposable
    {
        void Publish(ServiceEvent serviceEvent);
    }
}
