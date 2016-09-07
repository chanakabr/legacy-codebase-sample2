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
        [XmlArray(ElementName = "subscriptionIdIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public string SubscriptionIdIn { get; set; }

        /// <summary>
        /// Media-file identifier to get the subscriptions by
        /// </summary>
        [DataMember(Name = "mediaFileIdEqual")]
        [JsonProperty("mediaFileIdEqual")]
        [XmlArray(ElementName = "mediaFileIdEqual", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public int? MediaFileIdEqual { get; set; }

        public override KalturaSubscriptionOrderBy GetDefaultOrderByValue()
        {
            return KalturaSubscriptionOrderBy.START_DATE_ASC;
        }

        internal void Validate()
        {
            if (string.IsNullOrEmpty(SubscriptionIdIn) && !MediaFileIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaSubscriptionFilter.subscriptionIdIn, KalturaSubscriptionFilter.mediaFileIdEqual");
            }

            if (!string.IsNullOrEmpty(SubscriptionIdIn) && MediaFileIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.subscriptionIdIn", "KalturaSubscriptionFilter.mediaFileIdEqual");
            }
        }

        internal string[] getSubscriptionIdIn()
        {
            if (string.IsNullOrEmpty(SubscriptionIdIn))
                return null;

            return SubscriptionIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}