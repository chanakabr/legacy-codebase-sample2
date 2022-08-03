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

        [JsonProperty("bookmarkEventThreshold")]
        public int? BookmarkEventThreshold { get; set; }

        [JsonProperty("ProductType")]
        public eTransactionType? ProductType { get; set; }

        [JsonProperty("ProductId")]
        public int? ProductId { get; set; }

        [JsonProperty("Revoke")]
        public bool? Revoke { get; set; }

        [JsonProperty("linearWatchHistoryThreshold")]
        public int? LinearWatchHistoryThreshold { get; set; }

        [JsonProperty("isFree")]
        public bool? IsFree { get; set; }

        public DevicePlayData()
        {
            // default values to members from joker version
            playType = ePlayType.MEDIA.ToString();
            NpvrId = string.Empty;
        }
        
        public DevicePlayData(string udid, int assetID, int userId, long timeStamp, ePlayType playType, MediaPlayActions action, 
                              int deviceFamilyId, long createdAt, long programId, string npvrId, int domainId, List<int> mediaConcurrencyRuleIds = null,
                              List<long> assetMediaConcurrencyRuleIds = null, List<long> assetEpgConcurrencyRuleIds = null, 
                              int? bookmarkEventThreshold = null, eTransactionType? productType = null, int? productId = null, int? linearWatchHistoryThreshold = null,
                              bool? isFree = null)
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
            this.BookmarkEventThreshold = bookmarkEventThreshold;
            this.ProductType = productType;
            this.ProductId = productId;
            this.LinearWatchHistoryThreshold = linearWatchHistoryThreshold;
            this.IsFree = isFree;
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
            this.BookmarkEventThreshold = other.BookmarkEventThreshold;
            this.ProductType = other.ProductType;
            this.ProductId = other.ProductId;
            LinearWatchHistoryThreshold = other.LinearWatchHistoryThreshold;
            this.IsFree = other.IsFree;
        }

        public UserMediaMark ConvertToUserMediaMark(int location, int fileDuration, int assetTypeId, eAssetTypes assetType)
        {
            return new UserMediaMark()
            {
                UDID = this.UDID,
                AssetID = this.AssetId,
                UserID = this.UserId,
                Location = location,
                CreatedAt = Utils.UtcUnixTimestampSecondsToDateTime(this.CreatedAt),
                NpvrID = this.NpvrId,
                playType = this.playType,
                FileDuration = fileDuration,
                AssetAction = this.AssetAction,
                AssetTypeId = assetTypeId,
                CreatedAtEpoch = this.CreatedAt,
                MediaConcurrencyRuleIds = this.MediaConcurrencyRuleIds,
                AssetType = assetType
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
            StringBuilder sb = new StringBuilder("DevicePlayData Details:");
            sb.AppendLine(String.Concat("UDID: ", this.UDID));
            sb.AppendLine(String.Concat("AssetId: ", this.AssetId));
            sb.AppendLine(String.Concat("UserId: ", this.UserId));
            sb.AppendLine(String.Concat("TimeStamp: ", this.TimeStamp));
            sb.AppendLine(String.Concat("playType: ", this.playType));
            sb.AppendLine(String.Concat("AssetAction: ", this.AssetAction));
            sb.AppendLine(String.Concat("DeviceFamilyId: ", this.DeviceFamilyId));
            sb.AppendLine(String.Concat("CreatedAt: ", this.CreatedAt));
            sb.AppendLine(String.Concat("ProgramId: ", this.ProgramId));
            sb.AppendLine(String.Concat("NpvrId: ", this.NpvrId));
            sb.AppendLine(String.Concat("DomainId: ", this.DomainId));
            sb.AppendLine(String.Concat("MediaConcurrencyRuleIds: ", this.MediaConcurrencyRuleIds == null ? "null" : string.Join(", ", this.MediaConcurrencyRuleIds)));
            sb.AppendLine(String.Concat("AssetMediaConcurrencyRuleIds: ", this.AssetMediaConcurrencyRuleIds == null ? "null" : string.Join(", ", this.AssetMediaConcurrencyRuleIds)));
            sb.AppendLine(String.Concat("AssetEpgConcurrencyRuleIds: ", this.AssetEpgConcurrencyRuleIds == null ? "null" : string.Join(", ", this.AssetEpgConcurrencyRuleIds)));
            sb.AppendLine(String.Concat("PlayCycleKey: ", this.PlayCycleKey));
            sb.AppendLine(String.Concat("bookmarkEventThreshold: ", this.BookmarkEventThreshold));
            sb.AppendLine(String.Concat("ProductType: ", this.ProductType));
            sb.AppendLine(String.Concat("ProductId: ", this.ProductId));
            sb.AppendLine($"{nameof(LinearWatchHistoryThreshold)}: {LinearWatchHistoryThreshold}");

            return sb.ToString();
        }
    }
}

