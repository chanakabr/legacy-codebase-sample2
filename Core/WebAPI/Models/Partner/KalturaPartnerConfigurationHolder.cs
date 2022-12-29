using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
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