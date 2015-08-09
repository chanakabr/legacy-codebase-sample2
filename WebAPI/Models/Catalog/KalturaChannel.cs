using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Channel details
    /// </summary>
    public class KalturaChannel : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the channel
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Cannel description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Channel images 
        /// </summary>
        [DataMember(Name = "images")]
        [JsonProperty(PropertyName = "images")]
        [XmlArray(ElementName = "images")]
        [XmlArrayItem("item")] 
        public List<KalturaMediaImage> Images { get; set; }

        /// <summary>
        /// Media types in the channel 
        /// </summary>
        [DataMember(Name = "media_types")]
        [JsonProperty(PropertyName = "media_types")]
        [XmlArray(ElementName = "media_types")]
        [XmlArrayItem("item")] 
        public List<int> MediaTypes { get; set; }
    }
}