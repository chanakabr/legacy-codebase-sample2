using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Base class that defines segment value
    /// </summary>
    public partial class KalturaBaseSegmentValue : KalturaOTTObject
    {

    }
    public partial class KalturaSingleSegmentValue : KalturaBaseSegmentValue
    {
        /// <summary>
        /// Id of segment
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// The amount of users that are being affected by this Segmentation type
        /// </summary>
        [DataMember(Name = "affectedUsers")]
        [JsonProperty(PropertyName = "affectedUsers")]
        [XmlElement(ElementName = "affectedUsers")]
        [SchemeProperty(ReadOnly = true)]
        public int AffectedUsers { get; set; }
    }
}