using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.Partner;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner billing configuration
    /// </summary>
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
        [DataMember(Name = "partner_configuration_type")]
        [JsonProperty("partner_configuration_type")]
        [XmlElement(ElementName = "partner_configuration_type", IsNullable=true)]
        public KalturaPartnerConfigurationHolder Type { get; set; }
    }
}