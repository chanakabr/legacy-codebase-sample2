using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Exceptions;
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
        /// The number of the channel
        /// </summary>
        [DataMember(Name = "channelNumber")]
        [JsonProperty("channelNumber")]
        [XmlElement(ElementName = "channelNumber")]
        public int ChannelNumber { get; set; }

        public KalturaRegionChannelNumber(int regionId, int channelNumber)
        {
            RegionId = regionId;
            ChannelNumber = channelNumber;
        }

        public static void Validate(bool enableMultiLcns, IReadOnlyCollection<KalturaRegionChannelNumber> regionChannelNumbers)
        {
            if (regionChannelNumbers == null || regionChannelNumbers.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(regionChannelNumbers));
            }
            
            if (regionChannelNumbers.Select(x => x.RegionId).Distinct().Count() != regionChannelNumbers.Count)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "regionId");
            }

            foreach (var regionChannelNumber in regionChannelNumbers.OfType<KalturaRegionChannelNumberMultiLcns>().Where(x => enableMultiLcns))
            {
                regionChannelNumber.Validate();
            }
        }
    }
}