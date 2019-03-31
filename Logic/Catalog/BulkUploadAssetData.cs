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
    public abstract class BulkUploadAssetData : BulkUploadObjectData
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
    }

    public class BulkUploadMediaAssetData : BulkUploadAssetData
    {
        public override string DistributedTask { get { return "distributed_tasks.process_bulk_upload_media_asset"; } }
        public override string RoutingKey { get { return "PROCESS_BULK_UPLOAD_MEDIA_ASSET\\{0}"; } }

        public override IBulkUploadObject CreateObjectInstance()
        {
            var bulkObject = Activator.CreateInstance(typeof(MediaAsset)) as MediaAsset;
            return bulkObject;
        }
        
        public override bool EnqueueBulkUploadResult(BulkUpload bulkUpload, int resultIndex, IBulkUploadObject bulkUploadObject)
        {
            var mediaAsset = bulkUploadObject as MediaAsset;
            if (mediaAsset != null)
            {
                GenericCeleryQueue queue = new GenericCeleryQueue();
                var data = new BulkUploadItemData<MediaAsset>(this.DistributedTask, bulkUpload.GroupId, bulkUpload.UpdaterId, bulkUpload.Id, bulkUpload.Action, resultIndex, mediaAsset);
                bool enqueueSuccessful = queue.Enqueue(data, string.Format(this.RoutingKey, bulkUpload.GroupId));

                return enqueueSuccessful;
            }

            return false;
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, BulkUploadResultStatus status, int index, Status errorStatus)
        {
            var mediaAsset = bulkUploadObject as MediaAsset;
            if (mediaAsset != null)
            {
                BulkUploadMediaAssetResult bulkUploadAssetResult = new BulkUploadMediaAssetResult()
                {
                    Index = index,
                    ObjectId = mediaAsset.Id > 0 ? mediaAsset.Id : (long?)null,
                    BulkUploadId = bulkUploadId,
                    Status = status,
                    Type = mediaAsset.MediaType != null && mediaAsset.MediaType.m_nTypeID > 0 ? mediaAsset.MediaType.m_nTypeID : (int?)null,
                    ExternalId = string.IsNullOrEmpty(mediaAsset.CoGuid) ? null : mediaAsset.CoGuid
                };

                if (errorStatus != null)
                {
                    bulkUploadAssetResult.SetError(errorStatus);
                }
                return bulkUploadAssetResult;
            }

            return null;
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
    }
    
    public class BulkUploadEpgAssetData : BulkUploadAssetData
    {
        public override string DistributedTask { get { throw new NotImplementedException(); } }
        public override string RoutingKey { get { throw new NotImplementedException(); } }

        public override IBulkUploadObject CreateObjectInstance()
        {
            var bulkObject = Activator.CreateInstance(typeof(EpgAsset)) as EpgAsset;
            return bulkObject;
        }

        public override bool EnqueueBulkUploadResult(BulkUpload bulkUpload, int resultIndex, IBulkUploadObject bulkUploadObject)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, object> GetMandatoryPropertyToValueMap()
        {
            throw new NotImplementedException();
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, BulkUploadResultStatus status, int index, Status errorStatus)
        {
            throw new NotImplementedException();
        }
    }
}