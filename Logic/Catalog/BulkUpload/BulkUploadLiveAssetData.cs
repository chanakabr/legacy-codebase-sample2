using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using QueueWrapper;
using ApiObjects;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadLiveAssetData : BulkUploadMediaAssetData
    {
        public override string DistributedTask { get { return "distributed_tasks.process_bulk_upload_live_asset"; } }
        public override string RoutingKey { get { return "PROCESS_BULK_UPLOAD_LIVE_ASSET\\{0}"; } }

        private static readonly Type bulkUploadObjectType = typeof(LiveAsset);
        
        public override Type GetObjectType()
        {
            return bulkUploadObjectType;
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, int index, List<Status> errorStatusDetails)
        {
            // We know for sure this should be a LiveAsset if not we want an exception here
            var liveAsset = (LiveAsset)bulkUploadObject;
            var externalId = string.IsNullOrEmpty(liveAsset.CoGuid) ? null : liveAsset.CoGuid;
            liveAsset.ExternalEpgIngestId = externalId;
            liveAsset.EnableCatchUpState = TstvState.Inherited;
            liveAsset.EnableCdvrState = TstvState.Inherited;
            liveAsset.EnableRecordingPlaybackNonEntitledChannelState = TstvState.Inherited;
            liveAsset.EnableStartOverState = TstvState.Inherited;
            liveAsset.EnableTrickPlayState = TstvState.Inherited;
            liveAsset.BufferCatchUp = 0;
            liveAsset.BufferTrickPlay = 0;
            liveAsset.ChannelType = LinearChannelType.Unknown;

            var accountTstvSettings = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(this.GroupId);
            liveAsset.FillEnabledAndBufferProperties(accountTstvSettings);
            
            var bulkUploadAssetResult = new BulkUploadLiveAssetResult()
            {
                Index = index,
                ObjectId = liveAsset.Id > 0 ? liveAsset.Id : (long?)null,
                BulkUploadId = bulkUploadId,
                Status = BulkUploadResultStatus.InProgress,
                Type = liveAsset.MediaType != null && liveAsset.MediaType.m_nTypeID > 0 ? liveAsset.MediaType.m_nTypeID : (int?)null,
                ExternalId = externalId,
                Object = liveAsset
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
                var liveAsset = results[i].Object as LiveAsset;
                if (results[i].Status != BulkUploadResultStatus.Error && liveAsset != null)
                {
                    var queue = new GenericCeleryQueue();
                    var data = new BulkUploadItemData<LiveAsset>(this.DistributedTask, bulkUpload.GroupId, bulkUpload.UpdaterId, bulkUpload.Id, bulkUpload.Action, results[i].Index, liveAsset);
                    if (queue.Enqueue(data, string.Format(this.RoutingKey, bulkUpload.GroupId)))
                    {
                        log.Debug($"Success enqueue live asset bulkUploadObject. bulkUploadId:{bulkUpload.Id}, resultIndex:{results[i].Index}");
                    }
                    else
                    {
                        log.Debug($"Failed enqueue live asset bulkUploadObject. bulkUploadId:{bulkUpload.Id}, resultIndex:{results[i].Index}");
                    }
                }
            }
        }
    }
}
