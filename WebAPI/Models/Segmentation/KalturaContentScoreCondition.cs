using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Defines a condition which is essentially a combination of several content-based actions, each has their own score multiplier
    /// </summary>
    public partial class KalturaContentScoreCondition : KalturaBaseSegmentCondition
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
        /// If condition should be applied on specific field (and not the one of the segment value)
        /// </summary>
        [DataMember(Name = "field")]
        [JsonProperty(PropertyName = "field")]
        [XmlElement(ElementName = "field")]
        [SchemeProperty()]
        public string Field { get; set; }

        /// <summary>
        /// If condition should be applied on specific field (and not the one of the segment value) -
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        [SchemeProperty()]
        public string Value { get; set; }

        /// <summary>
        /// List of the actions that consist the condition
        /// </summary>
        [DataMember(Name = "actions", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "actions", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "actions", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty()]
        public List<KalturaContentActionCondition> Actions { get; set; }
    }
}