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
    /// TVM geo rule
    /// </summary>
    [Serializable]
    public partial class KalturaTvmGeoRule : KalturaTvmRule
    {
        // TODO SHIR - DONT FORGET TO UPDATE DESCRIPTION
        /// <summary>
        /// Indicates if the rule is relevent ONLY for the country ids or except country ids here.
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

        // TODO SHIR - DONT FORGET TO UPDATE TYPE, MAPPING, DESCRIPTION
        /// <summary>
        /// proxyRule - what is that?
        /// </summary>
        [DataMember(Name = "proxyRule")]
        [JsonProperty("proxyRule")]
        [XmlElement(ElementName = "proxyRule")]
        public int ProxyRule { get; set; }

        // TODO SHIR - DONT FORGET TO UPDATE TYPE, MAPPING, DESCRIPTION
        /// <summary>
        /// proxyLevel - what is that?
        /// </summary>
        [DataMember(Name = "proxyLevel")]
        [JsonProperty("proxyLevel")]
        [XmlElement(ElementName = "proxyLevel")]
        public int ProxyLevel { get; set; }

        protected override void Init()
        {
            base.Init();
            this.RuleType = KalturaRuleType.geo;
        }

        public HashSet<int> GetCountryIds()
        {
            return this.GetItemsIn<HashSet<int>, int>(CountryIds, "KalturaTvmGeoRule.countryIds");
        }
    }
}