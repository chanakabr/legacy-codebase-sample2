using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Partner configuration for segments configuration
    /// </summary>    
    public partial class KalturaSegmentationPartnerConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// The maximum number of past days to be calculated for dynamic segments, default=180
        /// </summary>
        [DataMember(Name = "maxCalculatedPeriod")]
        [JsonProperty(PropertyName = "maxCalculatedPeriod")]
        [XmlElement(ElementName = "maxCalculatedPeriod")]
        [SchemeProperty(MinInteger = 1, MaxInteger = 365, IsNullable = true)]
        public int? MaxCalculatedPeriod { get; set; }

        /// <summary>
        /// How many dynamic segments (segments with conditions) the operator is allowed to have, default=50
        /// </summary>
        [DataMember(Name = "maxDynamicSegments")]
        [JsonProperty(PropertyName = "maxDynamicSegments")]
        [XmlElement(ElementName = "maxDynamicSegments")]
        [SchemeProperty(MinInteger = 1, MaxInteger = 1000, IsNullable = true)]
        public int? MaxDynamicSegments { get; set; }
    }
}