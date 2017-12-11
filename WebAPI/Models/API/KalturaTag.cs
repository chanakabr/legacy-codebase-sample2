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
        /// Tag id 
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Tag Type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type", IsNullable = true)]
        [SchemeProperty(MinInteger = 1)]
        public int? TagTypeId { get; set; }

        /// <summary>
        /// Tag 
        /// </summary>
        [DataMember(Name = "tag")]
        [JsonProperty("tag")]
        [XmlElement(ElementName = "tag")]
        public string Tag { get; set; }
    }
}