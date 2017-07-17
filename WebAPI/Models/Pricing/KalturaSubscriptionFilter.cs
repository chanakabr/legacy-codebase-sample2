using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public enum KalturaSubscriptionOrderBy
    {
        START_DATE_ASC,
        START_DATE_DESC
    }

    public class KalturaSubscriptionFilter : KalturaFilter<KalturaSubscriptionOrderBy>
    {
        /// <summary>
        /// Comma separated subscription identifiers or file identifier (only 1) to get the subscriptions by
        /// </summary>
        [DataMember(Name = "subscriptionIdIn")]
        [JsonProperty("subscriptionIdIn")]
        [XmlElement(ElementName = "subscriptionIdIn", IsNullable = true)]
        public string SubscriptionIdIn { get; set; }

        /// <summary>
        /// Media-file identifier to get the subscriptions by
        /// </summary>
        [DataMember(Name = "mediaFileIdEqual")]
        [JsonProperty("mediaFileIdEqual")]
        [XmlElement(ElementName = "mediaFileIdEqual", IsNullable = true)]
        public int? MediaFileIdEqual { get; set; }

        /// <summary>
        /// Media-file identifier to get the subscriptions by
        /// </summary>
        [DataMember(Name = "externalIdIn")]
        [JsonProperty("externalIdIn")]
        [XmlElement(ElementName = "externalIdIn", IsNullable = true)]
        public string ExternalIdIn { get; set; }

        public override KalturaSubscriptionOrderBy GetDefaultOrderByValue()
        {
            return KalturaSubscriptionOrderBy.START_DATE_ASC;
        }

        internal void Validate()
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