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
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "subscriptionIdIn or mediaFileIdEqual must be filtered");
            }

            if (!string.IsNullOrEmpty(SubscriptionIdIn) && MediaFileIdEqual.HasValue)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "only one of subscriptionIdIn and mediaFileIdEqual can be filtered, but not both");
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