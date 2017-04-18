using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    [Obsolete]
    public class KalturaPricesFilter : KalturaOTTObject
    {
        /// <summary>
        /// Subscriptions Identifiers 
        /// </summary>
        [DataMember(Name = "subscriptionsIds")]
        [JsonProperty("subscriptionsIds")]
        [XmlArray(ElementName = "subscriptionsIds", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("subscriptions_ids")]
        public List<KalturaIntegerValue> SubscriptionsIds { get; set; }

        /// <summary>
        /// Media files Identifiers 
        /// </summary>
        [DataMember(Name = "filesIds")]
        [JsonProperty("filesIds")]
        [XmlArray(ElementName = "filesIds", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("files_ids")]
        public List<KalturaIntegerValue> FilesIds { get; set; }

        /// <summary>
        /// A flag that indicates if only the lowest price of an item should return
        /// </summary>
        [DataMember(Name = "shouldGetOnlyLowest")]
        [JsonProperty("shouldGetOnlyLowest")]
        [XmlElement(ElementName = "shouldGetOnlyLowest")]
        [OldStandardProperty("should_get_only_lowest")]
        public bool? ShouldGetOnlyLowest { get; set; }

        internal bool getShouldGetOnlyLowest()
        {
            return ShouldGetOnlyLowest.HasValue ? (bool)ShouldGetOnlyLowest : false;
        }
    }
}