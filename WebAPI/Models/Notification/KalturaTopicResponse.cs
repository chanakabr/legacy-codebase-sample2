using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// List of inbox message.
    /// </summary>
    [DataContract(Name = "KalturaTopicResponse", Namespace = "")]
    [XmlRoot("KalturaTopicResponse")]
    public class KalturaTopicResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaTopic> Topics{ get; set; }
    }
}