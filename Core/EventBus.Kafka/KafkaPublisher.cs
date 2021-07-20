using ConfigurationManager;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Couchbase.N1QL;
using EventBus.Abstraction;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EventBus.Kafka
{
    public class KafkaPublisher : IEventBusPublisher
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string REQ_ID_HEADER = "traceId";
        private const string PARTNER_ID_HEADER = "partnerId";
        private const string USER_ID_HEADER = "userId";
        private static IProducer<string, string> _Producer = null;
        private static KafkaPublisher _RandomProducerInstance = null;
        private static KafkaPublisher _ConsistantProducerInstance = null;
        private static object locker = new object();

        private bool _IsHealthy = true;

        public static IEventBusPublisher GetFromTcmConfiguration(bool useRandomPartitioner=true)
        {
            if (useRandomPartitioner)
            {
                if (_RandomProducerInstance == null)
                {
                    lock (locker)
                    {
                        var tcmConfig = ApplicationConfiguration.Current.KafkaClientConfiguration;
                        if (_RandomProducerInstance != null) return _RandomProducerInstance;
                        _RandomProducerInstance = GetKafkaPublisher(tcmConfig, Partitioner.Murmur2Random);
                    }
                }
                return _RandomProducerInstance;
            }

            if (_ConsistantProducerInstance == null) 
            {
                lock (locker)
                {
                    var tcmConfig = ApplicationConfiguration.Current.KafkaClientConfiguration;
                    if (_ConsistantProducerInstance != null) return _ConsistantProducerInstance;
                    _ConsistantProducerInstance = GetKafkaPublisher(tcmConfig, Partitioner.Murmur2);
                    
                }
            }
            return _ConsistantProducerInstance;
        }

        public static KafkaPublisher GetKafkaPublisher(KafkaClientConfiguration tcmConfig, Partitioner partitioner)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = tcmConfig.BootstrapServers.Value,
                SocketTimeoutMs = tcmConfig.SocketTimeoutMs.Value,
                ClientId = KLogger.GetServerName(),
                Partitioner = partitioner,
            };

            // create topic for health check if it doesn't exist
            using (var adminClient =
                new AdminClientBuilder(new AdminClientConfig {BootstrapServers = tcmConfig.BootstrapServers.Value}).Build())
            {
                try
                {
                    var createTopicsResult = adminClient.CreateTopicsAsync(new TopicSpecification[]
                    {
                        new TopicSpecification
                        {
                            Name = ApplicationConfiguration.Current.KafkaClientConfiguration.HealthCheckTopic.Value,
                            ReplicationFactor = 1,
                            NumPartitions = 1
                        }
                    });
                }
                catch (CreateTopicsException e)
                {
                    _Logger.Error($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}", e);
                }
            }

            var producerFactory = new KafkaProducerFactory<string, string>(producerConfig);
            var puvlisher = new KafkaPublisher(producerFactory);
            return puvlisher;
        }

        public KafkaPublisher(IKafkaProducerFactory<string, string> producerFactory)
        {
            _Producer = producerFactory.Build();
        }

        public void Publish(ServiceEvent serviceEvent)
        {
            Publish(serviceEvent, false);
        }

        public void Publish(IEnumerable<ServiceEvent> serviceEvents)
        {
            if (serviceEvents == null)
                return;

            foreach (ServiceEvent serviceEvent in serviceEvents)
            {
                Publish(serviceEvent, false);
            }
        }

        public void PublishHeadersOnly(ServiceEvent serviceEvent, Dictionary<string, string> headersToAdd = null)
        {
            Publish(serviceEvent, true, headersToAdd);
        }

        private void Publish(ServiceEvent serviceEvent, bool shouldSendOnlyHeaders = false, Dictionary<string, string> headersToAdd = null)
        {
            string groupId = serviceEvent.GroupId.ToString();
            string reqId = serviceEvent.RequestId;
            var topic = serviceEvent.GetRoutingKey();
            var msg = new Message<string, string>();
            string key = serviceEvent.EventKey;
            msg.Key = key;
            msg.Headers = GetMessageHeaders(groupId, reqId, serviceEvent.UserId, headersToAdd);
            if (!shouldSendOnlyHeaders)
            {
                msg.Value = JsonConvert.SerializeObject(serviceEvent);
            }
            
            using (var kmon = new KMonitor(Events.eEvent.EVENT_KAFKA, groupId, "kafka.publish", reqId) { Database = topic, Table = key })
            {                
                _Producer.Produce(topic, msg, DeliveryHandler);
            }
        }

        private Headers GetMessageHeaders(string groupId, string reqId, long userId, Dictionary<string, string> headersToAdd = null)
        {
            Headers headers = new Headers();
            headers.Add(PARTNER_ID_HEADER, System.Text.Encoding.UTF8.GetBytes(groupId));
            if (!string.IsNullOrEmpty(reqId))
            {
                headers.Add(REQ_ID_HEADER, System.Text.Encoding.UTF8.GetBytes(reqId));
            }

            if (userId > 0)
            {
                headers.Add(USER_ID_HEADER, System.Text.Encoding.UTF8.GetBytes(userId.ToString()));
            }

            if (headersToAdd != null)
            {
                foreach (KeyValuePair<string, string> header in headersToAdd)
                {
                    headers.Add(header.Key, System.Text.Encoding.UTF8.GetBytes(header.Value)); 
                }
            }

            return headers;
        }

        private void DeliveryHandler(DeliveryReport<string, string> ack)
        {
            if (ack.Error.IsError)
            {
                _Logger.Debug($"KafkaPublisher > Delivery Report key:[{ack.Key}], val:[{ack.Value}], err:[{ack.Error}]");
                _IsHealthy = false;
            }
            else
            {
                _IsHealthy = true;
            }
        }

        public bool HealthCheck()
        {
            return _IsHealthy;
        }
    }
}
