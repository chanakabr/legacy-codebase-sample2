using ApiObjects.Response;
using Newtonsoft.Json;
using System;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadJobData
    {
        public abstract GenericListResponse<BulkUploadResult> Deserialize(int groupId, long bulkUploadId, string fileUrl, BulkUploadObjectData objectData);
    }
}