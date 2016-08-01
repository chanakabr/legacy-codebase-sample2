using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// List of payment method profiles.
    /// </summary>
    [DataContract(Name = "KalturaPaymentMethodProfileListResponse", Namespace = "")]
    [XmlRoot("KalturaPaymentMethodProfileListResponse")]
    public class KalturaPaymentMethodProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// Payment method profiles list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaPaymentMethodProfile> PaymentMethodProfiles { get; set; }
    }

    public enum KalturaPaymentMethodProfileOrderBy
    {
        NONE
    }

    public class KalturaPaymentMethodProfileFilter : KalturaFilter<KalturaPaymentMethodProfileOrderBy>
    {
        public override KalturaPaymentMethodProfileOrderBy GetDefaultOrderByValue()
        {
            return KalturaPaymentMethodProfileOrderBy.NONE;
        }

        /// <summary>
        /// Payment gateway identifier to list the payment methods for
        /// </summary>
        [DataMember(Name = "paymentGatewayIdEqual")]
        [JsonProperty("paymentGatewayIdEqual")]
        [XmlElement(ElementName = "paymentGatewayIdEqual", IsNullable = true)]
        public int? PaymentGatewayIdEqual { get; set; }

        public void Validate()
        {
            if (!PaymentGatewayIdEqual.HasValue || PaymentGatewayIdEqual.Value <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter.PaymentGatewayIdEqual cannot be empty");
            }
        }
    }

    [OldStandard("allowMultiInstance", "allow_multi_instance")]
    public class KalturaPaymentMethodProfile : KalturaOTTObject
    {
        /// <summary>
        /// Payment method identifier (internal)
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int? Id { get; set; }

        /// <summary>
        /// Payment gateway identifier (internal)
        /// </summary>
        [DataMember(Name = "paymentGatewayId")]
        [JsonProperty("paymentGatewayId")]
        [XmlElement(ElementName = "paymentGatewayId")]
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        /// Payment method name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether the payment method allow multiple instances 
        /// </summary>
        [DataMember(Name = "allowMultiInstance")]
        [JsonProperty("allowMultiInstance")]
        [XmlElement(ElementName = "allowMultiInstance")]
        public bool? AllowMultiInstance { get; set; }

        internal bool getAllowMultiInstance()
        {
            return AllowMultiInstance.HasValue ? (bool)AllowMultiInstance : false;
        }

        internal int getId()
        {
            return Id.HasValue ? Id.Value : 0;
        }

        internal int getPaymentGatewayId()
        {
            return PaymentGatewayId.HasValue ? PaymentGatewayId.Value : 0;
        }
    }
}