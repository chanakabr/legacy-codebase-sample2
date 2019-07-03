using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
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
        FILTER
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
}