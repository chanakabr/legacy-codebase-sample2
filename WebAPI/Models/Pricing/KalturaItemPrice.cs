using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// PPV price details
    /// </summary>
    public class KalturaItemPrice : KalturaProductPrice
    {
        /// <summary>
        /// Media file identifier  
        /// </summary>
        [DataMember(Name = "file_id")]
        [JsonProperty("file_id")]
        [XmlElement(ElementName = "file_id")]
        public int FileId { get; set; }

        /// <summary>
        /// PPV price details
        /// </summary>
        [DataMember(Name = "ppv_price_details")]
        [JsonProperty("ppv_price_details")]
        [XmlArray(ElementName = "ppv_price_details")]
        [XmlArrayItem("item")] 
        public List<KalturaPPVItemPriceDetails> PPVPriceDetails { get; set; }
    }
}