using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Subscriptions list
    /// </summary>
    [DataContract(Name = "Subscriptions", Namespace = "")]
    [XmlRoot("Subscriptions")]
    public class KalturaSubscriptionsList : KalturaOTTObject
    {
        /// <summary>
        /// A list of subscriptions
        /// </summary>
        [DataMember(Name = "subscriptions")]
        [JsonProperty("subscriptions")]
        [XmlArray(ElementName = "subscriptions")]
        [XmlArrayItem("item")] 
        public List<KalturaSubscription> Subscriptions { get; set; }
    }
}