using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using ApiObjects.EventBus;
using ConfigurationManager;
using EventBus.Abstraction;
using KLogMonitor;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ
{
    public class EventBusPublisherRabbitMQ : IEventBusPublisher
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly IRabbitMQPersistentConnection _PersistentConnection;
        private readonly string _ExchangeName;
        private readonly int _RetryCount;

        public static EventBusPublisherRabbitMQ GetInstanceUsingTCMConfiguration()
        {
            // TODO: MakeSingleTone, in phoenix it is used without IOC \ DI so it has to maintaine a singletone on its one and not relay on ServiceCollection :(
            var eventBusConsumer = new EventBusPublisherRabbitMQ(
                RabbitMQPersistentConnection.GetInstanceUsingTCMConfiguration(),
                ApplicationConfiguration.Current.RabbitConfiguration.EventBus.Exchange.Value,
                ApplicationConfiguration.Current.QueueFailLimit.Value);

            return eventBusConsumer;
        }

        private EventBusPublisherRabbitMQ(IRabbitMQPersistentConnection persistentConnection, string exchangeName = "kaltura_event_bus", int retryCount = 5)
        {
            _PersistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _ExchangeName = exchangeName;
            _RetryCount = retryCount;
        }

        public void Publish(ServiceEvent serviceEvent)
        {
            Publish(new[] { serviceEvent });
        }

        public void Publish(IEnumerable<ServiceEvent> serviceEvents)
        {
            var publishRetryPolicy = GetRetryPolicyForEventPublishing();
            using (var channel = _PersistentConnection.CreateModel())
            {
                channel.ConfirmSelect();
                channel.BasicAcks += (o, e) =>
                {
                    _Logger.Debug($"Event delivered with tag:[{e.DeliveryTag}]");
                };

                foreach (var serviceEvent in serviceEvents)
                {
                    var eventName = ServiceEvent.GetEventName(serviceEvent);
                    var message = JsonConvert.SerializeObject(serviceEvent);
                    var body = Encoding.UTF8.GetBytes(message);

                    publishRetryPolicy.Execute(() =>
                    {
                        PublishEvent(channel, eventName, body, message);
                    });
                }
            }
        }

        private void PublishEvent(IModel channel, string eventName, byte[] body, string message)
        {
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2; // persistent
            const bool isDeliverySuccessMandatory = true;
            channel.BasicPublish(_ExchangeName, eventName, isDeliverySuccessMandatory, properties, body);
            _Logger.Info($"Event [{eventName}] with body:[{message}] sent to exchange:[{_ExchangeName}]");
            // TODO: Configure wait for conformation timespan;
            // TODO: This takes long, 100 msgs takes 11 seconds .. whitout it takes 1 sec.
            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
        }

        private RetryPolicy GetRetryPolicyForEventPublishing()
        {
            return RetryPolicy.Handle<BrokerUnreachableException>().Or<SocketException>()
                .WaitAndRetry(_RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _Logger.Warn(ex.ToString());
                    _Logger.Warn($"Waiting for:[{time.TotalSeconds}] seconds until next publish retry");
                });
        }
    }
}
