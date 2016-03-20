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
    public class KalturaHouseholdPaymentMethod : KalturaOTTObject
    {
        /// <summary>
        /// Household payment method identifier (internal)
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Payment method details
        /// </summary>
        [DataMember(Name = "details")]
        [JsonProperty("details")]
        [XmlElement(ElementName = "details")]
        public string Details { get; set; }

        /// <summary>
        /// Selected payment method 
        /// </summary>
        [DataMember(Name = "selected")]
        [JsonProperty("selected")]
        [XmlElement(ElementName = "selected")]
        public bool Selected { get; set; }
    }
}