using ApiObjects.Response;
using Newtonsoft.Json;
using System;

namespace ApiObjects.Catalog
{
    public enum BulkUploadResultStatus
    {
        ERROR = 1,
        OK = 2,
        IN_PROGRESS = 3
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadResult
    {
        // can be assetId, userId etc
        [JsonProperty("ObjectId")]
        public long? ObjectId { get; set; }

        [JsonProperty("Index")]
        public int Index { get; set; }

        [JsonProperty("BulkUploadId")]
        public long BulkUploadId { get; set; }

        [JsonProperty("Status")]
        public BulkUploadResultStatus Status { get; set; }

        [JsonProperty("ErrorCode")]
        public int? ErrorCode { get; private set; }

        [JsonProperty("ErrorMessage")]
        public string ErrorMessage { get; private set; }

        public BulkUploadResult()
        {
            Index = -1;
        }
        
        public override string ToString()
        {
            // TODO SHIR - BulkUploadResult ToString
            return base.ToString();
        }

        public void SetError(Status errorStatus)
        {
            this.Status = BulkUploadResultStatus.ERROR;
            if (errorStatus != null)
            {
                this.ErrorCode = errorStatus.Code;
                this.ErrorMessage = errorStatus.Message;
            }
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadAssetResult : BulkUploadResult
    {
        [JsonProperty("Type")]
        public int? Type { get; set; }

        [JsonProperty("ExternalId")]
        public string ExternalId { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadMediaAssetResult : BulkUploadAssetResult
    {
    }
}
