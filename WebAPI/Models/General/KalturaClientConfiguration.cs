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
    /// Define client optional configurations
    /// </summary>
    public class KalturaClientConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Client Tag
        /// </summary>
        [DataMember(Name = "clientTag")]
        [JsonProperty("clientTag")]
        [XmlElement(ElementName = "clientTag")]        
        public string ClientTag { get; set; }

        /// <summary>
        /// API client version
        /// </summary>
        [DataMember(Name = "apiVersion")]
        [JsonProperty("apiVersion")]
        [XmlElement(ElementName = "apiVersion")]
        public string ApiVersion { get; set; }
    }
}