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
    /// Payment Gateway Settings
    /// </summary>
    public class KalturaPaymentGWSettingsResponse : KalturaOTTObject
    {
        /// <summary>
        /// List of payment_gateway_settings
        /// </summary>
        [DataMember(Name = "payment_gateway")]        
        [JsonProperty("payment_gateway")]
        [XmlArray(ElementName = "payment_gateway")]
        [XmlArrayItem("item")]  
        public List<KalturaPaymentGW> pgw { get; set; }      
    }
}