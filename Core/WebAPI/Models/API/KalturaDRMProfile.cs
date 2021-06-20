using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// DRM Adapter
    /// </summary>
    public partial class KalturaDrmProfile : KalturaOTTObject
    {
        /// <summary>
        /// DRM adapter identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// DRM adapter name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// DRM adapter active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        [SchemeProperty(IsNullable = true)]
        public bool? IsActive { get; set; }

        /// <summary>
        /// DRM adapter URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// DRM adapter settings
        /// </summary>
        [DataMember(Name = "settings")]
        [JsonProperty("settings")]
        [XmlElement("settings", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string Settings { get; set; }

        /// <summary>
        /// DRM adapter alias
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName")]
        public string SystemName { get; set; }

        /// <summary>
        /// DRM shared secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        [SchemeProperty(ReadOnly = true)]
        public string SharedSecret { get; set; }

        public void ValidateForAdd()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");

            if (string.IsNullOrWhiteSpace(AdapterUrl))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "adapterUrl");

            if (string.IsNullOrWhiteSpace(SystemName))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");

        }
    }
}