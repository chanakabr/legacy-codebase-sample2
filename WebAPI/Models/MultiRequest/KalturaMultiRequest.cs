using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.MultiRequest
{
    public class KalturaMultiRequest : KalturaOTTObject
    {
        [XmlElement(ElementName = "service", IsNullable = true)]
        [JsonProperty("service")]
        [DataMember(Name = "service")]
        public string service { get; set; }

        [XmlElement(ElementName = "action", IsNullable = true)]
        [JsonProperty("action")]
        [DataMember(Name = "action")]
        public string action { get; set; }

        [JsonProperty("parameters")]
        [XmlArray(ElementName = "parameters", IsNullable = true)]
        [DataMember(Name = "parameters")]
        [XmlArrayItem("item")]
        public KalturaStringValue[] parameters { get; set; }
    }
}