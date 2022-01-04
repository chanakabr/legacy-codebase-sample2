using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog.Ordering
{
    public partial class KalturaChannelSlidingWindowOrder : KalturaBaseChannelOrder
    {
        /// <summary>
        /// Sliding window period in minutes
        /// </summary>
        [DataMember(Name = "period")]
        [JsonProperty(PropertyName = "period")]
        [XmlElement(ElementName = "period", IsNullable = true)]
        [SchemeProperty(MinInteger = 1)]
        public int SlidingWindowPeriod { get; set; }

        /// <summary>
        /// Order By
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        public KalturaChannelSlidingWindowOrderByType OrderBy { get; set; }
    }
}