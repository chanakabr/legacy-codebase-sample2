using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Pricing
{
    [DataContract(Name = "Subscriptions", Namespace = "")]
    [XmlRoot("Subscriptions")]
    public class SubscriptionsList
    {
        [DataMember(Name = "subscriptions")]
        [JsonProperty("subscriptions")]
        public List<Subscription> Subscriptions { get; set; }
    }
}