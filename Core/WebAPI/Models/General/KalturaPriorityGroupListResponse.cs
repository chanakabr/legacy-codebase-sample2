using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.General
{
    public partial class KalturaPriorityGroupListResponse : KalturaOTTObject, IKalturaListResponse
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
