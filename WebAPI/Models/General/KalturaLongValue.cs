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
    /// A string representation to return an array of longs
    /// </summary>
    public class KalturaLongValue : KalturaValue
    {
        [DataMember(Name = "value")]
        [XmlElement("value")]
        [JsonProperty("value")]
        public long value { get; set; }
    }
}