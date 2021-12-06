using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog.SearchPriorityGroup
{
    public partial class KalturaSearchPriorityCriteria : KalturaOTTObject
    {
        /// <summary>
        /// Criterion type 
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaSearchPriorityCriteriaType Type { get; set; }

        /// <summary>
        /// Condition
        /// KSQL has to have no more than 10 conditions. Text, boolean, enum and tag fields can be used only with = operator, numeric and datetime fields - only with &lt;, = and &gt; operators. 
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }
}