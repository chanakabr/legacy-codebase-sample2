using ApiObjects.BulkUpload;
using EventBus.Abstraction;
using Newtonsoft.Json;
using System;

namespace Core.Catalog
{
    [Serializable]
    public class MediaAssetBulkUploadRequest : ServiceEvent
    {
        [JsonProperty("bulk_upload_id")]
        public long BulkUploadId { get; set; }

        [JsonProperty("job_action")]
        public BulkUploadJobAction JobAction { get; set; }

        [JsonProperty("result_index")]
        public int ResultIndex { get; set; }

        [JsonProperty("object_Data")]
        public MediaAsset ObjectData { get; set; }
    }
}