using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Subscription details
    /// </summary>
    public class KalturaSubscriptionSet : KalturaOTTObject
    {
        /// <summary>
        /// SubscriptionSet identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// SubscriptionSet name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// A list of subscriptions associated with this set 
        /// </summary>
        [DataMember(Name = "subscriptions")]
        [JsonProperty("subscriptions")]
        [XmlArray(ElementName = "subscriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaSubscription> Subscriptions { get; set; }

    }
}