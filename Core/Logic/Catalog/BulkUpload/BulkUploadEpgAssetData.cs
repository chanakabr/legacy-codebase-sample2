using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using EventBus.RabbitMQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.EventBus;
using KLogMonitor;
using System.Reflection;
using Synchronizer;
using ConfigurationManager;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadEpgAssetData : BulkUploadObjectData
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        // TODO Arthur - remove disterbutedTask and ruting key from media assets and use the event bus instead.
        public override string DistributedTask { get { return "disterbuted task not supported for epg ingest, use event bus instead"; } }
        public override string RoutingKey { get { return "disterbuted task not supported for epg ingest, use event bus instead"; } }

        private static readonly Type bulkUploadObjectType = typeof(EpgProgramBulkUploadObject);

        public override Type GetObjectType()
        {
            return bulkUploadObjectType;
        }

        public override void EnqueueObjects(BulkUpload bulkUpload, List<BulkUploadResult> objects)
        {
            var hasErrors = objects.Any(o => o.Errors?.Any() == true);
            if (hasErrors)
            {
                _Logger.Error($"Ingest transformation encountered errors. will no enqueue bulkUploadId:[{bulkUpload.Id}], groupId:[{bulkUpload.GroupId}]");
                return;
            }

            var programResults = objects.Cast<BulkUploadProgramAssetResult>().Select(r => r.Object as EpgProgramBulkUploadObject);
            var programsByDate = programResults.GroupBy(p => p.StartDate.Date);
            var publisher = EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration();

            var bulkUploadIngestEvents = programsByDate.Select(p => new BulkUploadIngestEvent
            {
                BulkUploadId = bulkUpload.Id,
                GroupId = bulkUpload.GroupId,
                DateOfProgramsToIngest = p.Key,
                ProgramsToIngest = p.ToList(),
                RequestId = KLogger.GetRequestId(),
            });

            // Lock all dates before starting the ingest
            var jobData = bulkUpload.JobData as BulkUploadIngestJobData;
            var locker = new DistributedLock(bulkUpload.GroupId);
            var epgV2Config = ApplicationConfiguration.Current.EPGIngestV2Configuration;

            var isLocked = locker.Lock(jobData.LockKeys, epgV2Config.LockNumOfRetries.Value, epgV2Config.LockRetryIntervalMS.Value, epgV2Config.LockTTLSeconds.Value, $"BulkUpload_{bulkUpload.Id}");
           
            if (!isLocked) { throw new Exception("Failed to aquire lock on ingest dates"); }
            publisher.Publish(bulkUploadIngestEvents);
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, int index, List<Status> errorStatusDetails)
        {
            throw new NotImplementedException();
        }

        public override IBulkUploadStructureManager GetStructureManager()
        {
            throw new NotImplementedException();
        }
    }
}