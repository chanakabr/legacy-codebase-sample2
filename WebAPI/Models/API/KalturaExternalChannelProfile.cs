using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// OSS Adapter
    /// </summary>
    public class KalturaExternalChannelProfile : KalturaOTTObject
    {
        /// <summary>
        /// External channel id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
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
        [DataMember(Name = "is_active")]
        [JsonProperty("is_active")]
        [XmlElement(ElementName = "is_active")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// External channel external identifier
        /// </summary>
        [DataMember(Name = "external_identifier")]
        [JsonProperty("external_identifier")]
        [XmlElement(ElementName = "external_identifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Filter expression
        /// </summary>
        [DataMember(Name = "filter_expression")]
        [JsonProperty("filter_expression")]
        [XmlElement(ElementName = "filter_expression")]
        public string FilterExpression { get; set; }

        /// <summary>
        /// Recommendation engine id
        /// </summary>
        [DataMember(Name = "recommendation_engine_id")]
        [JsonProperty("recommendation_engine_id")]
        [XmlElement(ElementName = "recommendation_engine_id")]
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
