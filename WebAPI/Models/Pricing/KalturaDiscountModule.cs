using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Discount module
    /// </summary>
    public class KalturaDiscountModule
    {
        /// <summary>
        /// The discount percentage
        /// </summary>
        [DataMember(Name = "percent")]
        [JsonProperty("percent")]
        public double Percent { get; set; }

        /// <summary>
        /// The first date the discount is available
        /// </summary>
        [DataMember(Name = "start_date")]
        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The last date the discount is available
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        public DateTime EndDate { get; set; }
    }
}