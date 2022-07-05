using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using WebAPI.Exceptions;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaPurchaseBase : KalturaOTTObject
    {
        /// <summary>
        /// Identifier for the package from which this content is offered
        /// </summary>
        [DataMember(Name = "productId")]
        [JsonProperty("productId")]
        [XmlElement(ElementName = "productId")]
        [SchemeProperty(MinInteger = 1)]
        public int ProductId { get; set; }

        /// <summary>
        /// Identifier for the content to purchase. Relevant only if Product type = PPV
        /// </summary>
        [DataMember(Name = "contentId")]
        [JsonProperty("contentId")]
        [XmlElement(ElementName = "contentId")]
        public int? ContentId { get; set; }

        /// <summary>
        /// Package type. Possible values: PPV, Subscription, Collection
        /// </summary>
        [DataMember(Name = "productType")]
        [JsonProperty("productType")]
        [XmlElement(ElementName = "productType")]
        public KalturaTransactionType ProductType { get; set; }

        /// <summary>
        /// Additional data for the adapter
        /// </summary>
        [DataMember(Name = "adapterData")]
        [JsonProperty("adapterData")]
        [XmlElement(ElementName = "adapterData")]
        [SchemeProperty(MaxLength = 1024)]
        public string AdapterData { get; set; }
    }
}