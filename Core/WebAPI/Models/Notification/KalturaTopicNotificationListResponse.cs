using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Models.Notifications;

namespace WebAPI.Models.Notifications
{
    public partial class KalturaTopicNotificationListResponse : KalturaListResponse
    {
        /// <summary>
        /// Topic notifications
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTopicNotification> Objects { get; set; }
    }
}