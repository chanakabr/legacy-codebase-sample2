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
    /// AddUserFavorite request
    /// </summary>
    public class KalturaAddUserFavoriteRequest : KalturaOTTObject
    {
        /// <summary>
        /// Media Type ID (according to media type IDs defined dynamically in the system).
        /// </summary>
        [DataMember(Name = "media_type")]
        [JsonProperty("media_type")]
        [XmlElement(ElementName = "media_type")]
        public string MediaType { get; set; }

        /// <summary>
        /// Media identifier
        /// </summary>
        [DataMember(Name = "media_id")]
        [JsonProperty("media_id")]
        [XmlElement(ElementName = "media_id")]
        public string MediaId { get; set; }

        /// <summary>
        /// Extra Value
        /// </summary>
        [DataMember(Name = "extra_data")]
        [JsonProperty("extra_data")]
        [XmlElement(ElementName = "extra_data")]
        public string ExtraData { get; set; }
    }
}