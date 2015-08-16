using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// OTT User filter
    /// </summary>
    public class KalturaOTTUserFilter : KalturaOTTObject
    {
        /// <summary>
        /// User IDs to retrieve
        /// </summary>
        [DataMember(Name = "user_ids")]
        [JsonProperty("user_ids")]
        [XmlArray(ElementName = "user_ids")]
        [XmlArrayItem("item")] 
        public List<string> UserIDs { get; set; }
    }
}