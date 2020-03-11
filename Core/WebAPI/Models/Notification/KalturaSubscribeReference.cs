using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public partial class KalturaSubscribeReference : KalturaOTTObject
    {
        
    }

    [Serializable]
    public partial class KalturaSubscriptionSubscribeReference : KalturaSubscribeReference
    {
        /// <summary>
        /// Subscription ID
        /// </summary>
        [DataMember(Name = "subscriptionId")]
        [JsonProperty(PropertyName = "subscriptionId")]
        [XmlElement(ElementName = "subscriptionId")]
        [SchemeProperty(MinInteger = 1)]
        public long SubscriptionId { get; set; }
    }
}