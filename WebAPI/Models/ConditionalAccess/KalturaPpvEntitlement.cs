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
    public class KalturaPpvEntitlement : KalturaEntitlement
    {
        /// <summary>
        ///Media file identifier
        /// </summary>
        [DataMember(Name = "mediaFileId")]
        [JsonProperty("mediaFileId")]
        [XmlElement(ElementName = "mediaFileId")]
        [SchemeProperty(ReadOnly = true)]       
        public int? MediaFileId { get; set; }

        /// <summary>
        ///Media identifier
        /// </summary>
        [DataMember(Name = "mediaId")]
        [JsonProperty("mediaId")]
        [XmlElement(ElementName = "mediaId")]
        [SchemeProperty(ReadOnly = true)]
        public int? MediaId { get; set; }
    }
}