using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner billing configuration
    /// </summary>
    public partial class KalturaPaymentPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// configuration for unified billing cycles.
        /// </summary>
        [DataMember(Name = "unifiedBillingCycles")]
        [JsonProperty("unifiedBillingCycles")]
        [XmlElement(ElementName = "unifiedBillingCycles", IsNullable = true)]
        public List<KalturaUnifiedBillingCycle> UnifiedBillingCycles { get; set; }
    }
}