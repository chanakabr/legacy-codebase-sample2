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
    public class KalturaTagValue : KalturaOTTObject
    {
        [DataMember(Name = "tagType")]
        [JsonProperty("tagType")]
        [XmlElement(ElementName = "tagType")]
        public string TagType { get; set; }

        [DataMember(Name = "tagValue")]
        [JsonProperty("tagValue")]
        [XmlElement(ElementName = "tagValue")]
        public string TagValue { get; set; }
    }
}