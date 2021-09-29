using ConfigurationManager;
using Confluent.Kafka;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace EventBus.Kafka
{
    public class EventBusConsumerKafka : IEventBusConsumer, IDisposable
    {
        public delegate void OnConsumeAction(ConsumeResult<string, string> consumeResult);

        public delegate void OnBatchConsumeAction(List<ConsumeResult<string, string>> consumeResult);

        protected internal const string TRACE_ID_HEADER_NAME = "traceId";

        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private bool _cancelled = false;
        private ConsumerBuilder<string, string> _consumerBuilder = null;
        private List<string> _topics = null;
        private OnConsumeAction _onSingleMessageConsume = null;
        private OnBatchConsumeAction _onBatchConsume = null;
        private IConsumer<string, string> _consumer = null;
        private bool _shouldAutoCommit = false;

        private object _consumeBufferFlushLock = new object();
        private List<ConsumeResult<string, string>> _consumeBuffer = new List<ConsumeResult<string, string>>();
        private int _maxConsumeMessages;
        private int _maxConsumeWaitTimeMs;

        public EventBusConsumerKafka(string groupName, List<string> topics, int maxConsumeMessages, int maxConsumeWaitTimeMs, OnBatchConsumeAction onBatchConsume) : this(groupName, topics)
        {
            _maxConsumeMessages = maxConsumeMessages;
            _maxConsumeWaitTimeMs = maxConsumeWaitTimeMs;
            _onBatchConsume = onBatchConsume;

            // in case of batch consume this should always be false to avoid losing part of the batch
            _shouldAutoCommit = false;
        }

        public EventBusConsumerKafka(string groupName, List<string> topics, OnConsumeAction onSingleMessageConsume) : this(groupName, topics)
        {
            _onSingleMessageConsume = onSingleMessageConsume;
        }

        private EventBusConsumerKafka(string consumerGroupName, List<string> topics)
        {
            var kafkaConfig = new ConsumerConfig();
            kafkaConfig.GroupId = consumerGroupName;
            kafkaConfig.BootstrapServers = ApplicationConfiguration.Current.KafkaClientConfiguration.BootstrapServers.Value;
            kafkaConfig.SocketTimeoutMs = ApplicationConfiguration.Current.KafkaClientConfiguration.SocketTimeoutMs.Value;
            _shouldAutoCommit = ApplicationConfiguration.Current.KafkaClientConfiguration.ConsumerAutoCommit.Value;

            _topics = topics;
            _consumerBuilder = new ConsumerBuilder<string, string>(kafkaConfig);
        }

        public Task StartConsumerAsync(CancellationToken cancellationToken)
        {
            _consumerBuilder.SetErrorHandler(ConsumerErrorHandler);
            _consumerBuilder.SetLogHandler(ConsumeLogHandler);
            _consumer = _consumerBuilder.Build();
            _consumer.Subscribe(_topics);
            _cancelled = false;

            if (_onBatchConsume != null)
            {
                RunBatchConsumerLoop(cancellationToken);
            }
            else
            {
                RunSingleMessageConsumerLoop(cancellationToken);
            }

            return Task.CompletedTask;
        }

        private void RunBatchConsumerLoop(CancellationToken cancellationToken)
        {
            var consumeBufferFlushTimer = new System.Timers.Timer(_maxConsumeWaitTimeMs);
            consumeBufferFlushTimer.Elapsed += (sender, e) => { FlushConsumeBuffer(); };
            consumeBufferFlushTimer.Start();

            while (!_cancelled)
            {
                try
                {
                    // we update consumeBatchCount only inside a lock because of the flush timer
                    var consumeBatchCount = 0;
                    while (consumeBatchCount < _maxConsumeMessages)
                    {
                        // Poll for new messages / events. Blocks until a consume result is available or the operation has been cancelled.
                        var consumeResult = _consumer.Consume(cancellationToken);
                        lock (_consumeBufferFlushLock)
                        {
                            SetRequestId(consumeResult);
                            _logger.Debug($"Consuming message. topic = {consumeResult.Topic} partition = {consumeResult.Partition.Value} message = {consumeResult.Message.Value} key = {consumeResult.Message.Key}");
                            _consumeBuffer.Add(consumeResult);
                            consumeBatchCount = _consumeBuffer.Count;
                        }
                    }

                    try
                    {
                        // reset timer as we got to the size of batch and start it after batch is processed 
                        consumeBufferFlushTimer.Stop();
                        FlushConsumeBuffer();
                        consumeBufferFlushTimer.Start();
                    }
                    catch (OperationCanceledException)
                    {
                        _consumer.Close();
                        _cancelled = true;
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Error when invoking on consume method when consuming message from kafka. ex = {e}", e);
                    }
                    finally
                    {
                        if (!_cancelled && !_shouldAutoCommit)
                        {
                            _consumer.Commit();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _consumer.Close();
                    _cancelled = true;
                }
                catch (Exception e)
                {
                    _logger.Error($"Error when consuming message from kafka. ex = {e}", e);
                }
            }
        }

        private void FlushConsumeBuffer()
        {
            // locking here to avoid consuming while flushing due to consume timeout
            // interval passed
            List<ConsumeResult<string, string>> batchMessageResults;
            lock (_consumeBufferFlushLock)
            {
                if (_consumeBuffer.Count > 0)
                {
                    batchMessageResults = _consumeBuffer.ToList();
                    _consumeBuffer.Clear(); 
                    _onBatchConsume.Invoke(batchMessageResults);
                }
            }
        }

        private void RunSingleMessageConsumerLoop(CancellationToken cancellationToken)
        {
            while (!_cancelled)
            {
                try
                {
                    // Poll for new messages / events. Blocks until a consume result is available or the operation has been cancelled.
                    var consumeResult = _consumer.Consume(cancellationToken);

                    // try to get trace ID from header
                    SetRequestId(consumeResult);

                    var messageValue = consumeResult.Message.Value;
                    var messageKey = consumeResult.Message.Key;

                    var partition = consumeResult.Partition.Value;
                    var topic = consumeResult.Topic;

                    _logger.Debug($"Consuming message. topic = {topic} partition = {partition} message = {messageValue} key = {messageKey}");

                    try
                    {
                        _onSingleMessageConsume.Invoke(consumeResult);
                    }
                    catch (OperationCanceledException)
                    {
                        _consumer.Close();
                        _cancelled = true;
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Error when invoking on consume method when consuming message from kafka. ex = {e}", e);
                    }
                    finally
                    {
                        if (!_cancelled && !_shouldAutoCommit)
                        {
                            _consumer.Commit();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _consumer.Close();
                    _cancelled = true;
                }
                catch (Exception e)
                {
                    _logger.Error($"Error when consuming message from kafka. ex = {e}", e);
                }
            }
        }

        private void SetRequestId(ConsumeResult<string, string> consumedMessage)
        {
            try
            {
                byte[] traceIdHeader;
                if (consumedMessage.Message.Headers.TryGetLastBytes(TRACE_ID_HEADER_NAME, out traceIdHeader))
                {
                    var traceIdHeaderString = Encoding.Default.GetString(traceIdHeader);
                    KLogger.SetRequestId(traceIdHeaderString);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed getting request ID from message header. ex={ex}", ex);
            }
        }

        public Task StopConsumerAsync(CancellationToken cancellationToken)
        {
            _cancelled = true;

            if (_consumer != null)
            {
                _consumer.Dispose();
                _consumer = null;
            }

            return Task.CompletedTask;
        }

        private void ConsumeLogHandler(IConsumer<string, string> consumer, LogMessage msg)
        {
            switch (msg.Level)
            {
                case SyslogLevel.Emergency:
                case SyslogLevel.Alert:
                case SyslogLevel.Critical:
                case SyslogLevel.Error:
                    _logger.Error(msg.Message);
                    break;
                case SyslogLevel.Warning:
                    _logger.Warn(msg.Message);
                    break;
                case SyslogLevel.Notice:
                case SyslogLevel.Info:
                    _logger.Info(msg.Message);
                    break;
                case SyslogLevel.Debug:
                    _logger.Debug(msg.Message);
                    break;
                default:
                    break;
            }
        }

        private void ConsumerErrorHandler(IConsumer<string, string> consumer, Error err)
        {
            _logger.Error($"Error while trying to consume: [{err}]");
        }

        public void Dispose()
        {
            _cancelled = true;

            if (this._consumer != null)
            {
                this._consumer.Dispose();
                this._consumer = null;
            }
        }
    }
}