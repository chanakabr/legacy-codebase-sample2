using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.Partner;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner billing configuration
    /// </summary>
    [OldStandard("partnerConfigurationType", "partner_configuration_type")]
    public class KalturaBillingPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// configuration value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// partner configuration type
        /// </summary>
        [DataMember(Name = "partnerConfigurationType")]
        [JsonProperty("partnerConfigurationType")]
        [XmlElement(ElementName = "partnerConfigurationType", IsNullable = true)]
        public KalturaPartnerConfigurationHolder Type { get; set; }
    }
}