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
    /// <summary>
    /// Permitted media list
    /// </summary>
    [DataContract(Name = "PermittedMediaList", Namespace = "")]
    [XmlRoot("PermittedMediaList")]
    public class KalturaPermittedMediaList : KalturaOTTObject
    {
        /// <summary>
        /// A list of item prices
        /// </summary>
        [DataMember(Name = "permitted_media")]
        [JsonProperty("permitted_media")]
        [XmlArray(ElementName = "permitted_media")]
        [XmlArrayItem("permitted_media")]
        public List<KalturaPermittedMedia> PermittedMedia { get; set; }
    }
}