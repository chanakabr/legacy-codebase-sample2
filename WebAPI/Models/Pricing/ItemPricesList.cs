using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Pricing
{
    [DataContract(Name = "ItemPrice", Namespace = "")]
    [XmlRoot("ItemPrice")]
    public class ItemPricesList
    {
        [DataMember(Name = "item_prices")]
        [JsonProperty("item_prices")]
        public List<ItemPrice> ItemPrice { get; set; }
    }
}