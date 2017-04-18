using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
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
        public int? Width { get; set; }

        /// <summary>
        /// Image height
        /// </summary>
        [DataMember(Name = "height")]
        [JsonProperty(PropertyName = "height")]
        [XmlElement(ElementName = "height")]
        public int? Height { get; set; }

        /// <summary>
        /// Image URL
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Image Version
        /// </summary>
        [DataMember(Name = "version")]
        [JsonProperty(PropertyName = "version")]
        [XmlElement(ElementName = "version")]
        public int? Version { get; set; }

        /// <summary>
        /// Image ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }

        /// <summary>
        /// Determined whether image was taken from default configuration or not 
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty(PropertyName = "isDefault")]
        [XmlElement(ElementName = "isDefault")]
        [OldStandardProperty("is_default")]
        public bool? IsDefault { get; set; }
    }
}