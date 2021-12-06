using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ApiObjects.Pricing;
using Newtonsoft.Json;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Discount details
    /// </summary>
    public partial class KalturaDiscountDetails : KalturaOTTObject
    {
        /// <summary>
        /// The discount ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// The price code name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1)]

        public string name { get; set; }

        /// <summary>
        /// Multi currency discounts for all countries and currencies
        /// </summary>
        [DataMember(Name = "multiCurrencyDiscount")]
        [JsonProperty("multiCurrencyDiscount")]
        [XmlElement(ElementName = "multiCurrencyDiscount", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE)]
        public List<KalturaDiscount> MultiCurrencyDiscount { get; set; }

        /// <summary>
        /// Start date represented as epoch
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty(PropertyName = "startDate")]
        [XmlElement(ElementName = "startDate")]
        [SchemeProperty(MinInteger = 1)]
        public long StartDate { get; set; }

        /// <summary>
        /// End date represented as epoch
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate")]
        [SchemeProperty(MinInteger = 1)]
        public long EndtDate { get; set; }

        /// <summary>
        /// End date represented as epoch
        /// </summary>
        [DataMember(Name = "whenAlgoTimes")]
        [JsonProperty(PropertyName = "whenAlgoTimes")]
        [XmlElement(ElementName = "whenAlgoTimes")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ, MinInteger = 0)]
        public int WhenAlgoTimes { get; set; }

        /// <summary>
        /// End date represented as epoch
        /// </summary>
        [DataMember(Name = "whenAlgoType")]
        [JsonProperty(PropertyName = "whenAlgoType")]
        [XmlElement(ElementName = "whenAlgoType")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ, MinInteger = 1)]
        public int WhenAlgoType { get; set; }
    }
}