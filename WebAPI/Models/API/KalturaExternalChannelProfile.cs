using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// External channel profiles
    /// </summary>
    [Serializable]
    public class KalturaExternalChannelProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// External channel profiles
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaExternalChannelProfile> Objects { get; set; }
    }

    /// <summary>
    /// OSS Adapter
    /// </summary>
    [OldStandard("isActive", "is_active")]
    [OldStandard("externalIdentifier", "external_identifier")]
    [OldStandard("filterExpression", "filter_expression")]
    [OldStandard("recommendationEngineId", "recommendation_engine_id")]
    public class KalturaExternalChannelProfile : KalturaOTTObject
    {
        /// <summary>
        /// External channel id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// External channel name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// External channel active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// External channel external identifier
        /// </summary>
        [DataMember(Name = "externalIdentifier")]
        [JsonProperty("externalIdentifier")]
        [XmlElement(ElementName = "externalIdentifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Filter expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression")]
        public string FilterExpression { get; set; }

        /// <summary>
        /// Recommendation engine id
        /// </summary>
        [DataMember(Name = "recommendationEngineId")]
        [JsonProperty("recommendationEngineId")]
        [XmlElement(ElementName = "recommendationEngineId")]
        public int? RecommendationEngineId { get; set; }

        /// <summary>
        /// Enrichments
        /// </summary>
        [DataMember(Name = "enrichments")]
        [JsonProperty(PropertyName = "enrichments")]
        [XmlArray(ElementName = "enrichments", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaChannelEnrichmentHolder> Enrichments { get; set; }
    }
}
