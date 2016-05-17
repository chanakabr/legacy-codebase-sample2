using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Favorite request filter 
    /// </summary>
    [OldStandard("mediaType", "media_type")]
    [OldStandard("mediaIds", "media_ids")]
    public class KalturaFavoriteFilter : KalturaOTTObject
    {
        /// <summary>
        /// Media type to filter by the favorite assets
        /// </summary>
        [DataMember(Name = "mediaType")]
        [JsonProperty(PropertyName = "mediaType")]
        [XmlElement(ElementName = "mediaType")]
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
        [DataMember(Name = "mediaIds")]
        [JsonProperty(PropertyName = "mediaIds")]
        [XmlArray(ElementName = "mediaIds", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> MediaIds { get; set; }
    }
}