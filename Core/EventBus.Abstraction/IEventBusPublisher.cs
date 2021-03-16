using System;
using System.Collections.Generic;

namespace EventBus.Abstraction
{
    public interface IEventBusPublisher
    {
        void Publish(ServiceEvent serviceEvent);

        void Publish(IEnumerable<ServiceEvent> serviceEvents);

        void PublishHeadersOnly(ServiceEvent serviceEvent, Dictionary<string, string> headersToAdd = null);
    }
}
