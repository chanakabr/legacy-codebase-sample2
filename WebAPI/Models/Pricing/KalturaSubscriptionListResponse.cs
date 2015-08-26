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
    public class KalturaSubscriptionListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of subscriptions
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects")]
        [XmlArrayItem("item")] 
        public List<KalturaSubscription> Subscriptions { get; set; }
    }
}