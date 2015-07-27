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
    public class KalturaPaymentGWHouseholdResponse : KalturaOTTObject
    {
        /// <summary>
        /// Billing account identifier 
        /// </summary>
        [DataMember(Name = "charge_id")]
        [JsonProperty("charge_id")]
        [XmlElement(ElementName = "charge_id")]
        public string  chargeID { get; set; }
    }
}