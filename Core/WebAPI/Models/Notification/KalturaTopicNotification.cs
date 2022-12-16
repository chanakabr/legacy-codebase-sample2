using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
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
}