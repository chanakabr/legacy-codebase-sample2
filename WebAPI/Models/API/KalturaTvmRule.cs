using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// TVM rule
    /// </summary>
    [Serializable]
    public abstract partial class KalturaTvmRule : KalturaRule
    {
        /// <summary>
        /// Specifies when was the tvm rule was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies the tvm rule type.
        /// </summary>
        [DataMember(Name = "ruleType")]
        [JsonProperty("ruleType")]
        [XmlElement(ElementName = "ruleType")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaRuleType RuleType { get; set; }
    }
}