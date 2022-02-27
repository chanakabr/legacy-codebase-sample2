using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ApiObjects.Catalog;
using FeatureFlag;
using Newtonsoft.Json.Converters;

namespace ApiObjects.MediaMarks
{
    public static class MediaMarksNewModel
    {
        public static bool Enabled(int groupId) => PhoenixFeatureFlagInstance.Get().IsMediaMarksNewModel(groupId);
    }

    [JsonObject()]
    [Serializable]
    public class UserMediaMarks
    {
        [JsonProperty("mediaMarks")]
        public List<AssetAndLocation> mediaMarks;

        public UserMediaMarks()
        {
            mediaMarks = new List<AssetAndLocation>();
        }
    }

    public class AssetAndLocation
    {
        [JsonProperty("assetType")] public eAssetTypes AssetType { get; set; }
        [JsonProperty("assetId")] public int AssetId { get; set; }
        [JsonProperty("npvrId")] public string NpvrId { get; set; } = string.Empty;
        
        [JsonProperty("e")] public AssetAndLocationExtra Extra { get; set; }

        [JsonProperty("createdAt")] public long CreatedAt { get; set; }
        [JsonProperty("ExpiredAt")] public long ExpiredAt { get; set; }
    }

    // short names to save network-bandwidth
    public class AssetAndLocationExtra
    {
        [JsonProperty("udid")] public string UDID { get; set; }
        [JsonProperty("atid")] public int AssetTypeId { get; set; }

        [JsonProperty("pt"), JsonConverter(typeof(StringEnumConverter))]
        public ePlayType PlayType { get; set; } = ePlayType.MEDIA;
        
        [JsonProperty("a"), JsonConverter(typeof(StringEnumConverter))]
        public MediaPlayActions AssetAction { get; set; }
        
        [JsonProperty("loc")] public int Location { get; set; }
        [JsonProperty("d")] public int FileDuration { get; set; }
        [JsonProperty("ltv")] public int LocationTagValue { get; set; }
    }
}