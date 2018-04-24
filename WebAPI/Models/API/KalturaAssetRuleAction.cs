using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Action
    /// </summary>
    public abstract class KalturaAssetRuleAction : KalturaOTTObject
    {
        /// <summary>
        /// Description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }
    }

    /// <summary>
    /// Block action
    /// </summary>
    public class KalturaBlockRuleAction : KalturaAssetRuleAction
    {
    }

    /// <summary>
    /// Time offset action
    /// </summary>
    public class KalturaTimeOffsetRuleAction : KalturaAssetRuleAction
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
    }

    /// <summary>
    /// Start date offset action
    /// </summary>
    public class KalturaStartDateOffsetRuleAction : KalturaTimeOffsetRuleAction
    {
    }
}