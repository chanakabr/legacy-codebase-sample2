using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaTag : KalturaOTTObject
    {
        /// <summary>
        /// Tag Type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Tag 
        /// </summary>
        [DataMember(Name = "tag")]
        [JsonProperty("tag")]
        [XmlElement(ElementName = "tag")]
        public string Tag { get; set; }
    }
}