using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.API;

namespace WebAPI.Models.General
{
    public class KalturaApiArgumentPermissionItem : KalturaPermissionItem
    {
        /// <summary>
        /// API service name
        /// </summary>
        [DataMember(Name = "service")]
        [JsonProperty("service")]
        [XmlElement(ElementName = "service")]
        public string Service { get; set; }

        /// <summary>
        /// API action name
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty("action")]
        [XmlElement(ElementName = "action")]
        public string Action { get; set; }

        /// <summary>
        /// API parameter name
        /// </summary>
        [DataMember(Name = "parameter")]
        [JsonProperty("parameter")]
        [XmlElement(ElementName = "parameter")]
        public string Parameter { get; set; }
    }
}