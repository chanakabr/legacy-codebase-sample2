using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.API
{
    /// <summary>
    /// TVM geo rule
    /// </summary>
    [Serializable]
    public partial class KalturaTvmGeoRule : KalturaTvmRule
    {
        /// <summary>
        /// Indicates if the rule is relevent ONLY for the country ids or except country ids here. - is that true?
        /// </summary>
        [DataMember(Name = "onlyOrBut")]
        [JsonProperty("onlyOrBut")]
        [XmlElement(ElementName = "onlyOrBut")]
        public bool OnlyOrBut { get; set; }

        /// <summary>
        /// Comma separated list of country Ids.
        /// </summary>
        [DataMember(Name = "countryIds")]
        [JsonProperty("countryIds")]
        [XmlElement(ElementName = "countryIds")]
        public string CountryIds { get; set; }

        /// <summary>
        /// proxyRuleId - what is that?
        /// </summary>
        [DataMember(Name = "proxyRuleId")]
        [JsonProperty("proxyRuleId")]
        [XmlElement(ElementName = "proxyRuleId")]
        public int ProxyRuleId { get; set; }

        /// <summary>
        /// proxyRuleName - what is that?
        /// </summary>
        [DataMember(Name = "proxyRuleName")]
        [JsonProperty("proxyRuleName")]
        [XmlElement(ElementName = "proxyRuleName")]
        public string ProxyRuleName { get; set; }

        /// <summary>
        /// proxyLevelId - what is that?
        /// </summary>
        [DataMember(Name = "proxyLevelId")]
        [JsonProperty("proxyLevelId")]
        [XmlElement(ElementName = "proxyLevelId")]
        public int ProxyLevelId { get; set; }

        /// <summary>
        /// proxyLevelName - what is that?
        /// </summary>
        [DataMember(Name = "proxyLevelName")]
        [JsonProperty("proxyLevelName")]
        [XmlElement(ElementName = "proxyLevelName")]
        public string ProxyLevelName { get; set; }

        protected override void Init()
        {
            base.Init();
            this.RuleType = KalturaTvmRuleType.Geo;
        }

        public HashSet<int> GetCountryIds()
        {
            return Utils.Utils.ParseCommaSeparatedValues<HashSet<int>, int>(CountryIds, "KalturaTvmGeoRule.countryIds");
        }
    }
}