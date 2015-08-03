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
        [XmlElement(ElementName="service")]
        [JsonProperty("service")]
        [DataMember(Name="service")]
        public string service { get; set; }
        [XmlElement(ElementName = "action")]
        [JsonProperty("action")]
        [DataMember(Name = "action")]
        public string action { get; set; }
        [XmlElement(ElementName = "parameters")]
        [JsonProperty("parameters")]
        [XmlArray(ElementName = "parameters")]
        [XmlArrayItem("item")] 
        public string[] parameters { get; set; }
    }
}