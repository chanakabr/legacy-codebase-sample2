using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Newtonsoft.Json;
using QueueWrapper;
using System;
using System.Collections.Generic;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadMediaAssetData : BulkUploadAssetData
    {
        public override string DistributedTask { get { return "distributed_tasks.process_bulk_upload_media_asset"; } }
        public override string RoutingKey { get { return "PROCESS_BULK_UPLOAD_MEDIA_ASSET\\{0}"; } }

        private static readonly Type bulkUploadObjectType = typeof(MediaAsset);

        public override Type GetObjectType()
        {
            return bulkUploadObjectType;
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, int index, List<Status> errorStatusDetails)
        {
            // We know for sure this should be a MediaAsset if not we want an exception here
            var mediaAsset = (MediaAsset)bulkUploadObject;

            var bulkUploadAssetResult = new BulkUploadMediaAssetResult()
            {
                Index = index,
                ObjectId = mediaAsset.Id > 0 ? mediaAsset.Id : (long?)null,
                BulkUploadId = bulkUploadId,
                Status = BulkUploadResultStatus.InProgress,
                Type = mediaAsset.MediaType != null && mediaAsset.MediaType.m_nTypeID > 0 ? mediaAsset.MediaType.m_nTypeID : (int?)null,
                ExternalId = string.IsNullOrEmpty(mediaAsset.CoGuid) ? null : mediaAsset.CoGuid,
                Object = bulkUploadObject
            };

            if (errorStatusDetails != null)
            {
                bulkUploadAssetResult.AddErrors(errorStatusDetails);
            }
            return bulkUploadAssetResult;
        }
        
        public override void EnqueueObjects(BulkUpload bulkUpload, List<BulkUploadResult> results)
        {
            for (var i = 0; i < results.Count; i++)
            {
                var mediaAsset = results[i].Object as MediaAsset;
                if (results[i].Status != BulkUploadResultStatus.Error && mediaAsset != null)
                {
                    var eventBus = EventBus.RabbitMQ.EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration();
                    var serviceEvent = new MediaAssetBulkUploadRequest()
                    {
                        GroupId = bulkUpload.GroupId,
                        BulkUploadId = bulkUpload.Id,
                        JobAction = bulkUpload.Action,
                        ObjectData = mediaAsset,
                        ResultIndex = i,
                        UserId = bulkUpload.UpdaterId
                    };

                    eventBus.Publish(serviceEvent);

                    // Enqueue to CeleryQueue current bulkUploadObject (the remote will handle each bulkUploadObject in separate).
                    var queue = new GenericCeleryQueue();
                    var data = new BulkUploadItemData<MediaAsset>(this.DistributedTask, bulkUpload.GroupId, bulkUpload.UpdaterId, bulkUpload.Id, bulkUpload.Action, i, mediaAsset);
                    if (queue.Enqueue(data, string.Format(this.RoutingKey, bulkUpload.GroupId)))
                    {
                        log.DebugFormat("Success enqueue bulkUploadObject. bulkUploadId:{0}, resultIndex:{1}", bulkUpload.Id, i);
                    }
                    else
                    {
                        log.DebugFormat("Failed enqueue bulkUploadObject. bulkUploadId:{0}, resultIndex:{1}", bulkUpload.Id, i);
                    }
                }
            }
        }
    }
}
