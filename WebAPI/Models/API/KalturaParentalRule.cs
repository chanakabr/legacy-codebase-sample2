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
    /// Parental rule
    /// </summary>
    [Serializable]
    [OldStandard("mediaTag", "media_tag")]
    [OldStandard("epgTag", "epg_tag")]
    [OldStandard("blockAnonymousAccess", "block_anonymous_access")]
    [OldStandard("ruleType", "rule_type")]
    [OldStandard("mediaTagValues", "media_tag_values")]
    [OldStandard("epgTagValues", "epg_tag_values")]
    [OldStandard("isDefault", "is_default")]
    public class KalturaParentalRule : KalturaOTTObject
    {
        /// <summary>
        /// Unique parental rule identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long? id { get; set; }

        /// <summary>
        /// Rule display name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string name { get; set; }

        /// <summary>
        /// Explanatory description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        public string description { get; set; }

        /// <summary>
        /// Rule order within the full list of rules
        /// </summary>
        [DataMember(Name = "order")]
        [JsonProperty("order")]
        [XmlElement(ElementName = "order")]
        public int? order { get; set; }

        /// <summary>
        /// Media asset tag ID to in which to look for corresponding trigger values
        /// </summary>
        [DataMember(Name = "mediaTag")]
        [JsonProperty("mediaTag")]
        [XmlElement(ElementName = "mediaTag")]
        public int? mediaTagTypeId { get; set; }

        /// <summary>
        /// EPG asset tag ID to in which to look for corresponding trigger values
        /// </summary>
        [DataMember(Name = "epgTag")]
        [JsonProperty("epgTag")]
        [XmlElement(ElementName = "epgTag")]
        public int? epgTagTypeId { get; set; }

        /// <summary>
        /// Content that correspond to this rule is not available for guests
        /// </summary>
        [DataMember(Name = "blockAnonymousAccess")]
        [JsonProperty("blockAnonymousAccess")]
        [XmlElement(ElementName = "blockAnonymousAccess")]
        public bool? blockAnonymousAccess { get; set; }

        /// <summary>
        /// Rule type – Movies, TV series or both
        /// </summary>
        [DataMember(Name = "ruleType")]
        [JsonProperty("ruleType")]
        [XmlElement(ElementName = "ruleType", IsNullable = true)]
        public KalturaParentalRuleType ruleType { get; set; }

        /// <summary>
        /// Media tag values that trigger rule
        /// </summary>
        [DataMember(Name = "mediaTagValues")]
        [JsonProperty("mediaTagValues")]
        [XmlArray(ElementName = "mediaTagValues", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaStringValue> mediaTagValues { get; set; }

        /// <summary>
        /// EPG tag values that trigger rule
        /// </summary>
        [DataMember(Name = "epgTagValues")]
        [JsonProperty("epgTagValues")]
        [XmlArray(ElementName = "epgTagValues", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaStringValue> epgTagValues { get; set; }

        /// <summary>
        /// Is the rule the default rule of the account
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        public bool? isDefault { get; set; }

        /// <summary>
        /// Where was this rule defined account, household or user
        /// </summary>
        [DataMember(Name = "origin")]
        [JsonProperty("origin")]
        [XmlElement(ElementName = "origin", IsNullable = true)]
        public KalturaRuleLevel Origin { get; set; }

    }

    /// <summary>
    /// Rule type – Movies, TV series or both
    /// </summary>
    public enum KalturaParentalRuleType
    {
        ALL = 0,
        MOVIES = 1,
        TV_SERIES = 2
    }

    /// <summary>
    /// Distinction if rule was defined at account, household or user level
    /// </summary>
    public enum KalturaRuleLevel
    {
        invalid = 0,
        user = 1,
        household = 2,
        account = 3
    }
}