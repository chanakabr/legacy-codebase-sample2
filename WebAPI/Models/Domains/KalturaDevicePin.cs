using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Device pin
    /// </summary>
    public partial class KalturaDevicePin : KalturaOTTObject
    {
        /// <summary>
        /// Device pin
        /// </summary>
        [DataMember(Name = "pin")]
        [JsonProperty("pin")]
        [XmlElement(ElementName = "pin")]
        public string Pin { get; set; }
    }
}