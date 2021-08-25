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
    public enum KalturaSubscriptionOrderBy
    {
        START_DATE_ASC,
        START_DATE_DESC,
        CREATE_DATE_ASC,
        CREATE_DATE_DESC,
        UPDATE_DATE_ASC,
        UPDATE_DATE_DESC,
        NAME_ASC,
        NAME_DESC
    }

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
        /// Root only
        /// </summary>
        [DataMember(Name = "alsoInactive")]
        [JsonProperty("alsoInactive")]
        [XmlElement(ElementName = "alsoInactive")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public bool? AlsoInactive { get; set; }

        public override KalturaSubscriptionOrderBy GetDefaultOrderByValue()
        {
            return KalturaSubscriptionOrderBy.NAME_ASC;
        }

        internal void Validate()
        {
            if (string.IsNullOrEmpty(Ksql))
            {
                if (MediaFileIdEqual.HasValue && (!string.IsNullOrEmpty(SubscriptionIdIn) || !string.IsNullOrEmpty(ExternalIdIn) || AlsoInactive.HasValue || 
                                                  PreviewModuleIdEqual.HasValue || PricePlanIdEqual.HasValue || ChannelIdEqual.HasValue))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.mediaFileIdEqual", "KalturaSubscriptionFilter");

                if (!string.IsNullOrEmpty(SubscriptionIdIn) && (MediaFileIdEqual.HasValue || !string.IsNullOrEmpty(ExternalIdIn) || 
                                                                AlsoInactive.HasValue || PreviewModuleIdEqual.HasValue || PricePlanIdEqual.HasValue || ChannelIdEqual.HasValue))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.subscriptionIdIn", "KalturaSubscriptionFilter");

                if (!string.IsNullOrEmpty(ExternalIdIn) && (MediaFileIdEqual.HasValue || !string.IsNullOrEmpty(SubscriptionIdIn) || AlsoInactive.HasValue ||
                                                            PreviewModuleIdEqual.HasValue || PricePlanIdEqual.HasValue || ChannelIdEqual.HasValue))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.productCodeIn", "KalturaSubscriptionFilter");

                if (CouponGroupIdEqual.HasValue && (PreviewModuleIdEqual.HasValue || PricePlanIdEqual.HasValue || ChannelIdEqual.HasValue))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.couponGroupIdEqual", "KalturaSubscriptionFilter");
                if (PreviewModuleIdEqual.HasValue && (PricePlanIdEqual.HasValue || ChannelIdEqual.HasValue))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.PreviewModuleIdEqual", "KalturaSubscriptionFilter");
                if(PricePlanIdEqual.HasValue && ChannelIdEqual.HasValue)
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.PricePlanIdEqual", "KalturaSubscriptionFilter.ChannelIdEqual");
            }
            else
            {
                if (MediaFileIdEqual.HasValue || !string.IsNullOrEmpty(SubscriptionIdIn) || !string.IsNullOrEmpty(ExternalIdIn) ||  AlsoInactive.HasValue ||
                    PreviewModuleIdEqual.HasValue || PricePlanIdEqual.HasValue || ChannelIdEqual.HasValue)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.Ksql", "KalturaSubscriptionFilter");
                }
            }

            if (!string.IsNullOrEmpty(SubscriptionIdIn))
            {
                GetItemsIn<List<long>, long>(this.SubscriptionIdIn, "subscriptionIdIn", true);
            }
        }

        internal List<long> getSubscriptionIdIn()
        {
            if (string.IsNullOrEmpty(SubscriptionIdIn))
                return null;

            return GetItemsIn<List<long>, long>(this.SubscriptionIdIn, "subscriptionIdIn", true);
        }

        internal List<string> getExternalIdIn()
        {
            if (string.IsNullOrEmpty(ExternalIdIn))
                return null;

            return ExternalIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}