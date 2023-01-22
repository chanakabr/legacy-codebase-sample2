using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Playback profile
    /// </summary>
    public partial class KalturaPlaybackProfile : KalturaOTTObject
    {
        /// <summary>
        /// Playback profile identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// Playback profile name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Playback profile active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Playback profile Grpc address
        /// </summary>
        [DataMember(Name = "adapterGrpcAddress")]
        [JsonProperty("adapterGrpcAddress")]
        [XmlElement(ElementName = "adapterGrpcAddress")]
        public string AdapterGrpcAddress { get; set; }
        
        /// <summary>
        /// Playback profile URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// Playback profile settings
        /// </summary>
        [DataMember(Name = "settings")]
        [JsonProperty("settings")]
        [XmlElement("settings", IsNullable = true)]
        public string Settings { get; set; }

        /// <summary>
        /// Playback profile alias
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName")]
        public string SystemName { get; set; }

        /// <summary>
        /// Playback adapter shared secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        [SchemeProperty(ReadOnly = true)]
        public string SharedSecret { get; set; }        
    }
}