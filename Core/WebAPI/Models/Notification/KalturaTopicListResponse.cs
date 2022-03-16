using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// List of Topics.
    /// </summary>
    [DataContract(Name = "KalturaTopicListResponse", Namespace = "")]
    [XmlRoot("KalturaTopicListResponse")]
    public partial class KalturaTopicListResponse : KalturaListResponse
    {
        /// <summary>
        /// List of Topics
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaTopic> Topics { get; set; }
    }
}