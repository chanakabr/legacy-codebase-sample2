using System.Collections.Generic;
using ApiLogic.Context;
using ApiObjects;
using Confluent.Kafka;
using EventBus.Kafka;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Logical.RebuildRecordingsIndex;
using Language = Phoenix.Generated.Api.Events.Logical.RebuildRecordingsIndex.Language;

namespace ElasticSearchHandler.IndexBuilders
{
    public class RebuildRecordingsIndexMessageService
    {
        private readonly IKafkaProducer<string, RebuildRecordingsIndex> _indexRecordingProducer;
        
        public RebuildRecordingsIndexMessageService()
        {
            IKafkaProducerFactory producerFactory = KafkaProducerFactoryInstance.Get();
            _indexRecordingProducer = producerFactory.Get<string, RebuildRecordingsIndex>(WebKafkaContextProvider.Instance, Partitioner.Murmur2Random);
        }
        
        public void PublishKafkaEvent(long groupId, List<LanguageObj> languages, bool switchIndexAlias, bool deleteOldIndices, string newIndexName)
        {
            Language[] languagesArray = AutoMapper.Mapper.Map<Language[]>(languages);
            RebuildRecordingsIndex rebuildRecordingsIndex = new RebuildRecordingsIndex
            {
                PartnerId = groupId,
                Languages = languagesArray,
                DeleteOldIndices = deleteOldIndices,
                SwitchIndexAlias = switchIndexAlias,
                NewIndexName = newIndexName
            };
            
            _indexRecordingProducer.ProduceAsync(RebuildRecordingsIndex.GetTopic(),
                rebuildRecordingsIndex.GetPartitioningKey(), rebuildRecordingsIndex);
        }
    }
}