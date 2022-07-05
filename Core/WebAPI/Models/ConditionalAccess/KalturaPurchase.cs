using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using WebAPI.Exceptions;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaPurchase : KalturaPurchaseBase
    {
        /// <summary>
        /// Identifier for paying currency, according to ISO 4217
        /// </summary>
        [DataMember(Name = "currency")]
        [JsonProperty("currency")]
        [XmlElement(ElementName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Net sum to charge – as a one-time transaction. Price must match the previously provided price for the specified content.
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price")]
        [SchemeProperty(MinFloat = 0)]
        public double Price { get; set; }

        /// <summary>
        /// Identifier for a pre-entered payment method. If not provided – the household’s default payment method is used
        /// </summary>
        [DataMember(Name = "paymentMethodId")]
        [JsonProperty("paymentMethodId")]
        [XmlElement(ElementName = "paymentMethodId")]
        [SchemeProperty(MinInteger = 1)]
        public int? PaymentMethodId { get; set; }

        /// <summary>
        /// Identifier for a pre-associated payment gateway. If not provided – the account’s default payment gateway is used
        /// </summary>
        [DataMember(Name = "paymentGatewayId")]
        [JsonProperty("paymentGatewayId")]
        [XmlElement(ElementName = "paymentGatewayId")]
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        /// Coupon code
        /// </summary>
        [DataMember(Name = "coupon")]
        [JsonProperty("coupon")]
        [XmlElement(ElementName = "coupon")]
        public string Coupon { get; set; }
    }
}