using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public enum KalturaTvmRuleOrderBy
    {
        NONE
    }

    /// <summary>
    /// Asset user rule filter
    /// </summary>
    public partial class KalturaTvmRuleFilter : KalturaFilter<KalturaTvmRuleOrderBy>
    {
        /// <summary>
        /// Indicates which tvm rule list to return by their type.
        /// </summary>
        [DataMember(Name = "ruleTypeEqual")]
        [JsonProperty("ruleTypeEqual")]
        [XmlElement(ElementName = "ruleTypeEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaTvmRuleType? RuleTypeEqual { get; set; }

        /// <summary>
        /// Indicates which tvm rule list to return by their name.
        /// </summary>
        [DataMember(Name = "nameEqual")]
        [JsonProperty("nameEqual")]
        [XmlElement(ElementName = "nameEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string NameEqual { get; set; }
        
        public override KalturaTvmRuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaTvmRuleOrderBy.NONE;
        }
    }
}