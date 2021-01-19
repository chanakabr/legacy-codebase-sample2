using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadDynamicListResult : BulkUploadResult
    {
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadUdidDynamicListResult : BulkUploadDynamicListResult
    {
        [JsonProperty("Udid")]
        public string Udid { get; set; }
    }

}
