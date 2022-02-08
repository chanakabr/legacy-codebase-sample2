using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiObjects.BulkUpload;
using ApiObjects.EventBus.EpgIngest;
using EventBus.Abstraction;
using EventBus.Kafka;
using Newtonsoft.Json;
using Phx.Lib.Log;
using TVinciShared;

namespace Core.Catalog.CatalogManagement.Services
{
    public interface IEpgIngestMessaging
    {
        void EpgIngestStarted(EpgIngestStartedParameters parameters);
        void EpgIngestCompleted(EpgIngestCompletedParameters parameters);
        void EpgIngestPartCompleted(EpgIngestPartCompletedParameters parameters);
    }

    public class EpgIngestMessaging : IEpgIngestMessaging
    {
        private static readonly Lazy<EpgIngestMessaging> LazyInstance = new Lazy<EpgIngestMessaging>(() =>
                new EpgIngestMessaging(
                    KafkaPublisher.GetFromTcmConfiguration(),
                    new KLogger(nameof(EpgIngestMessaging))),
            LazyThreadSafetyMode.PublicationOnly);

        public static IEpgIngestMessaging Instance => LazyInstance.Value;

        private readonly IEventBusPublisher _kafkaPublisher;
        private readonly IKLogger _logger;

        public EpgIngestMessaging(IEventBusPublisher kafkaPublisher, IKLogger logger)
        {
            _kafkaPublisher = kafkaPublisher;
            _logger = logger;
        }

        public void EpgIngestStarted(EpgIngestStartedParameters parameters)
        {
            var e = new EpgIngestStarted
            {
                RequestId = KLogger.GetRequestId(),
                IngestedByUserId = parameters.UserId,
                PartnerId = parameters.GroupId,
                IngestId = parameters.BulkUploadId,
                CreatedDate = parameters.CreatedDate.ToUtcUnixTimestampSeconds(),
                IngestProfileId = parameters.IngestProfileId,
                IngestFileName = parameters.IngestFileName
            };

            PublishToKafka(e);
        }

        public void EpgIngestCompleted(EpgIngestCompletedParameters parameters)
        {
            var e = new EpgIngestCompleted
            {
                RequestId = KLogger.GetRequestId(),
                UserId = parameters.UserId,
                PartnerId = parameters.GroupId,
                IngestId = parameters.BulkUploadId,
                CompletedDate = parameters.CompletedDate.ToUtcUnixTimestampSeconds(),
                Status = ToCompletionStatus(parameters.Status, parameters.Results),
                Errors = parameters.Errors
            };

            PublishToKafka(e);
        }

        public void EpgIngestPartCompleted(EpgIngestPartCompletedParameters parameters)
        {
            var e = new EpgIngestPartCompleted
            {
                RequestId = KLogger.GetRequestId(),
                UserId = parameters.UserId,
                PartnerId = parameters.GroupId,
                IngestId = parameters.BulkUploadId,
                HasMore = parameters.HasMoreEpgToIngest,
                Results = parameters.Results.Select(MapToEpgIngestResult).ToArray()
            };

            PublishToKafka(e);
        }

        private void PublishToKafka(ServiceEvent e)
        {
            _logger.Debug($"epg ingest. send to kafka. type:[{e.GetType().Name}]. msg:[{JsonConvert.SerializeObject(e)}]");
            _kafkaPublisher.Publish(e);
        }

        private static EpgIngestResult MapToEpgIngestResult(BulkUploadProgramAssetResult source)
            => new EpgIngestResult
            {
                StartDate = source.StartDate.ToUtcUnixTimestampSeconds(),
                EndDate = source.EndDate.ToUtcUnixTimestampSeconds(),
                LinearChannelId = source.LiveAssetId,
                Status = MapStatus(source),
                IndexInFile = source.Index,
                ExternalProgramId = source.ProgramExternalId,
                ProgramId = source.ProgramId,
                Errors = source.Errors,
                Warnings = source.Warnings
            };

        private static EpgIngestCompletionStatus ToCompletionStatus(BulkUploadJobStatus status, IEnumerable<BulkUploadResult> results)
        {
            if (status == BulkUploadJobStatus.Success
                && results != null
                && results.Any(x => x.Warnings?.Length > 0))
            {
                return EpgIngestCompletionStatus.WARNING;
            }

            switch (status)
            {
                case BulkUploadJobStatus.Success: return EpgIngestCompletionStatus.SUCCESS;
                case BulkUploadJobStatus.Partial: return EpgIngestCompletionStatus.PARTIAL_FAILURE;
                case BulkUploadJobStatus.Failed: return EpgIngestCompletionStatus.TOTAL_FAILURE;
                case BulkUploadJobStatus.Fatal: return EpgIngestCompletionStatus.TOTAL_FAILURE;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private static ProgramIngestResultStatus MapStatus(BulkUploadProgramAssetResult source)
        {
            if (source.Status == BulkUploadResultStatus.Ok && source.Warnings?.Length > 0)
            {
                return ProgramIngestResultStatus.Warning;
            }

            switch (source.Status)
            {
                case BulkUploadResultStatus.Error:
                    return ProgramIngestResultStatus.Error;
                case BulkUploadResultStatus.Ok:
                    return ProgramIngestResultStatus.Success;
                default:
                    throw new ArgumentException(nameof(source.Status));
            }
        }
    }
}
