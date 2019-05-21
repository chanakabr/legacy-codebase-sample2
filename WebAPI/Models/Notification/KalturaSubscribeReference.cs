using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
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
        public long SubscriptionId { get; set; }
    }
}