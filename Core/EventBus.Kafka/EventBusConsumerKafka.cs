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
        protected internal const string TRACE_ID_HEADER_NAME = "traceId";

        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private bool _Cancelled = false;
        private ConsumerBuilder<string, string> _Consumer = null;
        private List<string> _Topics = null;
        private Action<string, string> _OnConsume = null;
        private IConsumer<string, string> _ConsumerBuild = null;
        private bool _ShouldAutoCommit = false;

        public EventBusConsumerKafka(string consumerGroupName, List<string> topics, Action<string, string> onConsume)
        {
            var kafkaConfig = new ConsumerConfig();
            kafkaConfig.GroupId = consumerGroupName;
            kafkaConfig.BootstrapServers = ApplicationConfiguration.Current.KafkaClientConfiguration.BootstrapServers.Value;
            kafkaConfig.SocketTimeoutMs = ApplicationConfiguration.Current.KafkaClientConfiguration.SocketTimeoutMs.Value;
            _ShouldAutoCommit = ApplicationConfiguration.Current.KafkaClientConfiguration.ConsumerAutoCommit.Value;
            kafkaConfig.EnableAutoCommit = _ShouldAutoCommit;

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


                    byte[] traceIdHeader;
                    if (consumedMessage.Message.Headers.TryGetLastBytes(TRACE_ID_HEADER_NAME, out traceIdHeader))
                    {
                        var traceIdHeaderString = Encoding.Default.GetString(traceIdHeader);
                        KLogger.SetRequestId(traceIdHeaderString);
                    }

                    var messageValue = consumedMessage.Message.Value;
                    var messageKey = consumedMessage.Message.Key;

                    var partition = consumedMessage.Partition.Value;
                    var topic = consumedMessage.Topic;

                    _Logger.Debug($"Consuming message. topic = {topic} partition = {partition} message = {messageValue} key = {messageKey}");

                    try
                    {
                        _OnConsume.Invoke(messageKey, messageValue);
                    }
                    catch (OperationCanceledException)
                    {
                        _ConsumerBuild.Close();
                        _Cancelled = true;
                    }
                    catch (Exception e)
                    {
                        _Logger.Error($"Error when invoking on consume method when consuming message from kafka. ex = {e}", e);
                    }
                    finally
                    {
                        if (!_Cancelled && !_ShouldAutoCommit)
                        {
                            _ConsumerBuild.Commit();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
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
