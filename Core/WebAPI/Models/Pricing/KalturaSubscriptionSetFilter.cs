using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public partial class KalturaSubscriptionSetFilter : KalturaFilter<KalturaSubscriptionSetOrderBy>
    {
        /// <summary>
        /// Comma separated identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlArray(ElementName = "idIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public string IdIn { get; set; }

        /// <summary>
        /// Comma separated subscription identifiers
        /// </summary>
        [DataMember(Name = "subscriptionIdContains")]
        [JsonProperty("subscriptionIdContains")]
        [XmlArray(ElementName = "subscriptionIdContains", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string SubscriptionIdContains { get; set; }

        /// <summary>
        /// Subscription Type
        /// </summary>
        [DataMember(Name = "typeEqual")]
        [JsonProperty("typeEqual")]
        [XmlArray(ElementName = "typeEqual", IsNullable = true)]
        [XmlArrayItem(ElementName = "typeEqual")]
        public KalturaSubscriptionSetType? TypeEqual { get; set; }
        
        public override KalturaSubscriptionSetOrderBy GetDefaultOrderByValue()
        {
            return KalturaSubscriptionSetOrderBy.NAME_ASC;
        }
    }
}