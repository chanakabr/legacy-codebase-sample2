using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{    
    public class KalturaString : KalturaOTTObject
    {
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement("value")]
        public string value { get; set; }
    }
}