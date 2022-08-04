using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Promotion
    /// </summary>
    public partial class KalturaPromotion : KalturaBasePromotion
    {
        /// <summary>
        /// The discount module id that is promoted to the user
        /// </summary>
        [DataMember(Name = "discountModuleId")]
        [JsonProperty("discountModuleId")]
        [XmlElement(ElementName = "discountModuleId")]
        [SchemeProperty(MinLong = 1)]
        public long DiscountModuleId { get; set; }

        /// <summary>
        /// the numer of recurring for this promotion
        /// </summary>
        [DataMember(Name = "numberOfRecurring")]
        [JsonProperty("numberOfRecurring")]
        [XmlElement(ElementName = "numberOfRecurring")]
        public int? NumberOfRecurring { get; set; }
    }
}