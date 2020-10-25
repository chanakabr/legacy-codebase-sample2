using ConfigurationManager;
using Confluent.Kafka;
using EventBus.Abstraction;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EventBus.Kafka
{


    public class KafkaPublisher : IEventBusPublisher
    {
        private static IProducer<string, string> _Producer;
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public static IEventBusPublisher GetFromTcmConfiguration()
        {
            var tcmConfig = ApplicationConfiguration.Current.KafkaClientConfiguration;
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = tcmConfig.BootstrapServers.Value,
                SocketTimeoutMs = tcmConfig.SocketTimeoutMs.Value,
                ClientId = KLogger.GetServerName(),
                // TODO: talk to eli about partition implementation in golang
                Partitioner = Partitioner.Random,
            };

            var producerFactory = new KafkaProducerFactory<string, string>(producerConfig);
            var publisher = new KafkaPublisher(producerFactory);
            return publisher;
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
            }
        }
    }
}
