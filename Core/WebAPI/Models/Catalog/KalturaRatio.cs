using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Exceptions;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaRatio : KalturaOTTObject
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(InsertOnly = true)]
        public string Name { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        [DataMember(Name = "height")]
        [JsonProperty(PropertyName = "height")]
        [XmlElement(ElementName = "height")]
        [SchemeProperty(MinInteger = 1, InsertOnly = true)]
        public int Height { get; set; }

        /// <summary>
        /// Width
        /// </summary>
        [DataMember(Name = "width")]
        [JsonProperty(PropertyName = "width")]
        [XmlElement(ElementName = "width")]
        [SchemeProperty(MinInteger = 1, InsertOnly = true)]
        public int Width { get; set; }

        /// <summary>
        /// Accepted error margin precentage of an image uploaded for this ratio
        /// 0 - no validation, everything accepted
        /// </summary>
        [DataMember(Name = "precisionPrecentage")]
        [JsonProperty(PropertyName = "precisionPrecentage")]
        [XmlElement(ElementName = "precisionPrecentage")]
        [SchemeProperty(MinInteger = 0, MaxInteger = 100)]
        public int PrecisionPrecentage { get; set; }
    }
}