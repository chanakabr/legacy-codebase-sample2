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
    /// Define base profile response -  optional configurations
    /// </summary>
    [JsonObject]
    public abstract class KalturaBaseResponseProfile : KalturaOTTObject
    {
        /// <summary>
        /// name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
    }
}