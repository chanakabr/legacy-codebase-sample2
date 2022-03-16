using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public partial class KalturaUsageModuleListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of usage modules
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaUsageModule> UsageModules { get; set; }
    }
}