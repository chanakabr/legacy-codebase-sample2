using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using System.Linq;


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
        public string Name { get; set; }

        /// <summary>
        /// Region external identifier
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Indicates whether this is the default region for the partner
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        public bool IsDefault{ get; set; }

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
        [DataMember(Name = "parentRegionId")]
        [JsonProperty("parentRegionId")]
        [XmlElement(ElementName = "parentRegionId")]
        public long ParentRegionId { get; set; }


        public void Validate()
        {
            if (ParentRegionId != 0 && RegionalChannels?.Count > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "parentRegionId", "linearChannels");
            }

            if (RegionalChannels?.Count > 0 && RegionalChannels.Select(c => c.LinearChannelId).Distinct().Count() != RegionalChannels.Count)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "linearChannels.linearChannelId");
            }
        }
    }
}