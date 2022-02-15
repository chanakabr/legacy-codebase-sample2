using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.ConditionalAccess
{
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
}