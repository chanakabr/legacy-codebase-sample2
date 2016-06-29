using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Schema;

namespace WebAPI.Models.ConditionalAccess
{
    [OldStandard("paymentGatewayReferenceId", "payment_gateway_reference_id")]
    [OldStandard("paymentGatewayResponseId", "payment_gateway_response_id")]
    [OldStandard("failReasonCode", "fail_reason_code")]
    [OldStandard("createdAt", "created_at")]
    public class KalturaTransaction : KalturaOTTObject
    {
        /// <summary>
        /// Kaltura unique ID representing the transaction
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Transaction reference ID received from the payment gateway. 
        /// Value is available only if the payment gateway provides this information.
        /// </summary>
        [DataMember(Name = "paymentGatewayReferenceId")]
        [JsonProperty("paymentGatewayReferenceId")]
        [XmlElement(ElementName = "paymentGatewayReferenceId")]
        public string PGReferenceID { get; set; }

        /// <summary>
        /// Response ID received from by the payment gateway. 
        /// Value is available only if the payment gateway provides this information.
        /// </summary>
        [DataMember(Name = "paymentGatewayResponseId")]
        [JsonProperty("paymentGatewayResponseId")]
        [XmlElement(ElementName = "paymentGatewayResponseId")]
        public string PGResponseID { get; set; }

        /// <summary>
        /// Transaction state: OK/Pending/Failed
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        [XmlElement(ElementName = "state")]
        public string State { get; set; }


        /// <summary>
        /// Adapter failure reason code
        /// Insufficient funds = 20, Invalid account = 21, User unknown = 22, Reason unknown = 23, Unknown payment gateway response = 24,
        /// No response from payment gateway = 25, Exceeded retry limit = 26, Illegal client request = 27, Expired = 28
        /// </summary>
        [DataMember(Name = "failReasonCode")]
        [JsonProperty("failReasonCode")]
        [XmlElement(ElementName = "failReasonCode")]
        public int? FailReasonCode { get; set; }

        /// <summary>
        /// Entitlement creation date
        /// </summary>
        [DataMember(Name = "createdAt")]
        [JsonProperty("createdAt")]
        [XmlElement(ElementName = "createdAt")]
        public int? CreatedAt { get; set; }
    }
}