using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    [Obsolete]
    public partial class KalturaSubscriptionsFilter : KalturaOTTObject
    {
        /// <summary>
        /// Subscription identifiers or file identifier (only 1) to get the subscriptions by
        /// </summary>
        [DataMember(Name = "ids")]
        [JsonProperty("ids")]
        [XmlArray(ElementName = "ids", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaIntegerValue> Ids { get; set; }

        /// <summary>
        /// The type of the identifiers to get the subscriptions by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaSubscriptionsFilterBy By { get; set; }
    }
}