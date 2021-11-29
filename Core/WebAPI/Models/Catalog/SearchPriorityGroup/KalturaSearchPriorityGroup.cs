using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog.SearchPriorityGroup
{
    public partial class KalturaSearchPriorityGroup : KalturaOTTObject
    {
        /// <summary>
        /// Identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// Search criterion
        /// </summary>
        [DataMember(Name = "criteria")]
        [JsonProperty("criteria")]
        [XmlElement(ElementName = "criteria")]
        public KalturaSearchPriorityCriteria Criteria { get; set; }
    }
}