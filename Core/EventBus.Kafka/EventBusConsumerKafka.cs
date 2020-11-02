using ConfigurationManager;
using Confluent.Kafka;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventBus.Kafka
{
    public class EventBusConsumerKafka : IEventBusConsumer, IDisposable
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private bool _Cancelled = false;
        private ConsumerBuilder<string, string> _Consumer = null;
        private List<string> _Topics = null;
        private Action<string, string> _OnConsume = null;
        private IConsumer<string, string> _ConsumerBuild = null;

        public EventBusConsumerKafka(string consumerGroupName, List<string> topics, Action<string, string> onConsume)
        {
            var kafkaConfig = new ConsumerConfig();
            kafkaConfig.BootstrapServers = ApplicationConfiguration.Current.KafkaClientConfiguration.BootstrapServers.Value;
            kafkaConfig.SocketTimeoutMs = ApplicationConfiguration.Current.KafkaClientConfiguration.SocketTimeoutMs.Value;

            kafkaConfig.GroupId = consumerGroupName;

            _Topics = topics;
            _OnConsume = onConsume;

            _Consumer = new ConsumerBuilder<string, string>(kafkaConfig);
        }

        public Task StartConsumerAsync(CancellationToken cancellationToken)
        {
            _ConsumerBuild = _Consumer.Build();
            _ConsumerBuild.Subscribe(_Topics);
            _Cancelled = false;

            while (!_Cancelled)
            {
                try
                {
                    // Poll for new messages / events. Blocks until a consume result is available or the operation has been cancelled.
                    var consumedMessage = _ConsumerBuild.Consume(cancellationToken);

                    var messageValue = consumedMessage.Message.Value;
                    var messageKey = consumedMessage.Message.Key;

                    var partition = consumedMessage.Partition.Value;
                    var topic = consumedMessage.Topic;

                    _Logger.Debug($"Consuming message. topic = {topic} partition = {partition} message = {messageValue} key = {messageKey}");
                    _OnConsume.Invoke(messageKey, messageValue);
                }
                catch (OperationCanceledException)
                {
                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    _ConsumerBuild.Close();
                    _Cancelled = true;
                }
                catch (Exception e)
                {
                    _Logger.Error($"Error when consuming message from kafka. ex = {e}", e);
                }
            }

            return Task.CompletedTask;
        }

        public Task StopConsumerAsync(CancellationToken cancellationToken)
        {
            _Cancelled = true;

            if (_ConsumerBuild != null)
            {
                _ConsumerBuild.Dispose();
                _ConsumerBuild = null;
            }

            return Task.CompletedTask;
        }


        public void Subscribe<T, TH>() where T : ServiceEvent where TH : IServiceEventHandler<T>
        {
            var eventType = typeof(T);
            var handlerType = typeof(TH);
            this.Subscribe(eventType, handlerType);
        }

        public void Unsubscribe<T, TH>() where TH : IServiceEventHandler<T> where T : ServiceEvent
        {
            var eventType = typeof(T);
            var handlerType = typeof(TH);
            this.Unsubscribe(eventType, handlerType);
        }

        public void Subscribe(Type eventType, Type handlerType)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Type eventType, Type handlerType)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _Cancelled = true;

            if (this._ConsumerBuild != null)
            {
                this._ConsumerBuild.Dispose();
                this._ConsumerBuild = null;
            }
        }
    }
}
