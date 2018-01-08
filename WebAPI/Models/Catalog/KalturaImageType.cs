using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Image type 
    /// </summary>
    [Serializable]
    public class KalturaImageType : KalturaOTTObject
    {
        /// <summary>
        /// Image type ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [OldStandardProperty("id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// System name
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty(PropertyName = "systemName")]
        [XmlElement(ElementName = "systemName")]
        [SchemeProperty(MinLength = 1)]
        public string SystemName { get; set; }

        /// <summary>
        /// Ration ID
        /// </summary>
        [DataMember(Name = "ratioId")]
        [JsonProperty(PropertyName = "ratioId")]
        [XmlElement(ElementName = "ratioId")]
        [SchemeProperty(MinLong = 1)]
        public long RatioId { get; set; }

        /// <summary>
        /// Help text
        /// </summary>
        [DataMember(Name = "helpText")]
        [JsonProperty(PropertyName = "helpText")]
        [XmlElement(ElementName = "helpText")]
        public string HelpText { get; set; }

        /// <summary>
        /// Default image ID
        /// </summary>
        [DataMember(Name = "defaultImageId")]
        [JsonProperty(PropertyName = "defaultImageId")]
        [XmlElement(ElementName = "defaultImageId", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public long? DefaultImageId { get; set; }
    }

    public class KalturaImageTypeListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of partner image types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaImageType> ImageTypes { get; set; }
    }
}