using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaBusinessModuleDetails : KalturaOTTObject
    {
        /// <summary>
        /// BusinessModuleId
        /// </summary>
        [DataMember(Name = "businessModuleId")]
        [JsonProperty(PropertyName = "businessModuleId")]
        [XmlElement(ElementName = "businessModuleId")]
        public int? BusinessModuleId { get; set; }

        /// <summary>
        /// BusinessModuleType
        /// </summary>
        [DataMember(Name = "businessModuleType")]
        [JsonProperty(PropertyName = "businessModuleType")]
        [XmlElement(ElementName = "businessModuleType")]
        public KalturaTransactionType? BusinessModuleType { get; set; }
    }
}