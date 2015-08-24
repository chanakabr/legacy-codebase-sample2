using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public class KalturaProductPrice : KalturaOTTObject
    {
        /// <summary>
        /// Product identifier
        /// </summary>
        [DataMember(Name = "product_id")]
        [JsonProperty("product_id")]
        [XmlElement(ElementName = "product_id")]
        public string ProductId { get; set; }

        /// <summary>
        /// Product Type
        /// </summary>
        [DataMember(Name = "product_type")]
        [JsonProperty("product_type")]
        [XmlElement(ElementName = "product_type")]
        public KalturaTransactionType ProductType { get; set; }
    }
}