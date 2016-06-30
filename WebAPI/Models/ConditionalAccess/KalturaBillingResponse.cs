using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Billing response
    /// </summary>
    public class KalturaBillingResponse : KalturaOTTObject
    {
        /// <summary>
        /// Receipt Code 
        /// </summary>
        [DataMember(Name = "receipt_code")]
        [JsonProperty("receipt_code")]
        [XmlElement(ElementName = "receipt_code")]
        public string ReceiptCode { get; set; }

        /// <summary>
        /// External receipt Code 
        /// </summary>
        [DataMember(Name = "external_receipt_code")]
        [JsonProperty("external_receipt_code")]
        [XmlElement(ElementName = "external_receipt_code")]
        public string ExternalReceiptCode { get; set; }
    }
}