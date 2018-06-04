using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Discount
    /// </summary>
    public class KalturaDiscount : KalturaPrice
    {
        /// <summary>
        /// The discount percentage
        /// </summary>
        [DataMember(Name = "percentage")]
        [JsonProperty("percentage")]
        [XmlElement(ElementName = "percentage", IsNullable = true)]
        [SchemeProperty(ReadOnly=true)]
        public int Percentage { get; set; }
    }

    /// <summary>
    /// Discount details
    /// </summary>
    public class KalturaDiscountDetails : KalturaOTTObject
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
        public long StartDate { get; set; }

        /// <summary>
        /// End date represented as epoch
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate")]
        public long EndtDate { get; set; }
    }

    public class KalturaDiscountDetailsListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of price details
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaDiscountDetails> Discounts { get; set; }
    }
}