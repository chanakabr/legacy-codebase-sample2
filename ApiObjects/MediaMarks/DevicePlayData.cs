using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using ODBCWrapper;
using ApiObjects.Catalog;

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

        [JsonProperty("timeStamp")]
        public long TimeStamp { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("mediaConcurrencyRuleIds")]
        public List<int> MediaConcurrencyRuleIds { get; set; }

        [JsonProperty("assetMediaConcurrencyRuleIds")]
        public List<long> AssetMediaConcurrencyRuleIds { get; set; }

        [JsonProperty("assetEpgConcurrencyRuleIds")]
        public List<long> AssetEpgConcurrencyRuleIds { get; set; }

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
        
        public DevicePlayData(string udid, int assetID, int userId, long timeStamp, ePlayType playType, MediaPlayActions action, 
                              int deviceFamilyId, long createdAt,long programId, string npvrId)
        {
            this.UDID = udid;
            this.AssetId = assetID;
            this.UserId = userId;
            this.TimeStamp = timeStamp;
            this.playType = playType.ToString();
            this.AssetAction = action.ToString();
            this.DeviceFamilyId = deviceFamilyId;
            this.CreatedAt = createdAt;
            this.ProgramId = programId;
            this.NpvrId = npvrId;
        }

        public UserMediaMark ConvertToUserMediaMark(int location, int fileDuration, int assetTypeId)
        {
            return new UserMediaMark()
            {
                UDID = this.UDID,
                AssetID = this.AssetId,
                UserID = this.UserId,
                playType = this.playType,
                AssetAction = this.AssetAction,
                AssetTypeId = assetTypeId,
                CreatedAt = Utils.UnixTimestampToDateTime(this.CreatedAt),
                CreatedAtEpoch = this.TimeStamp,
                MediaConcurrencyRuleIds = this.MediaConcurrencyRuleIds,
                AssetMediaConcurrencyRuleIds = this.AssetMediaConcurrencyRuleIds,
                AssetEpgConcurrencyRuleIds = this.AssetEpgConcurrencyRuleIds,
                DeviceFamilyId = this.DeviceFamilyId,
                Location = location,
                NpvrID = this.NpvrId,
                FileDuration = fileDuration
            };
        }
    }
}

