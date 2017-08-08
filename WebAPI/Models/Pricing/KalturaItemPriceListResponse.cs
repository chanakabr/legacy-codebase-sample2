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
    /// ItemPrice list
    /// </summary>
    [DataContract(Name = "ItemPrice", Namespace = "")]
    [XmlRoot("ItemPrice")]
    [Obsolete]
    public class KalturaItemPriceListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of item prices
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaItemPrice> ItemPrice { get; set; }
    }
}