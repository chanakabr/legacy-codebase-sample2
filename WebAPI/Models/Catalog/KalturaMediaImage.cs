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
    /// Image details
    /// </summary>
    public class KalturaMediaImage : KalturaOTTObject
    {
        /// <summary>
        /// Image aspect ratio
        /// </summary>
        [DataMember(Name = "ratio")]
        [JsonProperty(PropertyName = "ratio")]
        [XmlElement(ElementName = "ratio")]
        public string Ratio { get; set; }

        /// <summary>
        /// Image width
        /// </summary>
        [DataMember(Name = "width")]
        [JsonProperty(PropertyName = "width")]
        [XmlElement(ElementName = "width")]
        public int Width { get; set; }

        /// <summary>
        /// Image height
        /// </summary>
        [DataMember(Name = "height")]
        [JsonProperty(PropertyName = "height")]
        [XmlElement(ElementName = "height")]
        public int Height { get; set; }

        /// <summary>
        /// Image URL
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }
    }
}