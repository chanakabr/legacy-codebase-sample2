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
    /// A string representation to return an array of ints
    /// </summary>
    public class KalturaIntegerValue : KalturaValue
    {
        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [XmlElement("value")]
        [JsonProperty("value")]
        [ValidationException(SchemeValidationType.NULLABLE)]
        public int value { get; set; }
    }
}