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
    /// List of recommendation profiles.
    /// </summary>
    [DataContract(Name = "KalturaRecommendationProfileListResponse", Namespace = "")]
    [XmlRoot("KalturaRecommendationProfileListResponse")]
    public class KalturaRecommendationProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// Recommendation profiles list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaRecommendationProfile> RecommendationProfiles { get; set; }
    }

    /// <summary>
    /// PaymentGW
    /// </summary>
    [OldStandard("isActive", "is_active")]
    [OldStandard("adapterUrl", "adapter_url")]
    [OldStandard("recommendationEngineSettings", "recommendation_engine_settings")]
    [OldStandard("externalIdentifier", "external_identifier")]
    [OldStandard("sharedSecret", "shared_secret")]
    public class KalturaRecommendationProfile : KalturaOTTObject
    {
        /// <summary>
        /// recommendation engine id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// recommendation engine name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// recommendation engine is active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// recommendation engine adapter URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// recommendation engine extra parameters
        /// </summary>
        [DataMember(Name = "recommendationEngineSettings")]
        [JsonProperty("recommendationEngineSettings")]
        [XmlElement(ElementName = "recommendationEngineSettings", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// recommendation engine external identifier
        /// </summary>
        [DataMember(Name = "externalIdentifier")]
        [JsonProperty("externalIdentifier")]
        [XmlElement(ElementName = "externalIdentifier")]
        public string ExternalIdentifier { get; set; }
       
        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        [SchemeProperty(ReadOnly = true)]
        public string SharedSecret { get; set; }

        internal int getId()
        {
            return Id.HasValue ? (int)Id : 0;
        }
    }
}
