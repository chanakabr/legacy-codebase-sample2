using ApiObjects.BulkUpload;
using Core.Catalog;
using Newtonsoft.Json;
using System;

namespace MediaAssetBulkUploadHandler
{
    [Serializable]
    public class MediaAssetBulkUploadRequest
    {
        [JsonProperty("group_id")]
        public int GroupID { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

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