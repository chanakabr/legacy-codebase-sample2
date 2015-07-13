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
    public class ItemPrice
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
        public PPVItemPriceDetails PPVPriceDetails { get; set; }

        /// <summary>
        /// External identifier for the file
        /// </summary>
        [DataMember(Name = "external_file_id")]
        [JsonProperty("external_file_id")]
        public string ExternalFileId { get; set; }
    }
}