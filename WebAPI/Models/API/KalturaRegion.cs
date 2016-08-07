using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaRegion : KalturaOTTObject
    {
        /// <summary>
        /// Region identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Region name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Region external identifier
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Indicates whether this is the default region for the partner
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        public bool IsDefault{ get; set; }

        /// <summary>
        /// List of associated linear channels
        /// </summary>
        [DataMember(Name = "linearChannels")]
        [JsonProperty("linearChannels")]
        [XmlElement(ElementName = "linearChannels")]
        public List<KalturaLinearChannel> LinearChannels { get; set; }
    }
}