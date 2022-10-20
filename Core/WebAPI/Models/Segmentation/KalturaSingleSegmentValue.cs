using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
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

        /// <summary>
        /// The amount of households that are being affected by this Segmentation type
        /// </summary>
        [DataMember(Name = "affectedHouseholds")]
        [JsonProperty(PropertyName = "affectedHouseholds")]
        [XmlElement(ElementName = "affectedHouseholds")]
        [SchemeProperty(ReadOnly = true)]
        public int AffectedHouseholds { get; set; }
    }
}
