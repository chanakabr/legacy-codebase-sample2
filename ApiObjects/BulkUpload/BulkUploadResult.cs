using ApiObjects.Response;
using Newtonsoft.Json;
using System;

namespace ApiObjects.BulkUpload
{
    public enum BulkUploadResultStatus
    {
        Error = 1,
        Ok = 2,
        InProgress = 3
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

        /// <summary>
        /// Set the status to Error and update error code and message
        /// </summary>
        /// <param name="errorStatus"></param>
        public void SetError(Status errorStatus)
        {
            this.Status = BulkUploadResultStatus.Error;
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