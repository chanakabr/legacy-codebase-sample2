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
    /// <summary>
    /// Subscription Dependency Set
    /// </summary>
    public class KalturaSubscriptionDependencySet : KalturaSubscriptionSet
    {

        /// <summary>
        /// Base Subscription identifier
        /// </summary>
        [DataMember(Name = "baseSubscriptionId")]
        [JsonProperty("baseSubscriptionId")]
        [XmlElement(ElementName = "baseSubscriptionId")]       
        public long BaseSubscriptionId { get; set; }

    }
}