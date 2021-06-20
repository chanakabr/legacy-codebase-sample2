using ApiObjects.Pricing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Discount
    /// </summary>
    public partial class KalturaDiscount : KalturaPrice
    {
        /// <summary>
        /// The discount percentage
        /// </summary>
        [DataMember(Name = "percentage")]
        [JsonProperty("percentage")]
        [XmlElement(ElementName = "percentage", IsNullable = true)]
        public int Percentage { get; set; }
    }

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

        /// <summary>
        /// End date represented as epoch
        /// </summary>
        [DataMember(Name = "whenAlgoTimes")]
        [JsonProperty(PropertyName = "whenAlgoTimes")]
        [XmlElement(ElementName = "whenAlgoTimes")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ, MinInteger = 1)]
        public int WhenAlgoTimes { get; set; }

        /// <summary>
        /// End date represented as epoch
        /// </summary>
        [DataMember(Name = "whenAlgoType")]
        [JsonProperty(PropertyName = "whenAlgoType")]
        [XmlElement(ElementName = "whenAlgoType")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public int WhenAlgoType { get; set; }

        public void ValidateForAdd()
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");

            if (StartDate.Equals(0))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "startDate");

            if (EndtDate.Equals(0))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "EndtDate");

            if (MultiCurrencyDiscount.Count.Equals(0))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "multiCurrencyDiscount");

            if (!Enum.IsDefined(typeof(WhenAlgoType), WhenAlgoType))
                throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "WhenAlgoType", WhenAlgoType);

            foreach (KalturaDiscount discount in MultiCurrencyDiscount)
            {
                if (string.IsNullOrWhiteSpace(discount.Currency))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "currency"); 

                if (discount.Amount > 0 && discount.Percentage > 0)
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "amount", "Percentage");

                if (discount.Amount == 0 && discount.Percentage == 0)
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "amount, Percentage");
            }
        }
    }

    public partial class KalturaDiscountDetailsListResponse : KalturaListResponse
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