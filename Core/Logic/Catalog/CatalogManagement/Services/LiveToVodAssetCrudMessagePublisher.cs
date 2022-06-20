using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Crud.Asset.LiveToVodAsset;
using TVinciShared;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class LiveToVodAssetCrudMessagePublisher : ILiveToVodAssetCrudMessagePublisher
    {
        private readonly IKafkaProducer<string, LiveToVodAsset> _liveToVodAssetProducer;
        private readonly ILogger<LiveToVodAssetCrudMessagePublisher> _logger;

        public LiveToVodAssetCrudMessagePublisher(
            IKafkaProducerFactory producerFactory,
            IKafkaContextProvider contextProvider,
            ILogger<LiveToVodAssetCrudMessagePublisher> logger)
        {
            _liveToVodAssetProducer = producerFactory.Get<string, LiveToVodAsset>(contextProvider, Partitioner.Murmur2Random);
            _logger = logger;
        }

        public void Publish(long partnerId, Core.Catalog.LiveToVodAsset asset, IEnumerable<string> files, int operationType)
        {
            if (_liveToVodAssetProducer == null)
            {
                _logger.LogError($"{nameof(LiveToVodAsset)} message with parameters {nameof(partnerId)}={partnerId}," +
                    $"{nameof(LiveToVodAsset.Id)}=[{asset.Id}], operation={operationType}" +
                    $"can not be published : {nameof(_liveToVodAssetProducer)} is null.");
                return;
            }

            var liveToVodAssetEvent = new LiveToVodAsset
            {
                PartnerId = partnerId,
                Id = asset.Id,
                StartDate = asset.StartDate.Value.ToUtcUnixTimestampSeconds(),
                EndDate = asset.EndDate.Value.ToUtcUnixTimestampSeconds(),
                OrigStartDate = asset.OriginalStartDate.ToUtcUnixTimestampSeconds(),
                OrigEndDate = asset.OriginalEndDate.ToUtcUnixTimestampSeconds(),
                LinearAssetId = asset.LinearAssetId,
                OrigProgramAssetId = asset.EpgId,
                PaddingBeforeProgramStarts = asset.PaddingBeforeProgramStarts,
                PaddingAfterProgramEnds = asset.PaddingAfterProgramEnds,
                Operation = operationType,
                MediaFileUrls = files?.ToArray() ?? Array.Empty<string>()
            };

            _liveToVodAssetProducer.Produce(
                LiveToVodAsset.GetTopic(),
                liveToVodAssetEvent.GetPartitioningKey(),
                liveToVodAssetEvent);
        }
    }
}