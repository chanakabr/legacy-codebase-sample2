using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public partial class KalturaSubscriptionSubscribeReference : KalturaSubscribeReference
    {
        /// <summary>
        /// Subscription ID
        /// </summary>
        [DataMember(Name = "subscriptionId")]
        [JsonProperty(PropertyName = "subscriptionId")]
        [XmlElement(ElementName = "subscriptionId")]
        [SchemeProperty(MinLong = 1)]
        public long SubscriptionId { get; set; }
    }
}