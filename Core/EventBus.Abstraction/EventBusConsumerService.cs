using Phx.Lib.Log;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EventBus.Abstraction
{
    public class EventBusConsumerService : IHostedService
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly IEventBusConsumer _EventBus;

        public EventBusConsumerService(IEventBusConsumer eventBus)
        {
            _EventBus = eventBus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _Logger.Info($"Consuming in background.");
            _EventBus.StartConsumerAsync(cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _Logger.Info($"Stopping background consumer.");
            _EventBus.StopConsumerAsync(cancellationToken);
            return Task.CompletedTask;
        }
    }
}
