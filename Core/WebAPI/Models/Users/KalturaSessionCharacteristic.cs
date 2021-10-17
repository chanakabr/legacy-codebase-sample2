using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Kaltura Session Characteristic
    /// </summary>
    public partial class KalturaSessionCharacteristic : KalturaOTTObject
    {
        /// <summary>
        /// Session characteristic identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement("id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }
        
        /// <summary>
        /// Region identifier
        /// </summary>
        [DataMember(Name = "regionId")]
        [JsonProperty(PropertyName = "regionId")]
        [XmlElement("regionId")]
        [SchemeProperty(ReadOnly = true)]
        public int RegionId { get; set; }
        
        /// <summary>
        /// Comma-separated list of user segments identifiers
        /// </summary>
        [DataMember(Name = "userSegmentsIds")]
        [JsonProperty(PropertyName = "userSegmentsIds")]
        [XmlElement("userSegmentsIds")]
        [SchemeProperty(ReadOnly = true)]
        public string UserSegmentsIds { get; set; }
        
        /// <summary>
        /// Comma-separated list of user roles identifiers
        /// </summary>
        [DataMember(Name = "userRolesIds")]
        [JsonProperty(PropertyName = "userRolesIds")]
        [XmlElement("userRolesIds")]
        [SchemeProperty(ReadOnly = true)]
        public string UserRolesIds { get; set; }
        
        /// <summary>
        /// Comma-separated list of user session profiles identifiers
        /// </summary>
        [DataMember(Name = "userSessionProfilesIds")]
        [JsonProperty(PropertyName = "userSessionProfilesIds")]
        [XmlElement("userSessionProfilesIds")]
        [SchemeProperty(ReadOnly = true)]
        public string UserSessionProfilesIds { get; set; }
    }
}