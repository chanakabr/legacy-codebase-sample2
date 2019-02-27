using ApiObjects.Catalog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaAssetBulkUploadHandler
{
    [Serializable]
    public class BulkUploadRequest
    {
        [JsonProperty("group_id")]
        public int GroupID { get; set; }

        [JsonProperty("bulk_upload_id")]
        public long BulkUploadId { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }
    }
}
