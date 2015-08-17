using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// A representation to return an array of values
    /// </summary>
    public abstract class KalturaValue : KalturaOTTObject
    {
        [DataMember(Name = "description")]
        [XmlElement("description")]
        [JsonProperty("description")]
        public string description { get; set; }
    }
}