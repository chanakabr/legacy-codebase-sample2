using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public partial class KalturaPriceDetailsListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of price details
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPriceDetails> Prices { get; set; }
    }
}