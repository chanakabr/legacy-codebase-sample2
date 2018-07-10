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

        [JsonProperty("domainId")]
        public int DomainId { get; set; }

        [JsonProperty("playCycleKey")]
        public string PlayCycleKey { get; set; }

        public DevicePlayData()
        {
            // default values to members from joker version
            playType = ePlayType.MEDIA.ToString();
            NpvrId = string.Empty;
        }
        
        public DevicePlayData(string udid, int assetID, int userId, long timeStamp, ePlayType playType, MediaPlayActions action, 
                              int deviceFamilyId, long createdAt, long programId, string npvrId, int domainId, List<int> mediaConcurrencyRuleIds,
                              List<long> assetMediaConcurrencyRuleIds, List<long> assetEpgConcurrencyRuleIds)
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
            this.DomainId = domainId;
            this.MediaConcurrencyRuleIds = mediaConcurrencyRuleIds;
            this.AssetMediaConcurrencyRuleIds = assetMediaConcurrencyRuleIds;
            this.AssetEpgConcurrencyRuleIds = assetEpgConcurrencyRuleIds;
            this.PlayCycleKey = Guid.NewGuid().ToString();
        }

        public DevicePlayData(DevicePlayData other)
        {
            this.UDID = other.UDID;
            this.AssetId = other.AssetId;
            this.UserId = other.UserId;
            this.TimeStamp = other.TimeStamp;
            this.playType = other.playType;
            this.AssetAction = other.AssetAction;
            this.DeviceFamilyId = other.DeviceFamilyId;
            this.CreatedAt = other.CreatedAt;
            this.ProgramId = other.ProgramId;
            this.NpvrId = other.NpvrId;
            this.DomainId = other.DomainId;
            this.MediaConcurrencyRuleIds = other.MediaConcurrencyRuleIds;
            this.AssetMediaConcurrencyRuleIds = other.AssetMediaConcurrencyRuleIds;
            this.AssetEpgConcurrencyRuleIds = other.AssetEpgConcurrencyRuleIds;
            this.PlayCycleKey = other.PlayCycleKey;
        }

        public UserMediaMark ConvertToUserMediaMark(int location, int fileDuration, int assetTypeId)
        {
            return new UserMediaMark()
            {
                UDID = this.UDID,
                AssetID = this.AssetId,
                UserID = this.UserId,
                Location = location,
                CreatedAt = Utils.UnixTimestampToDateTime(this.CreatedAt),
                NpvrID = this.NpvrId,
                playType = this.playType,
                FileDuration = fileDuration,
                AssetAction = this.AssetAction,
                AssetTypeId = assetTypeId,
                CreatedAtEpoch = this.TimeStamp,
                MediaConcurrencyRuleIds = this.MediaConcurrencyRuleIds
            };
        }

        public ePlayType GetPlayType()
        {
            ePlayType realePlayType;
            Enum.TryParse(this.playType.ToUpper().Trim(), out realePlayType);
            return realePlayType;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("DevicePlayData DATA: ");
            sb.Append(String.Concat(" UDID: ", this.UDID));
            sb.Append(String.Concat(" Asset Id: ", this.AssetId));
            sb.Append(String.Concat(" User Id: ", this.UserId));
            sb.Append(String.Concat(" TimeStamp: ", this.TimeStamp));
            sb.Append(String.Concat(" playType: ", this.playType));
            sb.Append(String.Concat(" Asset Action: ", this.AssetAction));
            sb.Append(String.Concat(" Device Family Id: ", this.DeviceFamilyId));
            sb.Append(String.Concat(" Create dAt: ", this.CreatedAt));
            sb.Append(String.Concat(" Program Id: ", this.ProgramId));
            sb.Append(String.Concat(" Npvr Id: ", this.NpvrId));
            sb.Append(String.Concat(" Domain Id: ", this.DomainId));
            sb.Append(String.Concat(" Media Concurrency Rule Ids: ", this.MediaConcurrencyRuleIds == null ? "null" : string.Join(", ", this.MediaConcurrencyRuleIds)));
            sb.Append(String.Concat(" Asset Media Concurrency Rule Ids: ", this.AssetMediaConcurrencyRuleIds == null ? "null" : string.Join(", ", this.AssetMediaConcurrencyRuleIds)));
            sb.Append(String.Concat(" Asset Epg Concurrency Rule Ids: ", this.AssetEpgConcurrencyRuleIds == null ? "null" : string.Join(", ", this.AssetEpgConcurrencyRuleIds)));
            sb.Append(String.Concat(" Play Cycle Key: ", this.PlayCycleKey));

            return sb.ToString();
        }
    }
}

