using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public partial class KalturaTopicNotification : KalturaOTTObject
    {
        /// <summary>
        /// Topic notification ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Topic notification name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Topic notification description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Announcement enabled
        /// </summary>
        [DataMember(Name = "subscribeReference")]
        [JsonProperty(PropertyName = "subscribeReference")]
        [XmlElement(ElementName = "subscribeReference")]
        public KalturaSubscribeReference SubscribeReference { get; set; }
    }

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