using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Channel order details
    /// </summary>
    public partial class KalturaChannelOrder : KalturaOTTObject
    {
        /// <summary>
        /// Channel dynamic order by (meta)
        /// </summary>
        [DataMember(Name = "dynamicOrderBy")]
        [JsonProperty("dynamicOrderBy")]
        [XmlElement(ElementName = "dynamicOrderBy", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaDynamicOrderBy DynamicOrderBy { get; set; }

        /// <summary>
        /// Channel order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaChannelOrderBy? orderBy { get; set; }

        /// <summary>
        /// Sliding window period in minutes, used only when ordering by LIKES_DESC / VOTES_DESC / RATINGS_DESC / VIEWS_DESC
        /// </summary>
        [DataMember(Name = "period")]
        [JsonProperty(PropertyName = "period")]
        [XmlElement(ElementName = "period", IsNullable = true)]
        [SchemeProperty(MinLong = 1, IsNullable = true)]
        public int? SlidingWindowPeriod { get; set; }
    }
}