using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OTT.Lib.Kafka;
using Phx.Lib.Log;

namespace Phoenix.AsyncHandler.Kafka
{
    /// <summary>
    /// Long-running handler, which consumes kafka messages 
    /// </summary>
    /// <typeparam name="T">type of kafka message</typeparam>
    public abstract class Handler<T> : BackgroundService
    {
        private readonly IKafkaConsumer<string, T> _consumer;

        protected Handler(IKafkaConsumerFactory consumerFactory, string kafkaGroupSuffix)
        {
            var kafkaGroup = $"{KafkaConfig.KafkaGroupId}-{kafkaGroupSuffix}";
            _consumer = consumerFactory.Get<string, T>(kafkaGroup, HandleWrapper);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(new[] { Topic() });
            return Task.Run(() => _consumer.Run(stoppingToken), stoppingToken);
        }

        /// <summary>
        /// Kafka topic, which should be listened
        /// </summary>
        /// <returns></returns>
        protected abstract string Topic();

        /// <summary>
        /// Function to process a message
        /// </summary>
        /// <param name="consumeResult"></param>
        /// <returns></returns>
        protected abstract HandleResult Handle(ConsumeResult<string, T> consumeResult);
        
        private HandleResult HandleWrapper(ConsumeResult<string, T> consumeResult)
        {
            KLogger.SetRequestId(consumeResult.TraceId);
            if (consumeResult.PartnerId.HasValue) KLogger.SetGroupId(consumeResult.PartnerId.Value.ToString());
            KLogger.SetTopic(consumeResult.Result.Topic);
            
            return Handle(consumeResult);
        }
    }
}
