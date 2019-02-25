using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUpload
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("UploadTokenId")]
        public string UploadTokenId { get; set; }

        [JsonProperty("CreateDate")]
        public long CreateDate { get; set; }

        [JsonProperty("UpdateDate")]
        public long UpdateDate { get; set; }

        [JsonProperty("GroupId")]
        public int GroupId { get; set; }

        [JsonProperty("FileType")]
        public FileType FileType { get; set; }

        [JsonProperty("Status")]
        public BulkUploadJobStatus Status { get; set; }

        [JsonProperty("Action")]
        public BulkUploadJobAction Action { get; set; }

        // TODO SHIR - ADD THIS TO DB table and dr and kaltura objects
        [JsonProperty("TotalCountOfResults")]
        public int TotalCountOfResults { get; set; }

        //public List<T> Results { get; set; }
        [JsonProperty(PropertyName = "Results",
                     TypeNameHandling = TypeNameHandling.Auto,
                     ItemTypeNameHandling = TypeNameHandling.Auto,
                     ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<BulkUploadResult> Results { get; set; }

        public BulkUpload()
        {
            Results = new List<BulkUploadResult>();
        }

        public BulkUpload(long Id, string UploadTokenId) : base()
        {
        }
    }

    public enum BulkUploadJobStatus
    {
        PENDING = 0,
        QUEUED = 1,
        PROCESSING = 2,
        PROCESSED = 3,
        MOVEFILE = 4,
        FINISHED = 5,
        FAILED = 6,
        ABORTED = 7,
        RETRY = 9,
        FATAL = 10,
        DONT_PROCESS = 11,
        FINISHED_PARTIALLY = 12,
        FINISHED_WITH_NO_OBJECTS = 13
    }

    public enum BulkUploadJobAction
    {
        Upsert = 0,
        Delete = 1
    }

    public enum FileType
    {
        Excel = 0,
        Xml = 1
    }
}