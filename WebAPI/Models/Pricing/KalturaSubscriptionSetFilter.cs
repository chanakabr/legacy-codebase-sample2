using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public enum KalturaSubscriptionSetOrderBy
    {
        NAME_ASC,
        NAME_DESC
    }

    public class KalturaSubscriptionSetFilter : KalturaFilter<KalturaSubscriptionSetOrderBy>
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

        internal void Validate()
        {
            if (!string.IsNullOrEmpty(IdIn) && !string.IsNullOrEmpty(SubscriptionIdContains))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionSetFilter.idIn, KalturaSubscriptionSetFilter.subscriptionIdContains");
            }
        }

        public List<long> GetIdIn()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSubscriptionSetFilter.idIn");
                    }
                }
            }

            return list;
        }

        public List<long> GetSubscriptionIdContains()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(SubscriptionIdContains))
            {
                string[] stringValues = SubscriptionIdContains.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSubscriptionSetFilter.SubscriptionIdContains");
                    }
                }
            }

            return list;
        }

    }
}