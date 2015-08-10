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
    /// Base list wrapper
    /// </summary>
    [Serializable]
    public class KalturaListResponse : KalturaOTTObject
    {
        /// <summary>
        /// Total items
        /// </summary>
        [DataMember(Name = "totalCount")]
        [JsonProperty(PropertyName = "totalCount")]
        [XmlElement(ElementName = "totalCount")]
        public int TotalCount { get; set; }
    }
}