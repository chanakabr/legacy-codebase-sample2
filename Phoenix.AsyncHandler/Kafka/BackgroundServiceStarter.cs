using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OTT.Lib.Kafka;
using Phx.Lib.Log;

namespace Phoenix.AsyncHandler.Kafka
{
    public class BackgroundServiceStarter<THandler, T> : BackgroundService 
        where THandler : IHandler<T>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string _topic;
        private readonly IKafkaConsumer<string, T> _consumer;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        
        public BackgroundServiceStarter(IKafkaConsumerFactory consumerFactory, IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime hostApplicationLifetime, string kafkaGroupSuffix, string topic)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _hostApplicationLifetime = hostApplicationLifetime;
            _topic = topic;
            _consumer = consumerFactory.Get<string, T>($"{KafkaConfig.KafkaGroupId}-{kafkaGroupSuffix}", HandleWrapper);
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(new[] { _topic });
            return Task
                .Run(() => _consumer.Run(stoppingToken), stoppingToken)
                .ContinueWith(t => _hostApplicationLifetime.StopApplication(), stoppingToken);
        }
        
        private HandleResult HandleWrapper(ConsumeResult<string, T> consumeResult)
        {
            SetKLogger(consumeResult);
            
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                SetKafkaContext(consumeResult, serviceScope);
                var handler = serviceScope.ServiceProvider.GetRequiredService<THandler>();
                return handler.Handle(consumeResult);
            }
        }

        private static void SetKafkaContext(ConsumeResult<string, T> consumeResult, IServiceScope serviceScope)
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<IKafkaContextProvider>() as AsyncHandlerKafkaContextProvider;
            context?.Populate(consumeResult.TraceId, consumeResult.PartnerId, consumeResult.UserId);
        }

        private static void SetKLogger(ConsumeResult<string, T> consumeResult)
        {
            KLogger.SetRequestId(consumeResult.TraceId);
            if (consumeResult.PartnerId.HasValue) KLogger.SetGroupId(consumeResult.PartnerId.Value.ToString());
            KLogger.SetTopic(consumeResult.Result.Topic);
        }
    }
}
