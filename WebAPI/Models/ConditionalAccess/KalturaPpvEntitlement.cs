using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// KalturaPpvEntitlement
    /// </summary>
    public class KalturaPpvEntitlement : KalturaEntitlement
    {
        /// <summary>
        ///Media file identifier
        /// </summary>
        [DataMember(Name = "mediaFileId")]
        [JsonProperty("mediaFileId")]
        [XmlElement(ElementName = "mediaFileId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("media_file_id")]
        public int? MediaFileId { get; set; }

        /// <summary>
        ///Media identifier
        /// </summary>
        [DataMember(Name = "mediaId")]
        [JsonProperty("mediaId")]
        [XmlElement(ElementName = "mediaId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("media_id")]
        public int? MediaId { get; set; }
    }
}