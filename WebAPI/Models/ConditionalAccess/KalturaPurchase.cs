using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using WebAPI.Exceptions;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaPurchaseBase : KalturaOTTObject
    {
        /// <summary>
        /// Identifier for the package from which this content is offered
        /// </summary>
        [DataMember(Name = "productId")]
        [JsonProperty("productId")]
        [XmlElement(ElementName = "productId")]
        [SchemeProperty(MinInteger = 1)]
        public int ProductId { get; set; }

        /// <summary>
        /// Identifier for the content to purchase. Relevant only if Product type = PPV
        /// </summary>
        [DataMember(Name = "contentId")]
        [JsonProperty("contentId")]
        [XmlElement(ElementName = "contentId")]
        public int? ContentId { get; set; }

        /// <summary>
        /// Package type. Possible values: PPV, Subscription, Collection
        /// </summary>
        [DataMember(Name = "productType")]
        [JsonProperty("productType")]
        [XmlElement(ElementName = "productType")]
        public KalturaTransactionType ProductType { get; set; }

        internal int getContentId()
        {
            return ContentId.HasValue ? ContentId.Value : 0;
        }
    }

    public class KalturaPurchase : KalturaPurchaseBase
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
        
        internal void Validate()
        {
            // validate purchase token
            if (string.IsNullOrEmpty(Currency))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaPurchase.currency");
        }

        internal int getPaymentMethodId()
        {
            return PaymentMethodId.HasValue ? PaymentMethodId.Value : 0;
        }

        internal int getPaymentGatewayId()
        {
            return PaymentGatewayId.HasValue ? PaymentGatewayId.Value : 0;
        }

        internal string getCoupon()
        {
            return Coupon == null ? string.Empty : Coupon;
        }
    }

    public class KalturaPurchaseSession : KalturaPurchase
    {
        /// <summary>
        /// Preview module identifier (relevant only for subscription)
        /// </summary>
        [DataMember(Name = "previewModuleId")]
        [JsonProperty("previewModuleId")]
        [XmlElement(ElementName = "previewModuleId")]
        public int? PreviewModuleId { get; set; }

        internal int getPreviewModuleId()
        {
            return PreviewModuleId.HasValue ? PreviewModuleId.Value : 0;
        }

        internal new void Validate()
        {
            if (ProductId == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaPurchaseSession.productId");
            }

            if (ProductType == KalturaTransactionType.ppv && (!ContentId.HasValue && ContentId == 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaPurchaseSession.contentId");
            }
        }
    }

    public class KalturaExternalReceipt : KalturaPurchaseBase
    {
        /// <summary>
        /// A unique identifier that was provided by the In-App billing service to validate the purchase
        /// </summary>
        [DataMember(Name = "receiptId")]
        [JsonProperty("receiptId")]
        [XmlElement(ElementName = "receiptId")]
        public string ReceiptId { get; set; }

        /// <summary>
        /// The payment gateway name for the In-App billing service to be used. Possible values: Google/Apple
        /// </summary>
        [DataMember(Name = "paymentGatewayName")]
        [JsonProperty("paymentGatewayName")]
        [XmlElement(ElementName = "paymentGatewayName")]
        public string PaymentGatewayName { get; set; }

        internal void Validate()
        {
            // validate purchase token
            if (string.IsNullOrEmpty(ReceiptId))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaExternalReceipt.receiptId");

            // validate payment gateway id
            if (string.IsNullOrEmpty(PaymentGatewayName))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaExternalReceipt.paymentGatewayName");
        }
    }
}