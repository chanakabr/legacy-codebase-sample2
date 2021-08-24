using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Price 
    /// </summary>
    [Serializable]
    public partial class KalturaPrice : KalturaOTTObject
    {
        /// <summary>
        ///Price
        /// </summary>
        [DataMember(Name = "amount")]
        [JsonProperty("amount")]
        [XmlElement(ElementName = "amount", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public double? Amount { get; set; }

        /// <summary>
        ///Currency
        /// </summary>
        [DataMember(Name = "currency")]
        [JsonProperty("currency")]
        [XmlElement(ElementName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        ///Currency Sign
        /// </summary>
        [DataMember(Name = "currencySign")]
        [JsonProperty("currencySign")]
        [XmlElement(ElementName = "currencySign")]
        [OldStandardProperty("currency_sign")]
        public string CurrencySign { get; set; }

        /// <summary>
        ///Country ID
        /// </summary>
        [DataMember(Name = "countryId")]
        [JsonProperty("countryId")]
        [XmlElement(ElementName = "countryId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ, IsNullable = true)]
        public long? CountryId { get; set; }

        public void Validate()
        {
            if (!this.Amount.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "amount");
            }

            if (this.Amount.Value < 0.01)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "amount", "0.01");
            }

            if (string.IsNullOrWhiteSpace(this.Currency))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "currency");
            }
        }

        protected bool Equals(KalturaPrice other)
        {
            return Currency == other.Currency && 
                   CountryId == other.CountryId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KalturaPrice)obj);
        }
    }

    class PriceEqualityComparer : IEqualityComparer<KalturaPrice>
    {
        public bool Equals(KalturaPrice x, KalturaPrice y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(KalturaPrice obj)
        {
            return this.GetHashCode();
        }
    }
}