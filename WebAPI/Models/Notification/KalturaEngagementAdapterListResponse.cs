using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// Engagement adapter list
    /// </summary>
    [DataContract(Name = "GenericRules", Namespace = "")]
    [XmlRoot("GenericRules")]
    public class KalturaEngagementAdapterListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of Engagement adapter
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaEngagementAdapter> EngagementAdapters { get; set; }
    }
}
