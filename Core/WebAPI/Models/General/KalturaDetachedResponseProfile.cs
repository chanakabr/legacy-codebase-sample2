using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Define specific base profile response
    /// </summary>
    [JsonObject]
    public partial class KalturaDetachedResponseProfile : KalturaBaseResponseProfile
    {
        /// <summary>
        /// name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// filter
        /// </summary>
        [DataMember(Name = "filter")]
        [JsonProperty("filter")]
        [XmlElement(ElementName = "filter")]
        [SchemeProperty(IsNullable = true)]
        public KalturaRelatedObjectFilter Filter { get; set; }

        /// <summary>
        /// relatedProfiles
        /// </summary>
        [DataMember(Name = "relatedProfiles")]
        [JsonProperty("relatedProfiles")]
        [XmlElement(ElementName = "relatedProfiles")]
        public List<KalturaDetachedResponseProfile> RelatedProfiles { get; set; }
    }
}