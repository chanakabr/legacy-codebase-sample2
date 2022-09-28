using Newtonsoft.Json;
using System;
using OTT.Lib.MongoDB;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadAssetResult : BulkUploadResult
    {
        [JsonProperty("Type")] public int? Type { get; set; }

        [JsonProperty("ExternalId")] public string ExternalId { get; set; }

        [JsonProperty("EntryId")] public string EntryId { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadMediaAssetResult : BulkUploadAssetResult
    {
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    [MongoDbIgnoreExternalElements]
    public class BulkUploadProgramAssetResult : BulkUploadResult
    {
        [Obsolete("Use ObjectId instead of ProgramId. ProgramId is always null.")]
        public int? ProgramId { get; set; }
        public long LiveAssetId { get; set; }
        public int ChannelId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string ProgramExternalId { get; set; }

        public override string ToString()
        {
            var currentStr = $"time:[{PrettyFormatDateRange(StartDate, EndDate)}], exId:[{ProgramExternalId}] ";
            var baseStr = base.ToString();
            return currentStr + baseStr;
        }

        private string PrettyFormatDateRange(DateTime start, DateTime end)
        {
            var startStr = $"{start:yyyy-MM-dd HH:mm}";
            var endStr = (start.Date != end.Date) ? $"{end:yyyy-MM-dd HH:mm}" : $"{end:HH:mm}";
            return $"{startStr} - {endStr}";
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadLiveAssetResult : BulkUploadMediaAssetResult
    {
    }
}