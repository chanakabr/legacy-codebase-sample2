using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Base;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadDynamicListData : BulkUploadObjectData
    {
        [JsonProperty("DynamicListId")]
        public long DynamicListId { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadUdidDynamicListData : BulkUploadDynamicListData
    {
        private UdidDynamicList structureManager { get; set; }

        public override string DistributedTask { get { return "disterbuted task not supported for udid dynamicList, use event bus instead"; } }
        public override string RoutingKey { get { return "disterbuted task not supported for udid dynamicList, use event bus instead"; } }

        private static readonly Type bulkUploadObjectType = typeof(UdidDynamicList);
        public override Type GetObjectType()
        {
            return bulkUploadObjectType;
        }

        public override IBulkUploadStructureManager GetStructureManager()
        {
            if (structureManager == null)
            {
                var contextData = new ContextData(this.GroupId);
                var dynamicListResponse = DynamicListManager.Instance.Get(contextData, this.DynamicListId);
                if (dynamicListResponse.HasObject() && dynamicListResponse.Object.Type == DynamicListType.UDID)
                {
                    structureManager = (UdidDynamicList)dynamicListResponse.Object;
                }
            }

            return structureManager;
        }

        // TODO SHIR - BulkUploadUdidDynamicListData.EnqueueObjects
        // directly set in db and not send to rabbit again..
        public override void EnqueueObjects(BulkUpload bulkUpload, List<BulkUploadResult> results)
        {
            var list = new List<int>();

            //var udidList = results.Select(x => x.Object as )

            //-------------------------

            //for (var i = 0; i < results.Count; i++)
            //{
            //    var mediaAsset = results[i].Object as MediaAsset;
            //    if (results[i].Status != BulkUploadResultStatus.Error && mediaAsset != null)
            //    {
            //        // Enqueue to CeleryQueue current bulkUploadObject (the remote will handle each bulkUploadObject in separate).
            //        var queue = new GenericCeleryQueue();
            //        var data = new BulkUploadItemData<MediaAsset>(this.DistributedTask, bulkUpload.GroupId, bulkUpload.UpdaterId, bulkUpload.Id, bulkUpload.Action, i, mediaAsset);
            //        if (queue.Enqueue(data, string.Format(this.RoutingKey, bulkUpload.GroupId)))
            //        {
            //            log.DebugFormat("Success enqueue bulkUploadObject. bulkUploadId:{0}, resultIndex:{1}", bulkUpload.Id, i);
            //        }
            //        else
            //        {
            //            log.DebugFormat("Failed enqueue bulkUploadObject. bulkUploadId:{0}, resultIndex:{1}", bulkUpload.Id, i);
            //        }
            //    }
            //}
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, int index, List<Status> errorStatusDetails)
        {
            // We know for sure this should be a UdidDynamicList if not we want an exception here
            var udidDynamicList = (UdidDynamicList)bulkUploadObject;

            var result = new BulkUploadDynamicListResult()
            {
                Index = index,
                ObjectId = udidDynamicList.Id > 0 ? udidDynamicList.Id : (long?)null,
                BulkUploadId = bulkUploadId,
                Status = BulkUploadResultStatus.InProgress,
                Object = bulkUploadObject
            };

            if (errorStatusDetails != null)
            {
                result.AddErrors(errorStatusDetails);
            }
            return result;
        }
    }
}
