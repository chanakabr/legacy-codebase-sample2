using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
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
        [DataMember(Name = "receiptCode")]
        [JsonProperty("receiptCode")]
        [XmlElement(ElementName = "receiptCode")]
        [OldStandardProperty("receipt_code")]
        public string ReceiptCode { get; set; }

        /// <summary>
        /// External receipt Code 
        /// </summary>
        [DataMember(Name = "externalReceiptCode")]
        [JsonProperty("externalReceiptCode")]
        [XmlElement(ElementName = "externalReceiptCode")]
        [OldStandardProperty("external_receipt_code")]
        public string ExternalReceiptCode { get; set; }
    }
}