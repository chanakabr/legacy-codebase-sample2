using System;
using System.Linq;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Context;
using ApiObjects;
using ApiObjects.Response;
using Confluent.Kafka;
using Core.Catalog.CatalogManagement;
using EventBus.Kafka;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Logical.Ingest.Vod.IngestResult;
using Phx.Lib.Log;
using TVinciShared;
using Error = Phoenix.Generated.Api.Events.Logical.Ingest.Vod.IngestResult.Error;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class VodIngestAssetResultPublisher : IVodIngestAssetResultPublisher
    {
        private static readonly Lazy<IVodIngestAssetResultPublisher> LazyInstance =
            new Lazy<IVodIngestAssetResultPublisher>(
                () => new VodIngestAssetResultPublisher(
                    KafkaProducerFactoryInstance.Get(),
                    WebKafkaContextProvider.Instance,
                    CatalogManager.Instance),
                LazyThreadSafetyMode.PublicationOnly);
        private static readonly KLogger Logger = new KLogger(nameof(LiveToVodAssetCrudMessagePublisher));

        private readonly ICatalogManager _catalogManager;
        private readonly IKafkaProducer<string, IngestResult> _vodIngestResultProducer;

        public static IVodIngestAssetResultPublisher Instance = LazyInstance.Value;

        public VodIngestAssetResultPublisher(
            IKafkaProducerFactory producerFactory,
            IKafkaContextProvider contextProvider,
            ICatalogManager catalogManager)
        {
            _catalogManager = catalogManager;
            _vodIngestResultProducer = producerFactory.Get<string, IngestResult>(contextProvider, Partitioner.Murmur2Random);
        }

        public void PublishFailedIngest(int groupId, Status status, string fileName)
        {
            var @event = new IngestResult
            {
                IngestDate = DateUtils.GetUtcUnixTimestampNow(),
                PartnerId = groupId,
                FileName = fileName,
                Status = VodIngestStatus.FAILURE.ToString("G"),
                Errors = new [] { MapIngestError(status) }
            };

            _vodIngestResultProducer.Produce(IngestResult.GetTopic(), @event.GetPartitioningKey(), @event);
        }

        public void Publish(VodIngestPublishContext context)
        {
            if (!_catalogManager.TryGetCatalogGroupCacheFromCache(context.GroupId, out var cache))
            {
                Logger.ErrorFormat("failed to get catalogGroupCache for groupId: {0} while publishing VOD ingest result",
                    context.GroupId);

                return;
            }

            var defaultLanguage = cache.GetDefaultLanguage().Code;
            var name = context.Media.Basic?.Name?.Values
                .FirstOrDefault(x => x.LangCode.Equals(defaultLanguage))?.Text ?? context.MediaAsset?.Name;
            var @event = new IngestResult
            {
                ExternalAssetId = context.Media.CoGuid ?? context.MediaAsset?.CoGuid,
                IngestDate = DateUtils.GetUtcUnixTimestampNow(),
                AssetName = name,
                FileName = context.FileName,
                AssetStructSystemName = context.Media.Basic?.MediaType,
                PartnerId = context.GroupId,
                ShopAssetUserRuleId = context.ShopAssetUserRuleId,
                AssetId = context.MediaAsset?.Id > 0 ? context.MediaAsset?.Id : null,
                Status = ResolveStatus(context.AssetStatus).ToString("G")
            };

            if (context.AssetStatus.Warnings.Any())
            {
                @event.Warnings = context.AssetStatus.Warnings.Select(MapIngestError).ToArray();
            }

            if (!context.AssetStatus.Status.IsOkStatusCode())
            {
                @event.Errors = new[] { MapIngestError(context.AssetStatus.Status) };
            }

            _vodIngestResultProducer.Produce(IngestResult.GetTopic(), @event.GetPartitioningKey(), @event);
        }

        private static Error MapIngestError(Status status)
            => new Error
            {
                Code = status.Code, Message = status.Message
            };

        private static VodIngestStatus ResolveStatus(IngestAssetStatus status)
        {
            if (!status.Status.IsOkStatusCode())
            {
                return VodIngestStatus.FAILURE;
            }

            return status.Warnings.Any() ? VodIngestStatus.SUCCESS_WARNING : VodIngestStatus.SUCCESS;
        }
    }

    public enum VodIngestStatus
    {
        SUCCESS,
        SUCCESS_WARNING,
        FAILURE
    }
}