using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
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
        [DataMember(Name = "payment_gateway_reference_id")]
        [JsonProperty("payment_gateway_reference_id")]
        [XmlElement(ElementName = "payment_gateway_reference_id")]
        public string PGReferenceID { get; set; }

        /// <summary>
        /// Response ID received from by the payment gateway. 
        /// Value is available only if the payment gateway provides this information.
        /// </summary>
        [DataMember(Name = "payment_gateway_response_id")]
        [JsonProperty("payment_gateway_response_id")]
        [XmlElement(ElementName = "payment_gateway_response_id")]
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
        [DataMember(Name = "fail_reason_code")]
        [JsonProperty("fail_reason_code")]
        [XmlElement(ElementName = "fail_reason_code")]
        public int? FailReasonCode { get; set; }

        /// <summary>
        /// Entitlement creation date
        /// </summary>
        [DataMember(Name = "created_at")]
        [JsonProperty("created_at")]
        [XmlElement(ElementName = "created_at")]
        public int? CreatedAt { get; set; }
    }
}