using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// ItemPrice list
    /// </summary>
    [DataContract(Name = "ItemPrice", Namespace = "")]
    [XmlRoot("ItemPrice")]
    public class KalturaItemPricesList
    {
        /// <summary>
        /// A list of item prices
        /// </summary>
        [DataMember(Name = "item_prices")]
        [JsonProperty("item_prices")]
        public List<KalturaItemPrice> ItemPrice { get; set; }
    }
}