using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using ODBCWrapper;

namespace ApiObjects.MediaMarks
{
    [Serializable]
    public class DevicePlayData
    {
        [JsonProperty("udid")]
        public string UDID { get; set; }

        [JsonProperty("mid")]
        public int AssetId { get; set; }

        [JsonProperty("uid")]
        public int UserId { get; set; }

        [JsonProperty("playType", Required = Required.Default)]
        public string playType { get; set; }

        [JsonProperty("action")]
        public string AssetAction { get; set; }

        [JsonProperty("assetTypeId")]
        public int AssetTypeId { get; set; }

        [JsonProperty("timeStamp")]
        public long TimeStamp { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("mediaConcurrencyRuleIds")]
        public List<int> MediaConcurrencyRuleIds { get; set; }
        
        [JsonProperty("assetConcurrencyRuleIds")]
        public List<long> AssetConcurrencyRuleIds { get; set; }

        [JsonProperty("deviceFamilyId")]
        public int DeviceFamilyId { get; set; }

        [JsonProperty("npvrId", Required = Required.Default)]
        public string NpvrId { get; set; }

        [JsonProperty("programId")]
        public long ProgramId { get; set; }

        public DevicePlayData()
        {
            // default values to members from joker version
            playType = ePlayType.MEDIA.ToString();
            NpvrId = string.Empty;
        }

        public UserMediaMark ConvertToUserMediaMark(int location, int fileDuration)
        {
            return new UserMediaMark()
            {
                UDID = this.UDID,
                AssetID = this.AssetId,
                UserID = this.UserId,
                playType = this.playType,
                AssetAction = this.AssetAction,
                AssetTypeId = this.AssetTypeId,
                CreatedAt = Utils.UnixTimestampToDateTime(this.CreatedAt),
                CreatedAtEpoch = this.TimeStamp,
                MediaConcurrencyRuleIds = this.MediaConcurrencyRuleIds,
                AssetConcurrencyRuleIds = this.AssetConcurrencyRuleIds,
                DeviceFamilyId = this.DeviceFamilyId,
                Location = location,
                NpvrID = this.NpvrId,
                FileDuration = fileDuration
            };
        }
    }
}

