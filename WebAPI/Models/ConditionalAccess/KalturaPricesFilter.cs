using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaPricesFilter : KalturaOTTObject
    {
        /// <summary>
        /// Subscriptions Identifiers 
        /// </summary>
        [DataMember(Name = "subscriptions_ids")]
        [JsonProperty("subscriptions_ids")]
        [XmlArray(ElementName = "subscriptions_ids")]
        [XmlArrayItem("item")] 
        public List<KalturaIntegerValue> SubscriptionsIds { get; set; }

        /// <summary>
        /// Media files Identifiers 
        /// </summary>
        [DataMember(Name = "files_ids")]
        [JsonProperty("files_ids")]
        [XmlArray(ElementName = "files_ids")]
        [XmlArrayItem("item")] 
        public List<KalturaIntegerValue> FilesIds { get; set; }

        /// <summary>
        /// A flag that indicates if only the lowest price of an item should return
        /// </summary>
        [DataMember(Name = "should_get_only_lowest")]
        [JsonProperty("should_get_only_lowest")]
        [XmlElement(ElementName = "should_get_only_lowest")]
        public bool ShouldGetOnlyLowest { get; set; }
    }
}