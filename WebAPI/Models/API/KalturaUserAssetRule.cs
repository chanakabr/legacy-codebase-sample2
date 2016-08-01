using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Generic rule - representing different type of rules (Parental, Geo, User Type, Device)
    /// </summary>
    [OldStandard("ruleType", "rule_type")]
    [Obsolete]
    public class KalturaGenericRule : KalturaOTTObject
    {
        /// <summary>
        /// Unique rule identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long? Id { get; set; }

        /// <summary>
        /// Rule type - possible values: Rule type – Parental, Geo, UserType, Device
        /// </summary>
        [DataMember(Name = "ruleType")]
        [JsonProperty("ruleType")]
        [XmlElement(ElementName = "ruleType", IsNullable = true)]
        public KalturaRuleType RuleType { get; set; }

        /// <summary>
        /// Rule display name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Additional description for the specific rule
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }
    }
}