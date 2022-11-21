using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Phx.Lib.Appconfig;
using EventBus.Abstraction;
using Phx.Lib.Log;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.Http;

namespace EventBus.RabbitMQ
{
    internal class DummyHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; }
    }

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
        private List<int> _PartnerIds;

        public static EventBusConsumerRabbitMQ GetInstanceUsingTCMConfiguration(
            IServiceProvider serviceProvide,
            IRabbitMQPersistentConnection persistentConnection,
            string queueName,
            int concurrentConsumers,
            IEnumerable<int> partnerIds)
        {
            var eventBusConsumer = new EventBusConsumerRabbitMQ(
                serviceProvide,
                persistentConnection,
                ApplicationConfiguration.Current.RabbitConfiguration.EventBus.Exchange.Value,
                queueName,
                concurrentConsumers,
                partnerIds);

            return eventBusConsumer;
        }

        private EventBusConsumerRabbitMQ(
            IServiceProvider serviceProvide,
            IRabbitMQPersistentConnection persistentConnection,
            string exchangeName = "kaltura_event_bus",
            string queueName = null,
            int concurrentConsumers = 4,
            IEnumerable<int> partnerIds = null)
        {
            _Handlers = new ConcurrentDictionary<string, HashSet<SubscriptionInfo>>();
            _ServiceProvide = serviceProvide;
            _PersistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _ExchangeName = exchangeName;
            _QueueName = queueName;
            _ConcurrentConsumers = concurrentConsumers;
            _PartnerIds = partnerIds?.ToList();
        }

        public IEnumerable<int> GetDedicatedConsumerPartnerIds()
        {
            return _PartnerIds;
        }

        public void AddNewPartnerDedicatedConsumer(int partnerId) 
        {
            _PartnerIds.Add(partnerId);
            for (var i = 0; i < _ConcurrentConsumers; i++)
            {
                _Logger.Info($"Connecting new partner:[{partnerId}] consumer:[{i + 1}/{_ConcurrentConsumers}]");
                ConnectConsumer(partnerId);
            }
        }

        public Task StartConsumerAsync(CancellationToken cancellationToken)
        {
            _Logger.Info("Starting kaltura event-bus consumer...");
            if (_PartnerIds != null)
            {
                foreach (var partnerId in _PartnerIds)
                {
                    _Logger.Info($"Connecting consumer for partner [{partnerId}]");

                    for (var i = 0; i < _ConcurrentConsumers; i++)
                    {
                        _Logger.Info($"Connecting partner:[{partnerId}] consumer:[{i + 1}/{_ConcurrentConsumers}]");
                        ConnectConsumer(partnerId);
                    }
                }
            }
            else
            {
                for (var i = 0; i < _ConcurrentConsumers; i++)
                {
                    _Logger.Info($"Connecting consumer [{i + 1}/{_ConcurrentConsumers}]");
                    ConnectConsumer();
                }
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
            var routingKey = ServiceEvent.GetEventName(eventType);
            var subscriptionInfo = new SubscriptionInfo(eventType, handlerType);
            _Logger.Debug($"Subscribing [{subscriptionInfo}]");

            if (!_Handlers.ContainsKey(routingKey))
            {
                _Handlers[routingKey] = new HashSet<SubscriptionInfo>();
            }

            _Handlers[routingKey].Add(subscriptionInfo);
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


        private void ConnectConsumer(int? partnerId = null)
        {
            _Logger.Info($"Creating consumer channel for partner:[{partnerId}]");
            _ConsumerChannels = _ConsumerChannels ?? new List<IModel>();
            var currentConsumer = _PersistentConnection.CreateModel();
            _ConsumerChannels.Add(currentConsumer);

            const string exchangeType = "direct";
            const bool isExchangeDurable = true;
            _Logger.Info($"Declaring exchangeName:[{_ExchangeName}], exchangeType:[{exchangeType}], isExchangeDurable:[{isExchangeDurable}]");
            currentConsumer.ExchangeDeclare(_ExchangeName, exchangeType, isExchangeDurable);

            const bool isQueueDurable = true;
            const bool isQueueExclusive = false;
            const bool autoDelete = false;

            var partnerIdTag = partnerId.HasValue ? $"{partnerId}_" : "";
            var dedicatedQueueName = $"{partnerIdTag}{_QueueName}";
            _Logger.Info($"Declaring queue:[{dedicatedQueueName}], isQueueDurable:[{isQueueDurable}], isQueueExclusive:[{isQueueExclusive}]");
            currentConsumer.QueueDeclare(dedicatedQueueName, isQueueDurable, isQueueExclusive, autoDelete);

            foreach (var eventName in _Handlers.Keys)
            {
                var routingKey = ServiceEvent.GetDedicatedPartnerRoutingKey(partnerId, eventName);
                InitializeNewEventBinding(dedicatedQueueName, routingKey);
            }

            _Logger.Info($"Configuring consumer...");
            var consumer = new AsyncEventingBasicConsumer(currentConsumer);
            consumer.Received += ConsumerOnReceived;

            const bool isConsumerAutoAck = false;
            _Logger.Info($"Starting to consume events isConsumerAutoAck:[{isConsumerAutoAck}]");
            currentConsumer.BasicConsume(dedicatedQueueName, isConsumerAutoAck, consumer);
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
            ((IModel) sender).Dispose();
            _Logger.Error("Consumer encountered an error. ", e.Exception);
            _Logger.Info($"Connection was closed due to an error, attempting to reconnect...");
            ConnectConsumer();
        }

        private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var consumer = (AsyncEventingBasicConsumer) sender;
            var routingKey = eventArgs.RoutingKey;
            var headers = eventArgs.BasicProperties.Headers;

            // By default the event name is the routing key with which it was sent.
            // In case of a dedicated consumer, the event name and the routing key does not match
            // because of the partnerId prefix added to the routing key.
            // In this case an additional header will be sent representing the eventName
            var eventName = routingKey;
            if (headers.TryGetValue(EventBusPublisherRabbitMQ.HEADER_KEY_EVENT_BASE_NAME, out var eventNameBytes))
            {
                eventName = Encoding.UTF8.GetString((byte[]) eventNameBytes);
            }


            _Logger.Debug($"Channel:[{consumer.Model.ChannelNumber}] ConsumerTag:[{consumer.ConsumerTag}] received event:[{eventName}]], on routing key:[{routingKey}]");

            try
            {
                // calling Task.Yield() to make sure the processing of the event is really done in a separate thread
                // by returning control to the calling thread
                await Task.Yield();
                if (_Handlers.ContainsKey(eventName))
                {
                    await ProcessEvent(eventName, eventArgs.Body, eventArgs.BasicProperties.Headers);
                }
                else
                {
                    // This mimics Celery behaviour when the task is not found.
                    _Logger.Info($"No handlers for event:[{eventName}], returning Nack.");
                    consumer.Model.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                }

                consumer.Model.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (RetryableErrorException retryableErrorException)
            {
                _Logger.Error($"Error during invocation of ProcessEvent([{eventName}]).", retryableErrorException);
                consumer.Model.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true); // requeue true !
            }
            catch (Exception e)
            {
                _Logger.Error($"Error during invocation of ProcessEvent([{eventName}]).", e);
                consumer.Model.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        }

        private async Task ProcessEvent(string eventName, byte[] message, IDictionary<string, object> headers)
        {
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

                    Type eventType = subscription.EventType;
                    var serviceEvent = RabbitMqSerializationsHelper.Deserialize(message, eventType);


                    SetLoggingContext(serviceEvent);
                    SetEventContext(scope, serviceEvent);

                    #if !NETFRAMEWORK
                    // if we use net core we configure the httpContextAccessor with dummy context for layred cache to have request cache
                    var dummyCtxAccessor = new DummyHttpContextAccessor { HttpContext = new DefaultHttpContext() };
                    System.Web.HttpContext.Configure(dummyCtxAccessor);
                    #endif

                    using (var mon = new KMonitor(Events.eEvent.EVENT_API_START, serviceEvent?.GroupId.ToString(), eventName, serviceEvent?.RequestId))
                    {
                        mon.Database = eventName;
                        mon.Table = handler.GetType().Name;
                        await (Task) handleMethod.Invoke(handler, new[] {serviceEvent});
                    }
                }
            }
        }

        private void SetEventContext(IServiceScope scope, ServiceEvent serviceEvent)
        {
            var eventContext = scope.ServiceProvider.GetService<IEventContext>() as EventContext;
            // In case EventContext of specific type is null, meaning that it has been overriden by the user.
            eventContext?.PopulateFromServiceEvent(serviceEvent);
        }
        
        private ServiceEvent SetLoggingContext(object serviceEvent)
        {
            // TODO: Arthur, Think of the bigger context picture and how should it be managed, for logging and for in memepry data store
            var eventData = (ServiceEvent) serviceEvent;
            _Logger.Debug($"ProcessEvent > eventType:[{eventData.GetType().Name}] groupId:[{eventData.GroupId}] requestId:[{eventData.RequestId}] userId:[{eventData.UserId}]");

            KLogger.LogContextData[Phx.Lib.Log.Constants.USER_ID] = eventData.UserId;
            KLogger.LogContextData[Phx.Lib.Log.Constants.GROUP_ID] = eventData.GroupId;
            KLogger.LogContextData[Phx.Lib.Log.Constants.REQUEST_ID_KEY] = eventData.RequestId;
            return eventData;
        }

        private void InitializeNewEventBinding(string dedicatedQueueName, string routingKey)
        {
            _Logger.Info($"Initializing new event binding on queue:[{_QueueName}], exchange:{_ExchangeName}, routingKey:[{routingKey}]");
            using (var channel = _PersistentConnection.CreateModel())
            {
                channel.QueueBind(dedicatedQueueName, _ExchangeName, routingKey);
            }
        }
    }
}
