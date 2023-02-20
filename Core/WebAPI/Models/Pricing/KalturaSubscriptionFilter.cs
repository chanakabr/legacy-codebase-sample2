using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public partial class KalturaSubscriptionFilter : KalturaFilter<KalturaSubscriptionOrderBy>
    {
        /// <summary>
        /// Comma separated subscription IDs to get the subscriptions by
        /// </summary>
        [DataMember(Name = "subscriptionIdIn")]
        [JsonProperty("subscriptionIdIn")]
        [XmlElement(ElementName = "subscriptionIdIn", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 1)]
        public string SubscriptionIdIn { get; set; }

        /// <summary>
        /// Media-file ID to get the subscriptions by
        /// </summary>
        [DataMember(Name = "mediaFileIdEqual")]
        [JsonProperty("mediaFileIdEqual")]
        [XmlElement(ElementName = "mediaFileIdEqual", IsNullable = true)]
        public int? MediaFileIdEqual { get; set; }

        /// <summary>
        /// Comma separated subscription external IDs to get the subscriptions by
        /// </summary>
        [DataMember(Name = "externalIdIn")]
        [JsonProperty("externalIdIn")]
        [XmlElement(ElementName = "externalIdIn", IsNullable = true)]
        public string ExternalIdIn { get; set; }

        /// <summary>
        /// couponGroupIdEqual
        /// </summary>
        [DataMember(Name = "couponGroupIdEqual")]
        [JsonProperty("couponGroupIdEqual")]
        [XmlElement(ElementName = "couponGroupIdEqual", IsNullable = true)]
        public int? CouponGroupIdEqual { get; set; }

        /// <summary>
        /// previewModuleIdEqual
        /// </summary>
        [DataMember(Name = "previewModuleIdEqual")]
        [JsonProperty("previewModuleIdEqual")]
        [XmlElement(ElementName = "previewModuleIdEqual", IsNullable = true)]
        public long? PreviewModuleIdEqual { get; set; }

        /// <summary>
        /// pricePlanIdEqual
        /// </summary>
        [DataMember(Name = "pricePlanIdEqual")]
        [JsonProperty("pricePlanIdEqual")]
        [XmlElement(ElementName = "pricePlanIdEqual", IsNullable = true)]
        public long? PricePlanIdEqual { get; set; }

        /// <summary>
        /// channelIdEqual 
        /// </summary>
        [DataMember(Name = "channelIdEqual")]
        [JsonProperty("channelIdEqual")]
        [XmlElement(ElementName = "channelIdEqual", IsNullable = true)]
        public long? ChannelIdEqual { get; set; }

        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        /// <summary>
        ///  return also inactive 
        /// </summary>
        [DataMember(Name = "alsoInactive")]
        [JsonProperty("alsoInactive")]
        [XmlElement(ElementName = "alsoInactive")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public bool? AlsoInactive { get; set; }

        /// <summary>
        /// comma separated values of KalturaSubscriptionDependencyType 
        /// return subscriptions associated by their subscription sets dependency Type
        /// </summary>
        [DataMember(Name = "dependencyTypeIn")]
        [JsonProperty("dependencyTypeIn")]
        [XmlElement(ElementName = "dependencyTypeIn")]
        [SchemeProperty(IsNullable = true, DynamicType = typeof(KalturaSubscriptionDependencyType), MinLength = 1)]
        public string DependencyTypeIn { get; set; }

        public override KalturaSubscriptionOrderBy GetDefaultOrderByValue()
        {
            return KalturaSubscriptionOrderBy.START_DATE_ASC;
        }
    }
}