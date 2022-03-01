using Phx.Lib.Appconfig;
using EventBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;

namespace EventBus.Kafka
{
    public class EventBusConsumerKafka : IEventBusConsumer, IDisposable
    {
        private static readonly Lazy<IKafkaConsumerFactory> ConsumerFactoryLazy = new Lazy<IKafkaConsumerFactory>(InitializeConsumerFactory, LazyThreadSafetyMode.PublicationOnly);
        private readonly IKafkaConsumer<string, string> _consumer;
        private readonly IEnumerable<string> _topics;
        private bool _disposed;

        public delegate void OnConsumeAction(Confluent.Kafka.ConsumeResult<string, string> consumeResult);

        public delegate void OnBatchConsumeAction(List<Confluent.Kafka.ConsumeResult<string, string>> consumeResult);

        public EventBusConsumerKafka(string groupName, List<string> topics, int maxConsumeMessages, int maxConsumeWaitTimeMs, OnBatchConsumeAction onBatchConsume)
        {
            _consumer = ConsumerFactoryLazy.Value.Get<string, string>(
                groupName,
                maxConsumeMessages,
                maxConsumeWaitTimeMs,
                results =>
                {
                    onBatchConsume(results.Results.Select(x => x.Result).ToList());

                    return new BatchHandleResult();
                });
            _topics = topics;
        }

        public EventBusConsumerKafka(string groupName, List<string> topics, OnConsumeAction onSingleMessageConsume)
        {
            _consumer = ConsumerFactoryLazy.Value.Get<string, string>(
                groupName,
                result =>
                {
                    onSingleMessageConsume(result.Result);

                    return new HandleResult();
                });
            _topics = topics;
        }

        ~EventBusConsumerKafka()
        {
            Dispose(false);
        }

        public Task StartConsumerAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topics);
            _consumer.Run(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopConsumerAsync(CancellationToken cancellationToken)
        {
            _consumer.Stop(cancellationToken);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _consumer.Dispose();
                }

                _disposed = true;
            }
        }

        private static IKafkaConsumerFactory InitializeConsumerFactory()
        {
            var tcmConfig = ApplicationConfiguration.Current.KafkaClientConfiguration;
            var kafkaConfig = new Dictionary<string, string>
            {
                { KafkaConfigKeys.BootstrapServers, tcmConfig.BootstrapServers.Value },
                { KafkaConfigKeys.SocketTimeoutMs, tcmConfig.SocketTimeoutMs.Value.ToString() }
            };

            var loggerFactory = LoggerFactory.Create(builder => builder.AddLog4Net());

            var consumerFactory = new KafkaConsumerFactory(kafkaConfig, loggerFactory);

            return consumerFactory;
        }
    }
}