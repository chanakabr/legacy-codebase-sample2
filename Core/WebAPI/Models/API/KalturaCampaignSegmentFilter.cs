using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    [SchemeClass(Required = new[] { "segmentIdEqual", "stateIn" })]
    public partial class KalturaCampaignSegmentFilter : KalturaCampaignSearchFilter
    {
        /// <summary>
        /// segment id to be searched inside campaigns
        /// </summary>
        [DataMember(Name = "segmentIdEqual")]
        [JsonProperty("segmentIdEqual")]
        [XmlElement(ElementName = "segmentIdEqual")]
        [SchemeProperty(MinLong = 1)]
        public long SegmentIdEqual { get; set; }
    }
}