using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public enum KalturaPartnerConfigurationType
    {
        DEFAULTPAYMENTGATEWAY,
        ENABLEPAYMENTGATEWAYSELECTION,
        OSSADAPTER
    }

    /// <summary>
    /// Holder object for channel enrichment enum
    /// </summary>    
    public class KalturaPartnerConfigurationHolder : KalturaOTTObject
    {
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaPartnerConfigurationType type { get; set; }
    }
}