using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using WebAPI.Exceptions;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaExternalReceipt : KalturaPurchaseBase
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
    }

}