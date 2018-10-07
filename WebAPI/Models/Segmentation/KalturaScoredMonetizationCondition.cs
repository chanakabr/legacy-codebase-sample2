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
    public partial class KalturaScoredMonetizationCondition : KalturaBaseSegmentCondition
    {
        /// <summary>
        /// The minimum score to be met
        /// </summary>
        [DataMember(Name = "minScore")]
        [JsonProperty(PropertyName = "minScore")]
        [XmlElement(ElementName = "minScore")]
        [SchemeProperty()]
        public int MinScore { get; set; }

        /// <summary>
        /// The maximum score to be met
        /// </summary>
        [DataMember(Name = "maxScore")]
        [JsonProperty(PropertyName = "maxScore")]
        [XmlElement(ElementName = "maxScore")]
        [SchemeProperty()]
        public int MaxScore { get; set; }

        /// <summary>
        /// How many days back should the actions be considered
        /// </summary>
        [DataMember(Name = "days")]
        [JsonProperty(PropertyName = "days")]
        [XmlElement(ElementName = "days")]
        [SchemeProperty()]
        public int Days { get; set; }

        /// <summary>
        /// List of the actions that consist the condition
        /// </summary>
        [DataMember(Name = "actions", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "actions", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "actions", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty()]
        public List<KalturaMonetizationCondition> Actions { get; set; }
    }

    /// <summary>
    /// Defines a singular monetization condition
    /// </summary>
    public partial class KalturaMonetizationCondition : KalturaOTTObject
    {
        /// <summary>
        /// Purchase type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty()]
        public KalturaMonetizationType Type { get; set; }

        /// <summary>
        /// Minimum price of purchase
        /// </summary>
        [DataMember(Name = "minimumPrice")]
        [JsonProperty(PropertyName = "minimumPrice")]
        [XmlElement(ElementName = "minimumPrice")]
        [SchemeProperty()]
        public int MinimumPrice { get; set; }

        /// <summary>
        /// Score multiplier
        /// </summary>
        [DataMember(Name = "multiplier")]
        [JsonProperty(PropertyName = "multiplier")]
        [XmlElement(ElementName = "multiplier")]
        [SchemeProperty()]
        public int Multiplier { get; set; }
    }
}