using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;

namespace WebAPI.Models.General
{
    /// <summary>
    /// A string representation to return an array of doubles
    /// </summary>
    public class KalturaDoubleValue : KalturaValue
    {
        [DataMember(Name = "value")]
        [XmlElement("value")]
        [JsonProperty("value")]
        [ValidationException(ValidationType.NULLABLE)]
        public double value { get; set; }
    }
}