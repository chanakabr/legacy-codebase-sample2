using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadAssetData : BulkUploadObjectData
    {
        protected AssetStruct structure { get; private set; }

        [JsonProperty("TypeId")]
        public long TypeId { get; set; }
        
        public override IBulkUploadStructure GetStructure()
        {
            if (structure == null)
            {
                var assetStructResponse = CatalogManager.GetAssetStruct(GroupId, TypeId);
                if (assetStructResponse.HasObject())
                {
                    structure = assetStructResponse.Object;
                    if (structure.TopicsMapBySystemName == null || structure.TopicsMapBySystemName.Count == 0)
                    {
                        CatalogGroupCache catalogGroupCache;
                        if (CatalogManager.TryGetCatalogGroupCacheFromCache(GroupId, out catalogGroupCache))
                        {
                            structure.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => structure.MetaIds.Contains(x.Key))
                                                              .OrderBy(x => structure.MetaIds.IndexOf(x.Key))
                                                              .ToDictionary(x => x.Value.SystemName, y => y.Value);
                        }
                    }
                }
            }

            return structure;
        }

        public override string DistributedTask { get { return "distributed_tasks.process_bulk_upload_media_asset"; } }
        public override string RoutingKey { get { return "PROCESS_BULK_UPLOAD_MEDIA_ASSET\\{0}"; } }

        public override IBulkUploadObject CreateObjectInstance()
        {
            var bulkObject = Activator.CreateInstance(typeof(MediaAsset)) as MediaAsset;
            return bulkObject;
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, int index, Status errorStatus)
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

            if (errorStatus != null)
            {
                bulkUploadAssetResult.AddError(errorStatus);
            }
            return bulkUploadAssetResult;
        }

        public override Dictionary<string, object> GetMandatoryPropertyToValueMap()
        {
            Dictionary<string, object> mandatoryPropertyToValueMap = new Dictionary<string, object>();
            // set structure if null
            GetStructure();

            if (structure != null)
            {
                var mediaAssetTypeColumnName = ExcelColumn.GetFullColumnName(MediaAsset.MEDIA_ASSET_TYPE, null, null, true);
                mandatoryPropertyToValueMap.Add(mediaAssetTypeColumnName, structure.SystemName);
            }

            return mandatoryPropertyToValueMap;
        }

        public override void EnqueueObjects(BulkUpload bulkUpload, List<BulkUploadResult> results)
        {
            for (var i = 0; i < results.Count; i++)
            {
                var mediaAsset = results[i].Object as MediaAsset;
                if (results[i].Status != BulkUploadResultStatus.Error && mediaAsset != null)
                {
                    // Enqueue to CeleryQueue current bulkUploadObject (the remote will handle each bulkUploadObject in separate).
                    GenericCeleryQueue queue = new GenericCeleryQueue();
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

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadMediaAssetData : BulkUploadAssetData
    {
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadEpgAssetData : BulkUploadAssetData
    {
        // TODO: Arthur, remove disterbutedTask and ruting key from media assets and use the event bus instead.
        public override string DistributedTask { get { return "disterbuted task not supported for epg ingest, use event bus instead"; } }
        public override string RoutingKey { get { return "disterbuted task not supported for epg ingest, use event bus instead"; } }

        public override IBulkUploadObject CreateObjectInstance()
        {
            var bulkObject = Activator.CreateInstance(typeof(EpgAsset)) as EpgAsset;
            return bulkObject;
        }
        
        public override void EnqueueObjects(BulkUpload bulkUpload, List<BulkUploadResult> objects)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, object> GetMandatoryPropertyToValueMap()
        {
            throw new NotImplementedException();
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, int index, Status errorStatus)
        {
            throw new NotImplementedException();
        }
    }
}