using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Subscription details
    /// </summary>
    public class KalturaSubscriptionSet : KalturaOTTObject
    {
        /// <summary>
        /// SubscriptionSet identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// SubscriptionSet name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name", IsNullable = true)]
        public string Name { get; set; }

        /// <summary>
        /// A list of comma separated subscription ids associated with this set ordered by priority ascending
        /// </summary>
        [DataMember(Name = "subscriptions")]
        [JsonProperty("subscriptions")]
        [XmlArray(ElementName = "subscriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(DynamicMinInt = 1)]
        public string SubscriptionIds { get; set; }

        public List<long> GetSubscriptionIds()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(SubscriptionIds))
            {
                string[] stringValues = SubscriptionIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSubscriptionSet.SubscriptionIds");
                    }
                }
            }

            return list;
        }

    }
}