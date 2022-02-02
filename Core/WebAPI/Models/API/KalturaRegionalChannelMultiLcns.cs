using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Exceptions;

namespace WebAPI.Models.API
{
    public partial class KalturaRegionalChannelMultiLcns : KalturaRegionalChannel
    {
        /// <summary>
        /// Linear channel numbers
        /// </summary>
        [DataMember(Name = "lcns")]
        [JsonProperty("lcns")]
        [XmlElement(ElementName = "lcns")]
        public string LCNs { get; set; }

        internal IEnumerable<int> ParsedLcns => string.IsNullOrEmpty(LCNs)
            ? new[] { ChannelNumber }
            : Utils.Utils.ParseCommaSeparatedValues<int>(LCNs, $"lcns", true, true);

        public KalturaRegionalChannelMultiLcns(long linearChannelId, int channelNumber, string lcns)
            : base(linearChannelId, channelNumber)
        {
            LCNs = lcns;
        }

        public void Validate(string argumentName)
        {
            if (ParsedLcns.Any(x => x < 0))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, $"{argumentName}.lcns");
            }

            if (ChannelNumber != default && ParsedLcns.First() != ChannelNumber)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, $"{argumentName}.channelNumber", $"{argumentName}.lcns");
            }
        }
    }
}