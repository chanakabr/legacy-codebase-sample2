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
    public class KalturaBaseListWrapper : KalturaOTTObject
    {
        /// <summary>
        /// Total items
        /// </summary>
        [DataMember(Name = "total_items")]
        [JsonProperty(PropertyName = "total_items")]
        [XmlElement(ElementName="total_items")]
        public int TotalItems { get; set; }
    }
}