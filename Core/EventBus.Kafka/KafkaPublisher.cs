using Confluent.Kafka;
using EventBus.Abstraction;
using Phx.Lib.Log;
using Newtonsoft.Json;
using System.Collections.Generic;
using OTT.Lib.Kafka;

namespace EventBus.Kafka
{
    public class KafkaPublisher : IEventBusPublisher
    {
        private const string REQ_ID_HEADER = "traceId";
        private static readonly KLogger Logger = new KLogger(nameof(KafkaPublisher));
        private static bool _isHealthy = true;

        private readonly IKafkaProducer<string, string> _producer;

        public KafkaPublisher(IKafkaContextProvider contextProvider, IKafkaProducerFactory producerFactory)
            : this(producerFactory.Get<string, string>(contextProvider, Partitioner.Murmur2Random))
        {
        }

        private KafkaPublisher(IKafkaProducer<string, string> producer)
        {
            _producer = producer;
        }

        public static IEventBusPublisher GetFromTcmConfiguration(IKafkaContextProvider contextProvider, bool useRandomPartitioner = true)
        {
            var partitioner = useRandomPartitioner
                ? Partitioner.Murmur2Random
                : Partitioner.Murmur2;
            var kafkaProducer = KafkaProducerFactoryInstance.Get().Get<string, string>(contextProvider, partitioner);
            var publisher = new KafkaPublisher(kafkaProducer);

            return publisher;
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

        private void Publish(ServiceEvent serviceEvent, bool shouldSendOnlyHeaders, Dictionary<string, string> headersToAdd = null)
        {
            var groupId = serviceEvent.GroupId.ToString();
            var reqId = serviceEvent.RequestId;
            var topic = serviceEvent.GetRoutingKey();
            var key = serviceEvent.EventKey;
            using (new KMonitor(Events.eEvent.EVENT_KAFKA, groupId, "kafka.publish", reqId) { Database = topic, Table = key })
            {
                if (shouldSendOnlyHeaders)
                {
                    _producer.Produce(topic, serviceEvent.EventKey, null, headersToAdd, DeliveryHandler);
                }
                else
                {
                    var value = JsonConvert.SerializeObject(serviceEvent);
                    _producer.Produce(topic, serviceEvent.EventKey, value, headersToAdd, DeliveryHandler);
                }
            }
        }

        private void DeliveryHandler(DeliveryReport<string, string> ack)
        {
            if (ack.Error.IsError)
            {
                var traceId = ack.Headers.TryGetLastBytes(REQ_ID_HEADER, out var bytes)
                    ? System.Text.Encoding.UTF8.GetString(bytes)
                    : "unknown";
                Logger.Error($"KafkaPublisher > Delivery Report key:[{ack.Key}], val:[{ack.Value}], traceId:[{traceId}], err:[{ack.Error}]");
                _isHealthy = false;
            }
            else
            {
                using (new KMonitor(Events.eEvent.EVENT_KAFKA, "0", $"kafka.publish.success.{ack.Offset}", KLogger.GetRequestId()) { Database = ack.Value, Table = ack.Key, })
                {
                    _isHealthy = true;
                }
            }
        }

        public static bool HealthCheck()
        {
            return _isHealthy;
        }
    }
}