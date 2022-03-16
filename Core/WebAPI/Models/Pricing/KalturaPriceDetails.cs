using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using System.Linq;
using WebAPI.ModelsValidators;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Price details
    /// </summary>
    public partial class KalturaPriceDetails : KalturaOTTObject
    {
        /// <summary>
        /// The price code identifier
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
        public string Name { get; set; }

        /// <summary>
        /// The price 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaPrice Price { get; set; }

        /// <summary>
        /// Multi currency prices for all countries and currencies
        /// </summary>
        [DataMember(Name = "multiCurrencyPrice")]
        [JsonProperty("multiCurrencyPrice")]
        [XmlElement(ElementName = "multiCurrencyPrice", IsNullable = true)]
        [SchemeProperty(RequiresPermission=(int)RequestType.WRITE, IsNullable = true)]
        public List<KalturaPrice> MultiCurrencyPrice { get; set; }

        /// <summary>
        /// A list of the descriptions for this price on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        [XmlArray(ElementName = "descriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTranslationToken> Descriptions { get; set; }

        public void ValidateForAdd()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");

            if (MultiCurrencyPrice == null || MultiCurrencyPrice.Count == 0)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "multiCurrencyPrice");

            ValidateDuplicateMultiCurrencyPrice();
        }

        public void ValidateForUpdate()
        {
            if (this.MultiCurrencyPrice != null)
            {
                if (this.MultiCurrencyPrice.Count == 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "multiCurrencyPrice");
                }

                ValidateDuplicateMultiCurrencyPrice();
            }
        }

        private void ValidateDuplicateMultiCurrencyPrice()
        {
            var totalDistincts = MultiCurrencyPrice.Distinct(new PriceEqualityComparer()).Count();
            if (totalDistincts != this.MultiCurrencyPrice.Count)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "multiCurrencyPrice");
            }

            foreach (var price in this.MultiCurrencyPrice)
            {
                price.Validate();
            }
        }
    }
    
    class PriceEqualityComparer : IEqualityComparer<KalturaPrice>
    {
        public bool Equals(KalturaPrice x, KalturaPrice y)
        {
            return x.IsEquals(y);
        }

        public int GetHashCode(KalturaPrice obj)
        {
            return this.GetHashCode();
        }
    }
}