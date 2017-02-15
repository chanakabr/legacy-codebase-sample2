using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Npvr Premium Service
    /// </summary>
    public class KalturaNpvrPremiumService : KalturaPremiumService
    {
        /// <summary>
        /// Quota in minutes
        /// </summary>
        [DataMember(Name = "quotaInMinutes")]
        [JsonProperty("quotaInMinutes")]
        [XmlElement(ElementName = "quotaInMinutes")]
        [SchemeProperty(ReadOnly = true)]
        public long? QuotaInMinutes { get; set; }
    }
}