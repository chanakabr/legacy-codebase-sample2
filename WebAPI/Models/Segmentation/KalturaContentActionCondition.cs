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
    /// Segmentation condition regarding content actions
    /// </summary>
    public partial class KalturaContentActionCondition : KalturaOTTObject
    {
        /// <summary>
        /// The relevant action to be examined
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlElement(ElementName = "action")]
        [SchemeProperty()]
        public KalturaContentAction Action { get; set; }

        /// <summary>
        /// Optional - if action required specific length to be considered
        /// </summary>
        [DataMember(Name = "length")]
        [JsonProperty(PropertyName = "length")]
        [XmlElement(ElementName = "length")]
        [SchemeProperty()]
        public int Length { get; set; }

        /// <summary>
        /// Score multiplier - how much is a single action worth when considering the action
        /// </summary>
        [DataMember(Name = "multiplier")]
        [JsonProperty(PropertyName = "multiplier")]
        [XmlElement(ElementName = "multiplier")]
        [SchemeProperty()]
        public int Multiplier { get; set; }
    }

}