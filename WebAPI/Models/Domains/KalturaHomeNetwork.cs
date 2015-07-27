using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Home network details
    /// </summary>
    public class KalturaHomeNetwork : KalturaOTTObject
    {
        /// <summary>
        /// Home network identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

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
        /// Home network creation date (epoch)
        /// </summary>
        [DataMember(Name = "create_date")]
        [JsonProperty("create_date")]
        [XmlElement(ElementName = "create_date")]
        public long CreateDate { get; set; }

        /// <summary>
        /// Is home network is active
        /// </summary>
        [DataMember(Name = "is_active")]
        [JsonProperty("is_active")]
        [XmlElement(ElementName = "is_active")]
        public bool IsActive { get; set; }
    }
}