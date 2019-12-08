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
        START_DATE_DESC
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
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        public override KalturaSubscriptionOrderBy GetDefaultOrderByValue()
        {
            return KalturaSubscriptionOrderBy.START_DATE_ASC;
        }

        internal bool Validate()
        {
            if (string.IsNullOrEmpty(Ksql))
            {
                if (MediaFileIdEqual.HasValue && (!string.IsNullOrEmpty(SubscriptionIdIn) || !string.IsNullOrEmpty(ExternalIdIn)))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.subscriptionIdIn", "KalturaSubscriptionFilter.mediaFileIdEqual");
                }

                if (!string.IsNullOrEmpty(SubscriptionIdIn) && (MediaFileIdEqual.HasValue || !string.IsNullOrEmpty(ExternalIdIn)))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.subscriptionIdIn", "KalturaSubscriptionFilter.mediaFileIdEqual");
                }

                if (!string.IsNullOrEmpty(ExternalIdIn) && (MediaFileIdEqual.HasValue || !string.IsNullOrEmpty(SubscriptionIdIn)))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.productCodeIn", "KalturaSubscriptionFilter.mediaFileIdEqual");
                }

                if (string.IsNullOrEmpty(ExternalIdIn) && (!MediaFileIdEqual.HasValue || MediaFileIdEqual.Value == 0) && string.IsNullOrEmpty(SubscriptionIdIn) && CouponGroupIdEqual == null)
                {
                    return false;
                }
            }
            else
            {
                if (MediaFileIdEqual.HasValue || !string.IsNullOrEmpty(SubscriptionIdIn) || !string.IsNullOrEmpty(ExternalIdIn))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.Ksql", "KalturaSubscriptionFilter");
                }
            }

            return true;
        }

        internal string[] getSubscriptionIdIn()
        {
            if (string.IsNullOrEmpty(SubscriptionIdIn))
                return null;

            return SubscriptionIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        internal List<string> getExternalIdIn()
        {
            if (string.IsNullOrEmpty(ExternalIdIn))
                return null;

            return ExternalIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}