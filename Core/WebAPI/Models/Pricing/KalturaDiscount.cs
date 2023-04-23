using ApiObjects.Pricing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Discount
    /// </summary>
    public partial class KalturaDiscount : KalturaPrice
    {
        /// <summary>
        /// The discount percentage
        /// </summary>
        [DataMember(Name = "percentage")]
        [JsonProperty("percentage")]
        [XmlElement(ElementName = "percentage", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinFloat = 0, MaxFloat = 100)]//BEO-12569
        public double? Percentage { get; set; }
    }
}