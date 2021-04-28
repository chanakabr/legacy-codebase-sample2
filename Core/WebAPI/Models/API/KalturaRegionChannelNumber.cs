using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaRegionChannelNumber : KalturaOTTObject
    {
        /// <summary>
        /// The identifier of the region
        /// </summary>
        [DataMember(Name = "regionId")]
        [JsonProperty("regionId")]
        [XmlElement(ElementName = "regionId")]
        public int RegionId { get; set; }

        /// <summary>
        /// The number of channel
        /// </summary>
        [DataMember(Name = "channelNumber")]
        [JsonProperty("channelNumber")]
        [XmlElement(ElementName = "channelNumber")]
        public int ChannelNumber { get; set; }
    }
}
