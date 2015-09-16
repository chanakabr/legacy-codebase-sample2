using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Parental rule
    /// </summary>
    [Serializable]
    public class KalturaParentalRule : KalturaOTTObject
    {
        /// <summary>
        /// Unique parental rule identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long id { get; set; }

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
        public int order { get; set; }

        /// <summary>
        /// Media asset tag ID to in which to look for corresponding trigger values
        /// </summary>
        [DataMember(Name = "media_tag")]
        [JsonProperty("media_tag")]
        [XmlElement(ElementName = "media_tag")]
        public int mediaTagTypeId { get; set; }

        /// <summary>
        /// EPG asset tag ID to in which to look for corresponding trigger values
        /// </summary>
        [DataMember(Name = "epg_tag")]
        [JsonProperty("epg_tag")]
        [XmlElement(ElementName = "epg_tag")]
        public int epgTagTypeId { get; set; }

        /// <summary>
        /// Content that correspond to this rule is not available for guests
        /// </summary>
        [DataMember(Name = "block_anonymous_access")]
        [JsonProperty("block_anonymous_access")]
        [XmlElement(ElementName = "block_anonymous_access")]
        public bool blockAnonymousAccess { get; set; }

        /// <summary>
        /// Rule type – Movies, TV series or both
        /// </summary>
        [DataMember(Name = "rule_type")]
        [JsonProperty("rule_type")]
        [XmlElement(ElementName = "rule_type", IsNullable = true)]
        public KalturaParentalRuleType ruleType { get; set; }

        /// <summary>
        /// Media tag values that trigger rule
        /// </summary>
        [DataMember(Name = "media_tag_values")]
        [JsonProperty("media_tag_values")]
        [XmlArray(ElementName = "media_tag_values")]
        [XmlArrayItem("item")]
        public List<KalturaStringValue> mediaTagValues { get; set; }

        /// <summary>
        /// EPG tag values that trigger rule
        /// </summary>
        [DataMember(Name = "epg_tag_values")]
        [JsonProperty("epg_tag_values")]
        [XmlArray(ElementName = "epg_tag_values")]
        [XmlArrayItem("item")]
        public List<KalturaStringValue> epgTagValues { get; set; }

        /// <summary>
        /// Is the rule the default rule of the account
        /// </summary>
        [DataMember(Name = "is_default")]
        [JsonProperty("is_default")]
        [XmlElement(ElementName = "is_default")]
        public bool isDefault { get; set; }

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
        all = 0,
        movies = 1,
        tv_series = 2
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