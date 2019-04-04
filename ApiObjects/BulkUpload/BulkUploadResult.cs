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

        [JsonProperty("Errors")]
        public List<Status> Errors { get; private set; }

        [JsonProperty("Warnings")]
        public List<Status> Warnings { get; set; }

        [JsonIgnore()]
        public IBulkUploadObject Object { get; set; }

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
                sb.AppendFormat(", ObjectId:{0}", ObjectId);
            }

            if (Errors != null && Errors.Count > 0)
            {
                for (int i = 0; i < Errors.Count; i++)
                {
                    sb.AppendFormat(", Errors {0}:{1}", i + 1, Errors[i].ToString());
                }
            }

            if (Warnings != null && Warnings.Count > 0)
            {
                for (int i = 0; i < Warnings.Count; i++)
                {
                    sb.AppendFormat(", Warning {0}:{1}", i + 1, Warnings[i].ToString());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Set the status to Error and update error code and message
        /// </summary>
        /// <param name="errorStatus"></param>
        public void AddError(Status errorStatus)
        {
            this.Status = BulkUploadResultStatus.Error;
            if (Errors == null)
            {
                Errors = new List<Status>();
            }

            if (errorStatus != null)
            {
                this.Errors.Add(errorStatus);
            }
        }

        public void AddError(eResponseStatus errorCode, string msg = "")
        {
            var errorStatus = new Status((int)errorCode, msg);
            AddError(errorStatus);
        }
    }
}