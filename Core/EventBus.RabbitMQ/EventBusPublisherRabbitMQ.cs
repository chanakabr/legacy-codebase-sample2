using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
        public const string HEADER_KEY_EVENT_BASE_NAME = "x-ott-eventbus-event-name";

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
            Publish(new[] { serviceEvent }, false);
        }

        public void Publish(IEnumerable<ServiceEvent> serviceEvents)
        {
            Publish(serviceEvents, false);
        }

        public void PublishHeadersOnly(ServiceEvent serviceEvent, Dictionary<string, string> headersToAdd = null)
        {
            Publish(new[] { serviceEvent }, true, headersToAdd);
        }

        private void Publish(IEnumerable<ServiceEvent> serviceEvents, bool shouldSendOnlyHeaders = false, Dictionary<string, string> headersToAdd = null)
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
                    byte[] body = null;
                    var routingKey = serviceEvent.GetRoutingKey();
                    var eventName = serviceEvent.GetEventName();

                    if (!shouldSendOnlyHeaders)
                    {
                        try
                        {
                            body = RabbitMqSerializationsHelper.Serialize(serviceEvent);
                        }
                        catch (SerializationException e)
                        {
                            _Logger.Warn("Could not serialize ,will use JsonConvert and Encoding.UTF8.GetBytes instead.if you see this error add [Serializable] on the object " + e.Message);
                            var jsonMsg = JsonConvert.SerializeObject(serviceEvent);
                            body = Encoding.UTF8.GetBytes(jsonMsg);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                        
                    publishRetryPolicy.Execute(() =>
                    {
                        PublishEvent(channel, routingKey,eventName, body, string.Empty, headersToAdd);
                    });
                    
                }
            }
        }

        private void PublishEvent(IModel channel, string routingKey, string eventName, byte[] body, string message, Dictionary<string, string> headersToAdd = null)
        {
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2; // persistent

            //Gil:this is a temp fix until ingest handlers and phoenix are going to be synced so we need to have a
            //backwards compatibility to suppport json serialized data (we changed to binarry formatter)

            if (properties.Headers == null)
            {
                properties.Headers = new Dictionary<string, object>();
            }

            if (headersToAdd != null)
            {
                foreach (KeyValuePair<string, string> header in headersToAdd)
                {
                    properties.Headers.Add(header.Key, header.Value);
                }
            }

            properties.Headers.Add(HEADER_KEY_EVENT_BASE_NAME, eventName);
            const bool isDeliverySuccessMandatory = true;
            channel.BasicPublish(_ExchangeName, routingKey, isDeliverySuccessMandatory, properties, body);
            _Logger.Info($"RoutingKey [{routingKey}], EventName:[{eventName}] with body length:[{message?.Length}] sent to exchange:[{_ExchangeName}]");
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
