using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.Catalog;
using Newtonsoft.Json;

namespace ApiObjects.MediaMarks
{
    [Serializable]
    public class UserMediaMark
    {
        [JsonProperty("udid")]
        public string UDID { get; set; }

        [JsonProperty("mid")]
        public int AssetID { get; set; }

        [JsonProperty("uid")]
        public int UserID { get; set; }

        [JsonProperty("loc")]
        public int Location { get; set; }

        [JsonProperty("ts")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("NpvrID", Required = Required.Default)]
        public string NpvrID { get; set; }

        [JsonProperty("playType", Required = Required.Default)]
        public string playType { get; set; }

        [JsonProperty("duration")]
        public int FileDuration { get; set; }

        [JsonProperty("action")]
        public string AssetAction { get; set; }

        [JsonProperty("assetTypeId")]
        public int AssetTypeId { get; set; }

        [JsonProperty("ts_epoch")]
        public long CreatedAtEpoch { get; set; }

        [JsonProperty("mediaConcurrencyRuleIds")]
        public List<int> MediaConcurrencyRuleIds { get; set; }

        [JsonProperty("assetType")]
        public eAssetTypes AssetType { get; set; }

        [JsonIgnore]
        public long ExpiredAt { get; set; }

        [JsonProperty("LocationTagValue")]
        public int LocationTagValue { get; set; }

        public UserMediaMark()
        {
            // default values to members from joker version
            playType = ePlayType.MEDIA.ToString();
            NpvrID = string.Empty;
        }

        public class UMMDateComparerDesc : IComparer<UserMediaMark>
        {
            public int Compare(UserMediaMark x, UserMediaMark y)
            {
                return y.CreatedAt.CompareTo(x.CreatedAt);
            }
        }

        public class UMMMediaComparer : IComparer<UserMediaMark>
        {
            public int Compare(UserMediaMark x, UserMediaMark y)
            {
                return x.AssetID.CompareTo(y.AssetID);
            }
        }

        public bool IsFinished(int finishedPercentThreshold)
        {
            if (this.AssetAction.ToUpper() == MediaPlayActions.FINISH.ToString().ToUpper())
            {
                return true;
            }
            
            if (this.LocationTagValue > 0 && this.Location >= this.LocationTagValue)
            {
                return true;
            }

            return this.LocationTagValue == 0 && ((float)this.Location / (float)this.FileDuration) * 100 > finishedPercentThreshold;
        }
    }
}