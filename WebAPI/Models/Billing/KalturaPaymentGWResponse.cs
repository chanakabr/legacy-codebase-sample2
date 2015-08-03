using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// Payment Gateway
    /// </summary>
    public class KalturaPaymentGWResponse : KalturaOTTObject
    {
        /// <summary>
        /// List of payment_gateway 
        /// </summary>
        [DataMember(Name = "payment_gateway_basic")]   
        [JsonProperty("payment_gateway_basic")]        
        [XmlArray(ElementName = "payment_gateway_basic")]
        [XmlArrayItem("item")]  
        public List<KalturaPaymentGWBasic> pgw { get; set; }
    }
}