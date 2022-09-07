using EventBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OTT.Lib.Kafka;

namespace EventBus.Kafka
{
    public class EventBusConsumerKafka : IEventBusConsumer, IDisposable
    {
        private readonly IKafkaConsumer<string, string> _consumer;
        private readonly IEnumerable<string> _topics;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private bool _disposed;

        public delegate void OnConsumeAction(Confluent.Kafka.ConsumeResult<string, string> consumeResult);

        public delegate void OnBatchConsumeAction(List<Confluent.Kafka.ConsumeResult<string, string>> consumeResult);

        public EventBusConsumerKafka(
            string groupName,
            List<string> topics,
            int maxConsumeMessages,
            int maxConsumeWaitTimeMs,
            OnBatchConsumeAction onBatchConsume,
            IReadOnlyDictionary<string, string> additionalKafkaConfig,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _consumer = KafkaConsumerFactoryInstance.Instance.Acquire(additionalKafkaConfig)
                .Get<string, string>(
                    groupName,
                    maxConsumeMessages,
                    maxConsumeWaitTimeMs,
                    results =>
                    {
                        onBatchConsume(results.Results.Select(x => x.Result).ToList());

                        return new BatchHandleResult();
                    });
            _topics = topics;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public EventBusConsumerKafka(
            string groupName,
            List<string> topics,
            int maxConsumeMessages,
            int maxConsumeWaitTimeMs,
            OnBatchConsumeAction onBatchConsume,
            IHostApplicationLifetime hostApplicationLifetime) 
            : this(groupName, topics, maxConsumeMessages, maxConsumeWaitTimeMs, onBatchConsume, null, hostApplicationLifetime)
        {
        }

        public EventBusConsumerKafka(string groupName, List<string> topics, OnConsumeAction onSingleMessageConsume,
            IReadOnlyDictionary<string, string> additionalKafkaConfig, IHostApplicationLifetime hostApplicationLifetime)
        {
            _consumer = KafkaConsumerFactoryInstance.Instance.Acquire(additionalKafkaConfig)
                .Get<string, string>(
                    groupName,
                    result =>
                    {
                        onSingleMessageConsume(result.Result);

                        return new HandleResult();
                    });
            _topics = topics;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public EventBusConsumerKafka(string groupName, List<string> topics, OnConsumeAction onSingleMessageConsume,
            IHostApplicationLifetime hostApplicationLifetime) : this(groupName, topics, onSingleMessageConsume, null,
            hostApplicationLifetime)
        {
        }

        ~EventBusConsumerKafka()
        {
            Dispose(false);
        }

        public Task StartConsumerAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topics);
            Task
                .Run(() => _consumer.Run(cancellationToken), cancellationToken)
                .ContinueWith(t => _hostApplicationLifetime.StopApplication(), cancellationToken);
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
    }
}
