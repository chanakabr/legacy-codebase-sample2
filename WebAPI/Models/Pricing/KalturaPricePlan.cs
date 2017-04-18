using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Price plan
    /// </summary>
    public class KalturaPricePlan : KalturaUsageModule
    {
        /// <summary>
        /// Denotes whether or not this object can be renewed
        /// </summary>
        [DataMember(Name = "isRenewable")]
        [JsonProperty("isRenewable")]
        [XmlElement(ElementName = "isRenewable")]
        [OldStandardProperty("is_renewable")]
        public bool? IsRenewable { get; set; }

        /// <summary>
        /// Defines the number of times the module will be renewed (for the life_cycle period)
        /// </summary>
        [DataMember(Name = "renewalsNumber")]
        [JsonProperty("renewalsNumber")]
        [XmlElement(ElementName = "renewalsNumber")]
        [OldStandardProperty("renewals_number")]
        public int? RenewalsNumber { get; set; }

        /// <summary>
        /// Unique identifier associated with this object's price 
        /// </summary>
        [DataMember(Name = "priceId")]
        [JsonProperty("priceId")]
        [XmlElement(ElementName = "priceId")]
        [OldStandardProperty("price_id")]
        public int? PriceId { get; set; }

        /// <summary>
        /// The discount module identifier of the price plan
        /// </summary>
        [DataMember(Name = "discountId")]
        [JsonProperty("discountId")]
        [XmlElement(ElementName = "discountId")]
        [OldStandardProperty("discount_id")]
        public long? DiscountId { get; set; }
    }
}