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
    public class KalturaTransactionResponse : KalturaOTTObject
    {
        /// <summary>
        /// Kaltura unique ID representing the transaction
        /// </summary>
        [DataMember(Name = "transaction_id")]
        [JsonProperty("transaction_id")]
        [XmlElement(ElementName = "transaction_id")]
        public int TransactionID { get; set; }

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
        /// Transaction state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        [XmlElement(ElementName = "state")]
        public string State { get; set; }
    }
}