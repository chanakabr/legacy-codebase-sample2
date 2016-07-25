using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaAssetFileContext : KalturaOTTObject
    {
        /// <summary>
        /// duration
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty("duration")]
        [XmlElement(ElementName = "duration")]
        public string Duration { get; set; }
    }
}