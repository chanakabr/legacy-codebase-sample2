using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public enum KalturaPartnerConfigurationType
    {
        DefaultPaymentGateway,
        EnablePaymentGatewaySelection,
        OSSAdapter,
        Concurrency,
        General,
        ObjectVirtualAsset,
        Commerce,
        Playback,
        Payment,
        Catalog,
        Security,
        Opc,
        Base,
        CustomFields
    }

    /// <summary>
    /// Holder object for channel enrichment enum
    /// </summary>    
    [Obsolete]
    public partial class KalturaPartnerConfigurationHolder : KalturaOTTObject
    {
        /// <summary>
        /// Partner configuration type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaPartnerConfigurationType type { get; set; }
    }
}