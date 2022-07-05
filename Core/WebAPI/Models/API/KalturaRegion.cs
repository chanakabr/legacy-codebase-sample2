using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;

namespace WebAPI.Models.API
{
    public partial class KalturaRegion : KalturaOTTObject
    {
        /// <summary>
        /// Region identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Region name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Region external identifier
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        [SchemeProperty(MinLength = 1)]
        public string ExternalId { get; set; }

        /// <summary>
        /// Indicates whether this is the default region for the partner
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        [SchemeProperty(ReadOnly = true)]
        public bool IsDefault { get; set; }

        /// <summary>
        /// List of associated linear channels
        /// </summary>
        [DataMember(Name = "linearChannels")]
        [JsonProperty("linearChannels")]
        [XmlElement(ElementName = "linearChannels")]
        public List<KalturaRegionalChannel> RegionalChannels { get; set; }


        /// <summary>
        /// Parent region ID
        /// </summary>
        [DataMember(Name = "parentId")]
        [JsonProperty("parentId")]
        [XmlElement(ElementName = "parentId")]
        [SchemeProperty(MinLong = 1)]
        public long ParentId { get; set; }

        public void Validate(bool isMultiLcnsEnabled, bool validateRequiredFields)
        {
            if (validateRequiredFields && string.IsNullOrEmpty(Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "name");
            }

            if (validateRequiredFields && string.IsNullOrEmpty(ExternalId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "externalId");
            }

            if (RegionalChannels != null)
            {
                if (RegionalChannels.Select(x => x.LinearChannelId).Distinct().Count() != RegionalChannels.Count)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "linearChannels.linearChannelId");
                }

                if (RegionalChannels.Any(x => x.ChannelNumber < 0))
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "linearChannels.channelNumber");
                }

                var multiLcnRegionalChannels = RegionalChannels
                    .OfType<KalturaRegionalChannelMultiLcns>()
                    .Where(x => isMultiLcnsEnabled)
                    .ToArray();
                foreach (var multiLcnRegionalChannel in multiLcnRegionalChannels)
                {
                    multiLcnRegionalChannel.Validate("linearChannels");
                }
            }
        }
    }
}