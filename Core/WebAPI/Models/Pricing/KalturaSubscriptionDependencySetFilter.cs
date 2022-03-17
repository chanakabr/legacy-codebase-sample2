using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Pricing
{
    public partial class KalturaSubscriptionDependencySetFilter : KalturaSubscriptionSetFilter
    {
        /// <summary>
        /// Comma separated identifiers
        /// </summary>
        [DataMember(Name = "baseSubscriptionIdIn")]
        [JsonProperty(PropertyName = "baseSubscriptionIdIn")]
        [XmlElement(ElementName = "baseSubscriptionIdIn", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 1)]
        public string BaseSubscriptionIdIn { get; set; }
    }
}