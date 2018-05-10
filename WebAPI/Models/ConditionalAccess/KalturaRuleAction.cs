using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public enum KalturaRuleActionType
    {
        BLOCK,
        START_DATE_OFFSET,
        END_DATE_OFFSET,
        USER_BLOCK
    }
    
    public abstract class KalturaRuleAction : KalturaOTTObject
    {
        /// <summary>
        /// The type of the action
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaRuleActionType Type { get; protected set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }
    }

    public abstract class KalturaAssetRuleAction : KalturaRuleAction
    {
    }

    public abstract class KalturaAssetUserRuleAction : KalturaRuleAction
    {
    }
    
    public class KalturaAssetUserRuleBlockAction : KalturaAssetUserRuleAction
    {
        public KalturaAssetUserRuleBlockAction()
        {
            this.Type = KalturaRuleActionType.USER_BLOCK;
        }
    }

    public class KalturaAccessControlBlockAction : KalturaAssetRuleAction
    {
        public KalturaAccessControlBlockAction()
        {
            this.Type = KalturaRuleActionType.BLOCK;
        }
    }

    /// <summary>
    /// Time offset action
    /// </summary>
    public abstract class KalturaTimeOffsetRuleAction : KalturaAssetRuleAction
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
    public class KalturaEndDateOffsetRuleAction : KalturaTimeOffsetRuleAction
    {
        public KalturaEndDateOffsetRuleAction()
        {
            this.Type = KalturaRuleActionType.END_DATE_OFFSET;
        }
    }

    /// <summary>
    /// Start date offset action
    /// </summary>
    public class KalturaStartDateOffsetRuleAction : KalturaTimeOffsetRuleAction
    {
        public KalturaStartDateOffsetRuleAction()
        {
            this.Type = KalturaRuleActionType.START_DATE_OFFSET;
        }
    }
}