using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Preview module
    /// </summary>
    public partial class KalturaPreviewModule : KalturaOTTObject
    {
        /// <summary>
        /// Preview module identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

        /// <summary>
        /// Preview module name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Preview module life cycle - for how long the preview module is active
        /// </summary>
        [DataMember(Name = "lifeCycle")]
        [JsonProperty("lifeCycle")]
        [XmlElement(ElementName = "lifeCycle")]
        [OldStandardProperty("life_cycle")]
        [SchemeProperty(IsNullable = true, MinInteger = 1)]
        public int? LifeCycle { get; set; }

        /// <summary>
        /// The time you can't buy the item to which the preview module is assigned to again
        /// </summary>
        [DataMember(Name = "nonRenewablePeriod")]
        [JsonProperty("nonRenewablePeriod")]
        [XmlElement(ElementName = "nonRenewablePeriod")]
        [OldStandardProperty("non_renewable_period")]
        [SchemeProperty(IsNullable = true, MinInteger = 1)]
        public int? NonRenewablePeriod { get; set; }

        public void ValidateForAdd()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            if (!NonRenewablePeriod.HasValue)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "nonRenewablePeriod");
            if (!LifeCycle.HasValue)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "lifeCycle");
        }
    }
}