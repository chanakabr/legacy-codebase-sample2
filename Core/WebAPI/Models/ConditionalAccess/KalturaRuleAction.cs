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
        APPLY_FREE_PLAYBACK
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
    public partial class KalturaAssetUserRuleBlockAction : KalturaAssetUserRuleAction
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.USER_BLOCK;
        }
    }

    [Serializable]
    public partial class KalturaAssetUserRuleFilterAction : KalturaAssetUserRuleAction
    {
        /// <summary>
        /// Indicates whether to apply on channel
        /// </summary>
        [DataMember(Name = "applyOnChannel")]
        [JsonProperty("applyOnChannel")]
        [XmlElement(ElementName = "applyOnChannel")]
        public bool ApplyOnChannel { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FILTER;
        }
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

    [Serializable]
    public partial class KalturaAllowPlaybackAction : KalturaAssetRuleAction
    {
        public KalturaAllowPlaybackAction()
        {
            this.Type = KalturaRuleActionType.ALLOW_PLAYBACK;
        }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.ALLOW_PLAYBACK;
        }
    }

    [Serializable]
    public partial class KalturaBlockPlaybackAction : KalturaAssetRuleAction
    {
        public KalturaBlockPlaybackAction()
        {
            this.Type = KalturaRuleActionType.BLOCK_PLAYBACK;
        }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.BLOCK_PLAYBACK;
        }
    }

    [Serializable]
    public abstract partial class KalturaBusinessModuleRuleAction : KalturaRuleAction
    {
    }

    [Serializable]
    public partial class KalturaApplyDiscountModuleAction : KalturaBusinessModuleRuleAction
    {
        /// <summary>
        /// Discount module ID
        /// </summary>
        [DataMember(Name = "discountModuleId")]
        [JsonProperty("discountModuleId")]
        [XmlElement(ElementName = "discountModuleId")]
        public long DiscountModuleId { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.APPLY_DISCOUNT_MODULE;
        }
    }

    [Serializable]
    public partial class KalturaApplyPlaybackAdapterAction : KalturaAssetRuleAction
    {
        /// <summary>
        /// Playback Adapter Identifier 
        /// </summary>
        [DataMember(Name = "adapterId")]
        [JsonProperty("adapterId")]
        [XmlElement(ElementName = "adapterId")]
        public int AdapterId { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.APPLY_PLAYBACK_ADAPTER;
        }
    }

    [Serializable]
    public abstract partial class KalturaAssetLifeCycleTransitionAction : KalturaAssetRuleAction
    {
        /// <summary>
        /// Asset LifeCycle Rule Action Type
        /// </summary>
        [DataMember(Name = "assetLifeCycleRuleActionType")]
        [JsonProperty("assetLifeCycleRuleActionType")]
        [XmlElement(ElementName = "assetLifeCycleRuleActionType")]
        public KalturaAssetLifeCycleRuleActionType AssetLifeCycleRuleActionType { get; set; }

        /// <summary>
        /// Asset LifeCycle Rule Transition Type
        /// </summary>
        [DataMember(Name = "assetLifeCycleRuleTransitionType")]
        [JsonProperty("assetLifeCycleRuleTransitionType")]
        [XmlElement(ElementName = "assetLifeCycleRuleTransitionType")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaAssetLifeCycleRuleTransitionType AssetLifeCycleRuleTransitionType { get; protected set; }
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

    [Serializable]
    public partial class KalturaAssetLifeCycleTagTransitionAction : KalturaAssetLifeCycleTransitionAction
    {
        /// <summary>
        /// Comma separated list of tag Ids.
        /// </summary>
        [DataMember(Name = "tagIds")]
        [JsonProperty("tagIds")]
        [XmlElement(ElementName = "tagIds")]
        public string TagIds { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.ASSET_LIFE_CYCLE_TRANSITION;
            this.AssetLifeCycleRuleTransitionType = KalturaAssetLifeCycleRuleTransitionType.TAG;
        }
    }

    [Serializable]
    public partial class KalturaAssetLifeCycleBuisnessModuleTransitionAction : KalturaAssetLifeCycleTransitionAction
    {
        /// <summary>
        /// Comma separated list of fileType Ids.
        /// </summary>
        [DataMember(Name = "fileTypeIds")]
        [JsonProperty("fileTypeIds")]
        [XmlElement(ElementName = "fileTypeIds")]
        public string FileTypeIds { get; set; }

        /// <summary>
        /// Comma separated list of ppv Ids.
        /// </summary>
        [DataMember(Name = "ppvIds")]
        [JsonProperty("ppvIds")]
        [XmlElement(ElementName = "ppvIds")]
        public string PpvIds { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.ASSET_LIFE_CYCLE_TRANSITION;
            this.AssetLifeCycleRuleTransitionType = KalturaAssetLifeCycleRuleTransitionType.BUSINESS_MODEL;
        }
    }

    [Serializable]
    public partial class KalturaApplyFreePlaybackAction : KalturaBusinessModuleRuleAction
    {
        public KalturaApplyFreePlaybackAction()
        {
            this.Type = KalturaRuleActionType.APPLY_FREE_PLAYBACK;
        }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.APPLY_FREE_PLAYBACK;
        }
    }
}