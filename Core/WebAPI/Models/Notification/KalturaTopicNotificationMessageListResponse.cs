using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Notifications
{
    public partial class KalturaTopicNotificationMessageListResponse : KalturaListResponse
    {
        /// <summary>
        /// Topic notification messages
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTopicNotificationMessage> Objects { get; set; }
    }
}