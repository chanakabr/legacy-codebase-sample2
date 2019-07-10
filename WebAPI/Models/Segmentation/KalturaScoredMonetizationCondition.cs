using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Defines a condition which is essentially a combination of several monetization-based actions, each has their own score multiplier
    /// </summary>
    public partial class KalturaMonetizationCondition : KalturaBaseSegmentCondition
    {
        /// <summary>
        /// The minimum value to be met
        /// </summary>
        [DataMember(Name = "minValue")]
        [JsonProperty(PropertyName = "minValue")]
        [XmlElement(ElementName = "minValue")]
        [SchemeProperty()]
        public int? MinValue { get; set; }

        /// <summary>
        /// The maximum value to be met
        /// </summary>
        [DataMember(Name = "maxValue")]
        [JsonProperty(PropertyName = "maxValue")]
        [XmlElement(ElementName = "maxValue")]
        [SchemeProperty()]
        public int? MaxValue { get; set; }

        /// <summary>
        /// How many days back should the actions be considered
        /// </summary>
        [DataMember(Name = "days")]
        [JsonProperty(PropertyName = "days")]
        [XmlElement(ElementName = "days")]
        [SchemeProperty()]
        public int? Days { get; set; }

        /// <summary>
        /// Purchase type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty()]
        public KalturaMonetizationType Type { get; set; }

        /// <summary>
        /// Mathermtical operator to calculate
        /// </summary>
        [DataMember(Name = "operator")]
        [JsonProperty(PropertyName = "operator")]
        [XmlElement(ElementName = "operator")]
        [SchemeProperty()]
        public KalturaMathemticalOperatorType Operator { get; set; }

        /// <summary>
        /// Comma saperated list of business module IDs
        /// </summary>
        [DataMember(Name = "businessModuleIdIn")]
        [JsonProperty(PropertyName = "businessModuleIdIn")]
        [XmlElement(ElementName = "businessModuleIdIn")]
        [SchemeProperty()]
        public string BusinessModuleIdIn { get; set; }

        internal List<int> GetBusinessModuleIdIn()
        {
            return this.GetItemsIn<List<int>, int>(BusinessModuleIdIn, "KalturaMonetizationCondition.businessModuleIdIn");
        }
    }
}