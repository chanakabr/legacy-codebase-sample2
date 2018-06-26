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
    /// Prices list
    /// </summary>
    [DataContract(Name = "KalturaProductsPriceListResponse", Namespace = "")]
    [XmlRoot("KalturaProductsPriceListResponse")]
    public partial class KalturaProductsPriceListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of prices
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaProductPrice> ProductsPrices { get; set; }
    }
}