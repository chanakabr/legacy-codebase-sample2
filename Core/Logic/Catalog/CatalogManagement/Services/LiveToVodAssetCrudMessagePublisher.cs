using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Context;
using Confluent.Kafka;
using EventBus.Kafka;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Crud.Asset.LiveToVodAsset;
using Phx.Lib.Log;
using TVinciShared;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class LiveToVodAssetCrudMessagePublisher : ILiveToVodAssetCrudMessagePublisher
    {
        private static readonly Lazy<ILiveToVodAssetCrudMessagePublisher> LazyInstance =
            new Lazy<ILiveToVodAssetCrudMessagePublisher>(
                () => new LiveToVodAssetCrudMessagePublisher(
                    KafkaProducerFactoryInstance.Get(),
                    WebKafkaContextProvider.Instance),
                LazyThreadSafetyMode.PublicationOnly);
        private static readonly KLogger Logger = new KLogger(nameof(LiveToVodAssetCrudMessagePublisher));

        private readonly IKafkaProducer<string, LiveToVodAsset> _liveToVodAssetProducer;

        public static ILiveToVodAssetCrudMessagePublisher Instance = LazyInstance.Value;

        public LiveToVodAssetCrudMessagePublisher(IKafkaProducerFactory producerFactory, IKafkaContextProvider contextProvider)
        {
            _liveToVodAssetProducer = producerFactory.Get<string, LiveToVodAsset>(contextProvider, Partitioner.Murmur2Random);
        }

        public void Publish(long partnerId, Core.Catalog.LiveToVodAsset asset, IEnumerable<string> files, int operationType, long updaterId)
        {
            if (_liveToVodAssetProducer == null)
            {
                Logger.LogError($"{nameof(LiveToVodAsset)} message with parameters {nameof(partnerId)}={partnerId}," +
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
                MediaFileUrls = files?.ToArray() ?? Array.Empty<string>(),
                UpdaterId = updaterId
            };

            _liveToVodAssetProducer.Produce(
                LiveToVodAsset.GetTopic(),
                liveToVodAssetEvent.GetPartitioningKey(),
                liveToVodAssetEvent);
        }
    }
}