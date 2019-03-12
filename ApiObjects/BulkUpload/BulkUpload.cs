using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.BulkUpload
{
    public enum BulkUploadJobStatus
    {
        Pending = 1,
        Uploaded = 2,
        Queued = 3,
        Parsing = 4,
        Processing = 5,
        Processed = 6,
        Success = 7,
        Partial = 8,
        Failed = 9,
        Fatal = 10
    }

    public enum BulkUploadJobAction
    {
        Upsert = 1,
        Delete = 2
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUpload
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("FileURL")]
        public string FileURL { get; set; }

        [JsonProperty("FileName")]
        public string FileName { get; set; }

        [JsonProperty("BulkObjectType")]
        public string BulkObjectType { get; set; }

        [JsonProperty("Status")]
        public BulkUploadJobStatus Status { get; set; }

        [JsonProperty("Action")]
        public BulkUploadJobAction Action { get; set; }

        [JsonProperty("NumOfObjects")]
        public int? NumOfObjects { get; set; }

        [JsonProperty("GroupId")]
        public int GroupId { get; set; }
        
        [JsonProperty("CreateDate")]
        public DateTime CreateDate { get; set; }

        [JsonProperty("UpdateDate")]
        public DateTime UpdateDate { get; set; }

        [JsonProperty("UpdaterId")]
        public long UpdaterId { get; set; }
        
        [JsonProperty(PropertyName = "Results",
                     TypeNameHandling = TypeNameHandling.Auto,
                     ItemTypeNameHandling = TypeNameHandling.Auto,
                     ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<BulkUploadResult> Results { get; set; }

        [JsonProperty("JobData")]
        public BulkUploadJobData JobData { get; set; }

        [JsonProperty("ObjectData")]
        public BulkUploadObjectData ObjectData { get; set; }

        public BulkUpload()
        {
            Results = new List<BulkUploadResult>();
        }
    }
}