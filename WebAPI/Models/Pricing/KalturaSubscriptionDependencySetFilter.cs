using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Pricing
{
    public class KalturaSubscriptionDependencySetFilter : KalturaSubscriptionSetFilter
    {
        /// <summary>
        /// Comma separated identifiers
        /// </summary>
        [DataMember(Name = "baseSubscriptionIdIn")]
        [JsonProperty(PropertyName = "baseSubscriptionIdIn")]
        [XmlElement(ElementName = "baseSubscriptionIdIn", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 1)]
        public string BaseSubscriptionIdIn { get; set; }


        public List<long> GetBaseSubscriptionIdContains()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(BaseSubscriptionIdIn))
            {
                string[] stringValues = BaseSubscriptionIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSubscriptionDependencySetFilter.BaseSubscriptionIdIn");
                    }
                }
            }

            return list;
        }

    }
}