using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaUnifiedChannel : KalturaOTTObject
    {
        /// <summary>
        /// Channel identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(MinInteger = 1)]
        public long Id { get; set; }

        /// <summary>
        /// Channel Type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type", IsNullable = false)]
        public KalturaChannelType Type { get; set; }
    }

    public partial class KalturaUnifiedChannelInfo : KalturaUnifiedChannel
    {
        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Category time slot
        /// </summary>
        [DataMember(Name = "timeSlot")]
        [JsonProperty("timeSlot")]
        [XmlElement(ElementName = "timeSlot")]
        public KalturaTimeSlot TimeSlot { get; set; }
    }

    public enum KalturaChannelType
    {
        Internal,
        External
    }
}
