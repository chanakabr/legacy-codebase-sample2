using ApiObjects.BulkUpload;
using Core.Catalog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Core.Catalog
{
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

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadEpgAssetResult : BulkUploadAssetResult
    {
        public List<BulkUploadChannelResult> Channels { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadChannelResult : BulkUploadResult
    {
    }
}
