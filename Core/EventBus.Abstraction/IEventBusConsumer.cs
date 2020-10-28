using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventBus.Abstraction
{
    public interface IEventBusConsumer
    {
        void Subscribe<T, TH>() where T : ServiceEvent where TH : IServiceEventHandler<T>;
        void Subscribe(Type eventType, Type handlerType);
        
        
        void Unsubscribe<T, TH>() where TH : IServiceEventHandler<T> where T : ServiceEvent;
        void Unsubscribe(Type eventType, Type handlerType);

        Task StartConsumerAsync(CancellationToken cancellationToken);
        Task StopConsumerAsync(CancellationToken cancellationToken);
    }
}
