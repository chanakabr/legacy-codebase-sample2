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

        [JsonProperty("FileName")]
        public string FileName { get; set; }

        [JsonProperty("Status")]
        public BulkUploadJobStatus Status { get; set; }

        [JsonProperty("Action")]
        public BulkUploadJobAction Action { get; set; }

        [JsonProperty("NumOfObjects")]
        public int? NumOfObjects { get; set; }

        [JsonProperty("GroupId")]
        public long GroupId { get; set; }

        [JsonProperty("FileType")]
        public FileType FileType { get; set; }

        [JsonProperty("CreateDate")]
        public DateTime CreateDate { get; set; }

        [JsonProperty("UpdateDate")]
        public DateTime UpdateDate { get; set; }
        
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

    // TODO SHIR - CREATE NEW STATUS BY SPEC
    public enum BulkUploadJobStatus
    {
        PENDING = 1,
        UPLOADED = 2,
        QUEUED = 3,
        //PROCESSING = 2,
        //PROCESSED = 3,
        //MOVEFILE = 4,
        //FINISHED = 5,
        //FAILED = 6,
        //ABORTED = 7,
        //RETRY = 9,
        //FATAL = 10,
        //DONT_PROCESS = 11,
        //FINISHED_PARTIALLY = 12
    }

    public enum BulkUploadJobAction
    {
        Upsert = 1,
        Delete = 2
    }

    public enum FileType
    {
        Excel = 1,
        Xml = 2
    }
}