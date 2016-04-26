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
    /// Favorite request filter 
    /// </summary>
    public class KalturaFavoriteFilter : KalturaOTTObject
    {
        /// <summary>
        /// Media type to filter by the favorite assets
        /// </summary>
        [DataMember(Name = "media_type")]
        [JsonProperty(PropertyName = "media_type")]
        [XmlElement(ElementName = "media_type")]
        public int? MediaType { get; set; }

        /// <summary>
        /// Device UDID to filter by the favorite assets
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement(ElementName = "udid")]
        public string UDID { get; set; }

        /// <summary>
        /// Media identifiers from which to filter the favorite assets
        /// </summary>
        [DataMember(Name = "media_ids")]
        [JsonProperty(PropertyName = "media_ids")]
        [XmlArray(ElementName = "media_ids", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> MediaIds { get; set; }
    }
}