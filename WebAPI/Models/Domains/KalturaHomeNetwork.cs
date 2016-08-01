using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Home networks
    /// </summary>
    [Serializable]
    public class KalturaHomeNetworkListResponse : KalturaListResponse
    {
        /// <summary>
        /// Home networks
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaHomeNetwork> Objects { get; set; }
    }

    /// <summary>
    /// Home network details
    /// </summary>
    [OldStandard("externalId", "external_id")]
    [OldStandard("isActive", "is_active")]
    public class KalturaHomeNetwork : KalturaOTTObject
    {
        /// <summary>
        /// Home network identifier
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Home network name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        
        /// <summary>
        /// Home network description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Is home network is active
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public bool? IsActive { get; set; }

        internal bool getIsActive()
        {
            return IsActive.HasValue ? (bool)IsActive : true;
        }
    }
}