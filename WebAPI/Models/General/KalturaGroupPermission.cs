using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    public class KalturaGroupPermission : KalturaPermission
    {
        /// <summary>
        /// Permission identifier
        /// </summary>
        [DataMember(Name = "group")]
        [JsonProperty("group")]
        [XmlElement(ElementName = "group")]
        public string Group { get; set; }
    }
}