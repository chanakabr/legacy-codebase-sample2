using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConfigurationManager;
using EventBus.Abstraction;
using KLogMonitor;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ
{
    public class EventBusConsumerRabbitMQ : IEventBusConsumer, IDisposable
    {
        private readonly ConcurrentDictionary<string, HashSet<SubscriptionInfo>> _Handlers;
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly IServiceProvider _ServiceProvide;
        private readonly IRabbitMQPersistentConnection _PersistentConnection;
        private readonly string _ExchangeName;
        private readonly int _ConcurrentConsumers;

        private List<IModel> _ConsumerChannels;
        private string _QueueName;
        private bool _Disposed;

        public static EventBusConsumerRabbitMQ GetInstanceUsingTCMConfiguration(IServiceProvider serviceProvide, IRabbitMQPersistentConnection persistentConnection, string queueName, int concurrentConsumers)
        {
            var eventBusConsumer = new EventBusConsumerRabbitMQ(
                serviceProvide,
                persistentConnection,
                ApplicationConfiguration.RabbitConfiguration.EventBus.Exchange.Value,
                queueName,
                concurrentConsumers);

            return eventBusConsumer;
        }

        private EventBusConsumerRabbitMQ(
            IServiceProvider serviceProvide,
            IRabbitMQPersistentConnection persistentConnection,
            string exchangeName = "kaltura_event_bus",
            string queueName = null,
            int concurrentConsumers = 4)
        {
            _Handlers = new ConcurrentDictionary<string, HashSet<SubscriptionInfo>>();
            _ServiceProvide = serviceProvide;
            _PersistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _ExchangeName = exchangeName;
            _QueueName = queueName;
            _ConcurrentConsumers = concurrentConsumers;
        }

        public Task StartConsumerAsync(CancellationToken cancellationToken)
        {
            _Logger.Info("Starting kaltura event-bus consumer...");
            for (var i = 0; i < _ConcurrentConsumers; i++)
            {
                _Logger.Info($"Connecting consumer [{i + 1}/{_ConcurrentConsumers}]");
                ConnectConsumer();
            }

            _Logger.Info("Listening to following events...");
            foreach (var handler in _Handlers)
            {
                var handlersStr = string.Join(",", handler.Value);
                _Logger.Info($"event:[{handler.Key}], handlers:[{handlersStr}]");
            }
            return Task.CompletedTask;
        }

        public Task StopConsumerAsync(CancellationToken cancellationToken)
        {
            _Logger.Info("Stopping kaltura event-bus consumer, disposing consumers...");
            this.Dispose();
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
            var eventName = ServiceEvent.GetEventName(eventType);
            var subscriptionInfo = new SubscriptionInfo(eventType, handlerType);
            _Logger.Debug($"Subscribing [{subscriptionInfo}]");

            if (!_Handlers.ContainsKey(eventName))
            {
                _Handlers[eventName] = new HashSet<SubscriptionInfo>();
                // If we are already connected and someone asked to dynamically subscribe we need to add a new binding
                // otherwise the bindings for all subscriptions during configuration will be done in StartAsync
                if (_ConsumerChannels != null) { InitializeNewEventBinding(eventName); }
            }

            _Handlers[eventName].Add(subscriptionInfo);
        }

        public void Unsubscribe(Type eventType, Type handlerType)
        {
            var eventName = ServiceEvent.GetEventName(eventType);
            var subscriptionInfo = new SubscriptionInfo(eventType, handlerType);
            if (_Handlers.TryGetValue(eventName, out var handlersForEvent))
            {
                TryRemoveSubscription(subscriptionInfo, handlersForEvent, eventName);
            }
            else
            {
                _Logger.Warn($"Unable to unsubscribe subscription:[{subscriptionInfo}], there are no handlers for that event");
            }

            if (!_Handlers.Any())
            {
                _Logger.Info($"Handlers list is empty, closing connection");
                _QueueName = string.Empty;
                _ConsumerChannels.ForEach(c => c.Close());
            }
        }

        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;

            try
            {
                _Logger.Debug($"Disposing [{_ConsumerChannels?.Count}] consumers...");
                foreach (var consumer in _ConsumerChannels)
                {
                    _Logger.Debug($"Disposing: [{consumer.ToString()}]");
                    consumer.Dispose();
                }

                _Logger.Debug($"Disposing Connection...");
                _PersistentConnection.Dispose();
            }
            catch (Exception e)
            {
                _Logger.Error("Error while disposing consumer ", e);
            }


        }

        private void ConnectConsumer()
        {
            _Logger.Info($"Creating consumer channel");
            _ConsumerChannels = _ConsumerChannels ?? new List<IModel>();
            var currentConsumer = _PersistentConnection.CreateModel();
            _ConsumerChannels.Add(currentConsumer);


            // TODO: Consider making exchangeType configurable
            const string exchangeType = "direct";
            const bool isExchangeDurable = true;
            _Logger.Info($"Declaring exchangeName:[{_ExchangeName}], exchangeType:[{exchangeType}], isExchangeDurable:[{isExchangeDurable}]");
            currentConsumer.ExchangeDeclare(_ExchangeName, exchangeType, isExchangeDurable);

            const bool isQueueDurable = true;
            const bool isQueueExclusive = false;
            const bool autoDelete = false;
            _Logger.Info($"Declaring queue:[{_QueueName}], isQueueDurable:[{isQueueDurable}], isQueueExclusive:[{isQueueExclusive}]");
            currentConsumer.QueueDeclare(_QueueName, isQueueDurable, isQueueExclusive, autoDelete);

            foreach (var eventName in _Handlers.Keys)
            {
                InitializeNewEventBinding(eventName);
            }

            _Logger.Info($"Configuring consumer...");
            var consumer = new AsyncEventingBasicConsumer(currentConsumer);
            consumer.Received += ConsumerOnReceived;

            const bool isConsumerAutoAck = false;
            _Logger.Info($"Starting to consume events isConsumerAutoAck:[{isConsumerAutoAck}]");
            currentConsumer.BasicConsume(_QueueName, isConsumerAutoAck, consumer);
            currentConsumer.BasicQos(0, 1, false);
            currentConsumer.CallbackException += ConsumerOnCallbackException;

        }

        private void TryRemoveSubscription(SubscriptionInfo subscription, HashSet<SubscriptionInfo> handlersForEvent, string eventName)
        {
            if (handlersForEvent.Remove(subscription))
            {
                _Logger.Info($"Successfully unsubscribe subscription:[{subscription}].");
                using (var channel = _PersistentConnection.CreateModel())
                {
                    channel.QueueUnbind(_QueueName, _ExchangeName, eventName);
                }
            }
            else
            {
                _Logger.Warn($"Unable to unsubscribe subscription:[{subscription}], handler is not subscribe  for that event");
            }
        }

        private void ConsumerOnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            ((IModel)sender).Dispose();
            _Logger.Error("Consumer encountered an error. ", e.Exception);
            _Logger.Info($"Connection was closed due to an error, attempting to reconnect...");
            ConnectConsumer();
        }

        private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var consumer = (AsyncEventingBasicConsumer)sender;
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body);
            _Logger.Debug($"Channel:[{consumer.Model.ChannelNumber}] ConsumerTag:[{consumer.ConsumerTag}] received event:[{eventName}], message:[{message}]");

            try
            {
                // calling Task.Yield() to make sure the processing of the event is really done in a separate thread
                // by returning control to the calling thread
                await Task.Yield();
                if (_Handlers.ContainsKey(eventName))
                {
                    await ProcessEvent(eventName, message);
                }
                else
                {
                    // This mimics Celery behaviour when the task is not found.
                    _Logger.Info($"No handlers for event:[{eventName}], returning Nack.");
                    consumer.Model.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                }

                consumer.Model.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception e)
            {
                _Logger.Error($"Error during invocation of ProcessEvent([{eventName}],[{message}]).", e);
                consumer.Model.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _Logger.Debug($"ProcessEvent > eventName:[{eventName}] with message:[{message}]");
            var subscriptions = _Handlers[eventName];

            _Logger.Debug($"ProcessEvent > Found following subscriptions: [{string.Join(",", subscriptions)}]");
            using (var scope = _ServiceProvide.CreateScope())
            {
                foreach (var subscription in subscriptions)
                {

                    var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                    if (handler == null)
                    {
                        _Logger.Error($"could not find handler [{subscription.HandlerType.Name}], did you forget to inject it as a service?");
                        return;
                    }

                    // TODO: try to find a way to cast service handler of <T> to its type so that we can avoid reflection
                    var handleMethod = subscription.HandlerType.GetMethod("Handle");
                    if (handleMethod == null)
                    {
                        _Logger.Error($"could not find Handle(...) method in handler [{subscription.HandlerType.Name}]?");
                        return;
                    }

                    var eventType = subscription.EventType;
                    var serviceEvent = JsonConvert.DeserializeObject(message, eventType);
                    SetLoggingContext(serviceEvent);
                    await (Task)handleMethod.Invoke(handler, new[] { serviceEvent });
                }
            }

        }

        private void SetLoggingContext(object serviceEvent)
        {
            // TODO: Arthur, Think of the bigger context picture and how should it be managed, for logging and for in memepry data store
            var eventData = (ServiceEvent)serviceEvent;
            KLogger.LogContextData[KLogMonitor.Constants.USER_ID] = eventData.UserId;
            KLogger.LogContextData[KLogMonitor.Constants.GROUP_ID] = eventData.GroupId;
            KLogger.LogContextData[KLogMonitor.Constants.REQUEST_ID_KEY] = eventData.RequestId;
        }

        private void InitializeNewEventBinding(string eventName)
        {
            _Logger.Info($"Initializing new event binding on queue:[{_QueueName}], exchange:{_ExchangeName}, routingKey:[{eventName}]");
            using (var channel = _PersistentConnection.CreateModel())
            {
                channel.QueueBind(_QueueName, _ExchangeName, eventName);
            }
        }
    }
}
