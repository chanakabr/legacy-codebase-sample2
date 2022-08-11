using EventBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OTT.Lib.Kafka;

namespace EventBus.Kafka
{
    public class EventBusConsumerKafka : IEventBusConsumer, IDisposable
    {
        private readonly IKafkaConsumer<string, string> _consumer;
        private readonly IEnumerable<string> _topics;
        private bool _disposed;

        public delegate void OnConsumeAction(Confluent.Kafka.ConsumeResult<string, string> consumeResult);

        public delegate void OnBatchConsumeAction(List<Confluent.Kafka.ConsumeResult<string, string>> consumeResult);

        public EventBusConsumerKafka(
            string groupName,
            List<string> topics,
            int maxConsumeMessages,
            int maxConsumeWaitTimeMs,
            OnBatchConsumeAction onBatchConsume,
            IReadOnlyDictionary<string, string> additionalKafkaConfig)
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
        }

        public EventBusConsumerKafka(
            string groupName,
            List<string> topics,
            int maxConsumeMessages,
            int maxConsumeWaitTimeMs,
            OnBatchConsumeAction onBatchConsume) : this(groupName, topics, maxConsumeMessages, maxConsumeWaitTimeMs, onBatchConsume, null)
        {
        }

        public EventBusConsumerKafka(string groupName, List<string> topics, OnConsumeAction onSingleMessageConsume, IReadOnlyDictionary<string, string> additionalKafkaConfig)
        {
            KafkaConsumerFactoryInstance.Instance.Acquire(additionalKafkaConfig)
                .Get<string, string>(
                    groupName,
                    result =>
                    {
                        onSingleMessageConsume(result.Result);

                        return new HandleResult();
                    });
            _topics = topics;
        }

        public EventBusConsumerKafka(string groupName, List<string> topics, OnConsumeAction onSingleMessageConsume) : this(groupName, topics, onSingleMessageConsume, null)
        {
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
    }
}
