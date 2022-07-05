using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{ 
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
    }
}