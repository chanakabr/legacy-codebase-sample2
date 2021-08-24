using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Npvr Premium Service
    /// </summary>
    public partial class KalturaNpvrPremiumService : KalturaPremiumService
    {
        /// <summary>
        /// Quota in minutes
        /// </summary>
        [DataMember(Name = "quotaInMinutes")]
        [JsonProperty("quotaInMinutes")]
        [XmlElement(ElementName = "quotaInMinutes")]
        public long? QuotaInMinutes { get; set; }
    }
}