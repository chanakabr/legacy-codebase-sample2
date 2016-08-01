using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
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
        [Obsolete]
        public KalturaPartnerConfigurationHolder PartnerConfigurationType { get; set; }

        /// <summary>
        /// partner configuration type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaPartnerConfigurationType Type { get; set; }

        internal KalturaPartnerConfigurationType getType()
        {
            if (PartnerConfigurationType != null)
                return PartnerConfigurationType.type;

            return Type;
        }
    }
}