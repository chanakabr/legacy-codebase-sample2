using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Parental rule
    /// </summary>
    [Serializable]
    public class KalturaParentalRule
    {
        /// <summary>
        /// Unique parental rule identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public long id;

        /// <summary>
        /// Rule display name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string name;

        /// <summary>
        /// Explanatory description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        public string description;

        /// <summary>
        /// Rule order within the full list of rules
        /// </summary>
        [DataMember(Name = "order")]
        [JsonProperty("order")]
        public int order;

        /// <summary>
        /// Media asset tag ID to in which to look for corresponding trigger values
        /// </summary>
        [DataMember(Name = "media_tag")]
        [JsonProperty("media_tag")]
        public int mediaTagTypeId;

        /// <summary>
        /// EPG asset tag ID to in which to look for corresponding trigger values
        /// </summary>
        [DataMember(Name = "epg_tag")]
        [JsonProperty("epg_tag")]
        public int epgTagTypeId;

        /// <summary>
        /// Content that correspond to this rule is not available for guests
        /// </summary>
        [DataMember(Name = "block_anonymous_access")]
        [JsonProperty("block_anonymous_access")]
        public bool blockAnonymousAccess;

        /// <summary>
        /// Rule type – Movies, TV series or both
        /// </summary>
        [DataMember(Name = "rule_type")]
        [JsonProperty("rule_type")]
        public eParentalRuleType ruleType;

        /// <summary>
        /// Media tag values that trigger rule
        /// </summary>
        [DataMember(Name = "media_tag_values")]
        [JsonProperty("media_tag_values")]
        public List<string> mediaTagValues;

        /// <summary>
        /// EPG tag values that trigger rule
        /// </summary>
        [DataMember(Name = "epg_tag_values")]
        [JsonProperty("epg_tag_values")]
        public List<string> epgTagValues;

        /// <summary>
        /// Is the rule the default rule of the account
        /// </summary>
        [DataMember(Name = "is_default")]
        [JsonProperty("is_default")]
        public bool isDefault;

        /// <summary>
        /// Where was this rule defined account, household or user
        /// </summary>
        [DataMember(Name = "origin")]
        [JsonProperty("origin")]
        public eRuleLevel origin;

    }

    /// <summary>
    /// Rule type – Movies, TV series or both
    /// </summary>
    public enum eParentalRuleType
    {
        all = 0,
        movies = 1,
        tv_series = 2
    }

    /// <summary>
    /// Distinction if rule was defined at account, household or user level
    /// </summary>
    public enum eRuleLevel
    {
        invalid = 0,
        user = 1,
        household = 2,
        account = 3
    }
}