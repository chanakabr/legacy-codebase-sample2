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
    /// Device brand details
    /// </summary>
    public class KalturaDeviceBrand : KalturaOTTObject
    {
        /// <summary>
        /// Device brand identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

        /// <summary>
        /// Device brand name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Device family identifier
        /// </summary>
        [DataMember(Name = "deviceFamilyid")]
        [JsonProperty("deviceFamilyid")]
        [XmlElement(ElementName = "deviceFamilyid")]
        [SchemeProperty(ReadOnly = true)]
        public long? DeviceFamilyId { get; set; }
    }
}