using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Device details
    /// </summary>
    public class KalturaDevice
    {
        /// <summary>
        /// Device UDID
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty("udid")]
        public string Udid { get; set; }

        /// <summary>
        /// Device name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Device brand name
        /// </summary>
        [DataMember(Name = "brand")]
        [JsonProperty("brand")]
        public string Brand { get; set; }

        /// <summary>
        /// Device brand identifier
        /// </summary>
        [DataMember(Name = "brand_id")]
        [JsonProperty("brand_id")]
        public int BrandId { get; set; }

        /// <summary>
        /// Device activation date (epoch)
        /// </summary>
        [DataMember(Name = "activated_on")]
        [JsonProperty("activated_on")]
        public DateTime ActivatedOn { get; set; }

        /// <summary>
        /// Device state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        public KalturaDeviceState State { get; set; }
    }
}