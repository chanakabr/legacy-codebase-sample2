using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
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
        [XmlElement(ElementName = "percent", IsNullable = true)]
        public double? Percent { get; set; }

        /// <summary>
        /// The first date the discount is available
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate", IsNullable = true)]
        [OldStandardProperty("start_date")]
        public long? StartDate { get; set; }

        /// <summary>
        /// The last date the discount is available
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate", IsNullable = true)]
        [OldStandardProperty("end_date")]
        public long? EndDate { get; set; }
    }
}