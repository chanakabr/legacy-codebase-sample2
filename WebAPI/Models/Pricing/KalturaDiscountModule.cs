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
    /// Discount module
    /// </summary>
    public class KalturaDiscountModule : KalturaOTTObject
    {
        /// <summary>
        /// The discount percentage
        /// </summary>
        [DataMember(Name = "percent")]
        [JsonProperty("percent")]
        [XmlElement(ElementName = "percent")]
        public double Percent { get; set; }

        /// <summary>
        /// The first date the discount is available
        /// </summary>
        [DataMember(Name = "start_date")]
        [JsonProperty("start_date")]
        [XmlElement(ElementName = "start_date")]
        public long StartDate { get; set; }

        /// <summary>
        /// The last date the discount is available
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        [XmlElement(ElementName = "end_date")]
        public long EndDate { get; set; }
    }
}