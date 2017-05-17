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
    /// SubscriptionSets list
    /// </summary>
    [DataContract(Name = "SubscriptionSets", Namespace = "")]
    [XmlRoot("SubscriptionSets")]
    public class KalturaSubscriptionSetListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of subscriptionSets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaSubscriptionSet> SubscriptionSets { get; set; }
    }
}