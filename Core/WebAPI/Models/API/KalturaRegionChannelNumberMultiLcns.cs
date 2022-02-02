using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Exceptions;

namespace WebAPI.Models.API
{
    public partial class KalturaRegionChannelNumberMultiLcns : KalturaRegionChannelNumber
    {
        /// <summary>
        /// Linear channel numbers
        /// </summary>
        [DataMember(Name = "lcns")]
        [JsonProperty("lcns")]
        [XmlElement(ElementName = "lcns")]
        public string LCNs { get; set; }

        internal IEnumerable<int> ParsedLcns => Utils.Utils.ParseCommaSeparatedValues<int>(LCNs, $"lcns", true);

        public KalturaRegionChannelNumberMultiLcns(int regionId, int channelNumber, string lcns)
            : base(regionId, channelNumber)
        {
            RegionId = regionId;
            LCNs = lcns;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(LCNs))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "lcns");
            }

            if (ChannelNumber != default && LCNs.First() != ChannelNumber)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "channelNumber", "lcns");
            }
        }
    }
}