using ConfigurationManager;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventBus.Abstraction;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace EventBus.Kafka
{
    public class KafkaPublisher : IEventBusPublisher
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static IProducer<string, string> _Producer = null;
        private static KafkaPublisher _Instance = null;
        private static object locker = new object();

        private bool _IsHealthy = true;

        public static IEventBusPublisher GetFromTcmConfiguration()
        {
            if (_Instance == null)
            {
                lock (locker)
                {
                    if (_Instance == null)
                    {
                        var tcmConfig = ApplicationConfiguration.Current.KafkaClientConfiguration;
                        var producerConfig = new ProducerConfig
                        {
                            BootstrapServers = tcmConfig.BootstrapServers.Value,
                            SocketTimeoutMs = tcmConfig.SocketTimeoutMs.Value,
                            ClientId = KLogger.GetServerName(),
                            // TODO: talk to eli about partition implementation in golang
                            Partitioner = Partitioner.Murmur2Random,
                        };

                        // create topic for health check if it doesn't exist
                        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = tcmConfig.BootstrapServers.Value }).Build())
                        {
                            try
                            {
                                  var createTopicsResult = adminClient.CreateTopicsAsync(new TopicSpecification[] {
                                    new TopicSpecification {
                                        Name = ApplicationConfiguration.Current.KafkaClientConfiguration.HealthCheckTopic.Value,
                                        ReplicationFactor = 1,
                                        NumPartitions = 1 } });
                            }
                            catch (CreateTopicsException e)
                            {
                                _Logger.Error($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}", e);
                            }
                        }

                        var producerFactory = new KafkaProducerFactory<string, string>(producerConfig);
                        _Instance = new KafkaPublisher(producerFactory);
                    }
                }
            }
            
            return _Instance;
        }

        public KafkaPublisher(IKafkaProducerFactory<string, string> producerFactory)
        {
            _Producer = producerFactory.Build();
        }

        public void Publish(ServiceEvent serviceEvent)
        {
            using (var kmon = new KLogMonitor.KMonitor(Events.eEvent.EVENT_KAFKA, serviceEvent.GroupId.ToString(), "kafka.publish", serviceEvent.RequestId))
            {
                var topic = serviceEvent.GetRoutingKey();
                var key = serviceEvent.EventKey;
                var payload = JsonConvert.SerializeObject(serviceEvent);
                kmon.Database = topic;
                kmon.Table = key;

                var msg = new Message<string, string> { Key = key, Value = payload };
                _Producer.Produce(topic, msg, DeliveryHandler);
            }
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
