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
using Core.Catalog.CatalogManagement;

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

        // directly set in db and not send to rabbit again
        public override void EnqueueObjects(BulkUpload bulkUpload, List<BulkUploadResult> results)
        {
            var goodResults = results.Where(x => x.Status != BulkUploadResultStatus.Error).Select(x => x as BulkUploadUdidDynamicListResult);
            if(DAL.ApiDAL.SaveUdidDynamicList(this.GroupId, this.DynamicListId, goodResults.Select(x => x.Udid).ToList()))
            {    
                foreach (var udidResult in goodResults)
                {
                    var resultStatus = BulkUploadManager.UpdateBulkUploadResult(this.GroupId, bulkUpload.Id, udidResult.Index, null, udidResult.ObjectId);
                    if (!resultStatus.IsOkStatusCode())
                    {
                        var errorMassage = $"fail to update BulkUploadUdidDynamicListResult BulkUploadId:{bulkUpload.Id}, ResultIndex:{udidResult.Index}, error: {resultStatus}.";
                        log.Error(errorMassage);
                    }
                }
            }

            var badResults = results.Where(x => x.Status == BulkUploadResultStatus.Error).Select(x => x as BulkUploadUdidDynamicListResult);
            foreach (var udidResult in badResults)
            {
                var error = Status.Error;// TODO SHIR - SET ERROR udidResult.Errors  
                var resultStatus = BulkUploadManager.UpdateBulkUploadResult(this.GroupId, bulkUpload.Id, udidResult.Index, error, udidResult.ObjectId);
                if (!resultStatus.IsOkStatusCode())
                {
                    var errorMassage = $"fail to update BulkUploadUdidDynamicListResult BulkUploadId:{bulkUpload.Id}, ResultIndex:{udidResult.Index}, error: {resultStatus}.";
                    log.Error(errorMassage);
                }
            }
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, int index, List<Status> errorStatusDetails)
        {
            // We know for sure this should be a UdidDynamicList if not we want an exception here
            var udidDynamicList = (UdidDynamicList)bulkUploadObject;

            var result = new BulkUploadUdidDynamicListResult()
            {
                Index = index,
                ObjectId = udidDynamicList.Id > 0 ? udidDynamicList.Id : (long?)null,
                BulkUploadId = bulkUploadId,
                Status = BulkUploadResultStatus.InProgress,
                Udid = udidDynamicList.SingileUdidValue
            };

            if (errorStatusDetails != null)
            {
                result.AddErrors(errorStatusDetails);
            }
            return result;
        }
    }
}
