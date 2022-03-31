using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.AssetPersonalMarkup
{
    /// <summary>
    /// Product Markup
    /// </summary>
    [Serializable]
    public partial class KalturaProductMarkup : KalturaOTTObject
    {
        /// <summary>
        /// Product Id
        /// </summary>
        [DataMember(Name = "productId")]
        [JsonProperty("productId")]
        [XmlElement("productId")]
        [SchemeProperty(MinLong = 1, ReadOnly = true)]
        public long ProductId { get; set; }

        /// <summary>
        /// Product Type
        /// </summary>
        [DataMember(Name = "productType")]
        [JsonProperty("productType")]
        [XmlElement("productType")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaTransactionType ProductType { get; set; }

        /// <summary>
        /// Is Entitled to this product
        /// </summary>
        [DataMember(Name = "isEntitled")]
        [JsonProperty("isEntitled")]
        [XmlElement("isEntitled")]
        [SchemeProperty(ReadOnly = true)]
        public bool IsEntitled { get; set; }
    }
}