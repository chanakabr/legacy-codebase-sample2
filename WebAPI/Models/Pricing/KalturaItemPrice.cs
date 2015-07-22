using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// PPV price details
    /// </summary>
    public class KalturaItemPrice
    {
        /// <summary>
        /// Media file identifier  
        /// </summary>
        [DataMember(Name = "file_id")]
        [JsonProperty("file_id")]
        public int FileId { get; set; }

        /// <summary>
        /// PPV price details
        /// </summary>
        [DataMember(Name = "ppv_price_details")]
        [JsonProperty("ppv_price_details")]
        public List<KalturaPPVItemPriceDetails> PPVPriceDetails { get; set; }

        /// <summary>
        /// Product code for the file
        /// </summary>
        [DataMember(Name = "product_code")]
        [JsonProperty("product_code")]
        public string ProductCode { get; set; }
    }
}