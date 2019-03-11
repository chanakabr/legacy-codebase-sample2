using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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

        [JsonProperty("Warnings")]
        public List<Status> Warnings { get; set; }

        public BulkUploadResult()
        {
            Index = -1;
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("BulkUploadId:{0}, Index:{1}, Status:{2}", BulkUploadId, Index, Status);

            if (ObjectId.HasValue)
            {
                sb.AppendFormat("ObjectId:{0}", ObjectId);
            }

            if (ErrorCode.HasValue)
            {
                sb.AppendFormat("ErrorCode:{0}", ErrorCode);
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                sb.AppendFormat("ErrorMessage:{0}", ErrorMessage);
            }

            return sb.ToString();
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