using System.Collections.Generic;
using ApiLogic.Context;
using ApiObjects;
using Confluent.Kafka;
using EventBus.Kafka;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Logical.IndexRecording;

namespace ApiLogic.Catalog.Services
{
    public static class IndexRecordingMessageService
    {
        private static readonly IKafkaProducer<string, IndexRecording> _indexRecordingProducer;
        
        static IndexRecordingMessageService()
        {
            IKafkaProducerFactory producerFactory = KafkaProducerFactoryInstance.Get();
            _indexRecordingProducer = producerFactory.Get<string, IndexRecording>(WebKafkaContextProvider.Instance, Partitioner.Murmur2Random);
        }
        
        public static void PublishIndexRecordingKafkaEvent(long groupId, List<long> ids, eAction action)
        {
            var indexRecording = new IndexRecording
            {
                PartnerId = groupId,
                AssetIds = ids.ToArray(),
                Action = action.ToString()
            };
            
            _indexRecordingProducer.Produce(IndexRecording.GetTopic(),
                indexRecording.GetPartitioningKey(), indexRecording);
        }
    }
}