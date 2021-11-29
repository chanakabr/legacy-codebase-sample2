using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public enum KalturaRuleActionType
    {
        BLOCK,
        START_DATE_OFFSET,
        END_DATE_OFFSET,
        USER_BLOCK,
        ALLOW_PLAYBACK,
        BLOCK_PLAYBACK,
        APPLY_DISCOUNT_MODULE,
        APPLY_PLAYBACK_ADAPTER,
        FILTER,
        ASSET_LIFE_CYCLE_TRANSITION,
        APPLY_FREE_PLAYBACK,
        FilterAssetByKsql,
        FilterFileByQualityInDiscovery,
        FilterFileByQualityInPlayback,
        FilterFileByFileTypeIdForAssetTypeInDiscovery,
        FilterFileByFileTypeIdForAssetTypeInPlayback,
        FilterFileByFileTypeIdInDiscovery,
        FilterFileByFileTypeIdInPlayback,
        FilterFileByAudioCodecInDiscovery,
        FilterFileByAudioCodecInPlayback,
        FilterFileByVideoCodecInDiscovery,
        FilterFileByVideoCodecInPlayback,
        FilterFileByStreamerTypeInDiscovery,
        FilterFileByStreamerTypeInPlayback,
        FilterFileByLabelInDiscovery,
        FilterFileByLabelInPlayback
    }
    
    [Serializable]
    public abstract partial class KalturaRuleAction : KalturaOTTObject
    {
        /// <summary>
        /// The type of the action
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaRuleActionType Type { get; protected set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        public virtual void Validate() { }
    }

    [Serializable]
    public abstract partial class KalturaAssetRuleAction : KalturaRuleAction
    {
    }

    [Serializable]
    public abstract partial class KalturaAssetUserRuleAction : KalturaRuleAction
    {
    }

    [Serializable]
    public partial class KalturaAccessControlBlockAction : KalturaAssetRuleAction
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.BLOCK;
        }
    }

    /// <summary>
    /// Time offset action
    /// </summary>
    [Serializable]
    public abstract partial class KalturaTimeOffsetRuleAction : KalturaAssetRuleAction
    {
        /// <summary>
        /// Offset in seconds 
        /// </summary>
        [DataMember(Name = "offset")]
        [JsonProperty("offset")]
        [XmlElement(ElementName = "offset")]
        public int Offset { get; set; }

        /// <summary>
        /// Indicates whether to add time zone offset to the time 
        /// </summary>
        [DataMember(Name = "timeZone")]
        [JsonProperty("timeZone")]
        [XmlElement(ElementName = "timeZone")]
        public bool TimeZone { get; set; }
    }

    /// <summary>
    /// End date offset action
    /// </summary>
    [Serializable]
    public partial class KalturaEndDateOffsetRuleAction : KalturaTimeOffsetRuleAction
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.END_DATE_OFFSET;
        }
    }

    /// <summary>
    /// Start date offset action
    /// </summary>
    [Serializable]
    public partial class KalturaStartDateOffsetRuleAction : KalturaTimeOffsetRuleAction
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.START_DATE_OFFSET;
        }
    }

    public enum KalturaAssetLifeCycleRuleActionType
    {
        ADD = 1,
        REMOVE = 2
    }

    public enum KalturaAssetLifeCycleRuleTransitionType
    {
        TAG = 0,
        BUSINESS_MODEL = 1
    }
}