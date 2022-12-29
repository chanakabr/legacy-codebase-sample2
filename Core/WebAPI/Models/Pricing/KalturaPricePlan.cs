using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Price plan
    /// </summary>
    public partial class KalturaPricePlan : KalturaUsageModule
    {
        /// <summary>
        /// Denotes whether or not this object can be renewed
        /// </summary>
        [DataMember(Name = "isRenewable")]
        [JsonProperty("isRenewable")]
        [XmlElement(ElementName = "isRenewable")]
        [OldStandardProperty("is_renewable")]
        [SchemeProperty(WriteOnly = true, IsNullable = true)]

        public bool? IsRenewable { get; set; }

        /// <summary>
        /// Defines the number of times the module will be renewed (for the life_cycle period)
        /// </summary>
        [DataMember(Name = "renewalsNumber")]
        [JsonProperty("renewalsNumber")]
        [XmlElement(ElementName = "renewalsNumber")]
        [OldStandardProperty("renewals_number")]
        [SchemeProperty(WriteOnly = true, MinInteger = 0, IsNullable = true)]
        public int? RenewalsNumber { get; set; }

        /// <summary>
        /// Unique identifier associated with this object's price 
        /// </summary>
        [DataMember(Name = "priceId")]
        [JsonProperty("priceId")]
        [XmlElement(ElementName = "priceId")]
        [OldStandardProperty("price_id")]
        [SchemeProperty(ReadOnly = true)]
        [Deprecated("4.5.0.0")]
        public int? PriceId { get; set; }

        /// <summary>
        /// The discount module identifier of the price plan
        /// </summary>
        [DataMember(Name = "discountId")]
        [JsonProperty("discountId")]
        [XmlElement(ElementName = "discountId")]
        [OldStandardProperty("discount_id")]
        [SchemeProperty(WriteOnly = true, MinLong = 1, IsNullable = true)]
        public long? DiscountId { get; set; }

        /// <summary>
        /// The ID of the price details associated with this price plan
        /// </summary>
        [DataMember(Name = "priceDetailsId")]
        [JsonProperty("priceDetailsId")]
        [XmlElement(ElementName = "priceDetailsId")]
        [SchemeProperty(MinLong = 1, IsNullable = true)]
        public long? PriceDetailsId { get; set; }
    }
}