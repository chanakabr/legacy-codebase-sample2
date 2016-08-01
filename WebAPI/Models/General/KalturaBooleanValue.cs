using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// A string representation to return an array of booleans
    /// </summary>
    public class KalturaBooleanValue : KalturaValue
    {
        [DataMember(Name = "value")]
        [XmlElement("value")]
        [JsonProperty("value")]
        [ValidationException(SchemeValidationType.NULLABLE)]
        public bool value { get; set; }
    }
}