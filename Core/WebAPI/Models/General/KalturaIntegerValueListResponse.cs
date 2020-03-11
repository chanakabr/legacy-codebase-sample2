using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Integer list wrapper
    /// </summary>
    [DataContract(Name = "KalturaIntegerValueListResponse", Namespace = "")]
    [XmlRoot("KalturaIntegerValueListResponse")]
    public partial class KalturaIntegerValueListResponse : KalturaListResponse
    {
        /// <summary>
        /// Interger value items
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaIntegerValue> Values { get; set; }

    }
}