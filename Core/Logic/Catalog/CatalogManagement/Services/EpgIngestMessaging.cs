using System;
using System.Collections.Generic;
using System.Threading;
using ApiObjects.BulkUpload;
using ApiObjects.EventBus.EpgIngest;
using ApiObjects.Response;
using EventBus.Abstraction;
using EventBus.Kafka;
using Phx.Lib.Log;
using Newtonsoft.Json;
using TVinciShared;

namespace Core.Catalog.CatalogManagement.Services
{
    public interface IEpgIngestMessaging
    {
        void EpgIngestStarted(int groupId, long userId, long bulkUploadId, int? ingestProfileId, string ingestFileName,
            DateTime createdDate);

        void EpgIngestCompleted(int groupId, long userId, long bulkUploadId, BulkUploadJobStatus status,
            IEnumerable<Status> errors, DateTime completedDate);
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

        public void EpgIngestStarted(int groupId, long userId, long bulkUploadId, int? ingestProfileId,
            string ingestFileName, DateTime createdDate)
        {
            var e = new EpgIngestStarted
            {
                RequestId = KLogger.GetRequestId(),
                IngestedByUserId = userId,
                PartnerId = groupId,
                IngestId = bulkUploadId,
                CreatedDate = createdDate.ToUtcUnixTimestampSeconds(),
                IngestProfileId = ingestProfileId,
                IngestFileName = ingestFileName
            };
            PublishToKafka(e);
        }
        
        public void EpgIngestCompleted(int groupId, long userId, long bulkUploadId, BulkUploadJobStatus status, IEnumerable<Status> errors, DateTime completedDate)
        {
            var e = new EpgIngestCompleted
            {
                RequestId = KLogger.GetRequestId(),
                UserId = userId,
                PartnerId = groupId,
                IngestId = bulkUploadId,
                CompletedDate = completedDate.ToUtcUnixTimestampSeconds(),
                Status = ToCompletionStatus(status),
                Errors = errors
            };
            PublishToKafka(e);
        }

        private void PublishToKafka(ServiceEvent e)
        {
            _logger.Debug($"epg ingest. send to kafka. type:[{e.GetType().Name}]. msg:[{JsonConvert.SerializeObject(e)}]");
            _kafkaPublisher.Publish(e);
        }

        private static EpgIngestCompletionStatus ToCompletionStatus(BulkUploadJobStatus status)
        {
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
    }
}
