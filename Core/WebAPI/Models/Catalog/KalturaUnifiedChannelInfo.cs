using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaUnifiedChannel : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// Channel identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(MinInteger = 1)]
        public long Id { get; set; }

        /// <summary>
        /// Channel Type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type", IsNullable = false)]
        public KalturaChannelType Type { get; set; }

        public virtual void Validate()
        {
        }
    }

    public partial class KalturaUnifiedChannelInfo : KalturaUnifiedChannel
    {
        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Start date in seconds
        /// </summary>
        [DataMember(Name = "startDateInSeconds")]
        [JsonProperty("startDateInSeconds")]
        [XmlElement(ElementName = "startDateInSeconds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? StartDateInSeconds { get; set; }

        /// <summary>
        /// End date in seconds
        /// </summary>
        [DataMember(Name = "endDateInSeconds")]
        [JsonProperty("endDateInSeconds")]
        [XmlElement(ElementName = "endDateInSeconds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? EndDateInSeconds { get; set; }

        public override void Validate()
        {
            if (StartDateInSeconds.HasValue && EndDateInSeconds.HasValue && StartDateInSeconds >= EndDateInSeconds)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDateInSeconds", "endDateInSeconds");
            }
        }
    }

    public enum KalturaChannelType
    {
        Internal,
        External
    }
}
