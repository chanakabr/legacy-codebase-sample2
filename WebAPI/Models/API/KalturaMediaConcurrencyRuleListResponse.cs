using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;
namespace WebAPI.Models.API
{
    public partial class KalturaMediaConcurrencyRuleListResponse : KalturaListResponse
    {
        /// <summary>
        /// Media CONCURRENCY RULES
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaMediaConcurrencyRule> Objects { get; set; }
    }
}