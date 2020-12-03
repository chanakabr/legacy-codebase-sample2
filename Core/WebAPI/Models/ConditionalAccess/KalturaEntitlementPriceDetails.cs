using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlement price details
    /// </summary>
    [Serializable]
    public partial class KalturaEntitlementPriceDetails : KalturaOTTObject
    {
        /// <summary>
        /// Full price
        /// </summary>
        [DataMember(Name = "fullPrice")]
        [JsonProperty("fullPrice")]
        [XmlElement(ElementName = "fullPrice")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaPrice FullPrice { get; set; }

        /// <summary>
        /// List of the season numbers to exclude.
        /// </summary>
        [DataMember(Name = "discountDetails")]
        [JsonProperty("discountDetails")]
        [XmlElement(ElementName = "discountDetails")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaEntitlementDiscountDetails> DiscountDetails { get; set; }
    }
}