using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Models;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Project version info
    /// </summary>
    public class KalturaProjectVersion : KalturaOTTObject
    {
        /// <summary>
        /// Assembly version
        /// </summary>
        [DataMember(Name = "version")]
        [JsonProperty("version")]
        [XmlElement(ElementName = "version")]
        public string Version { get; set; }
    }
}