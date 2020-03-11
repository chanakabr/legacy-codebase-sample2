using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public enum KalturaChannelEnrichment
    {
        ClientLocation = 1,
        UserId = 2,
        HouseholdId = 4,
        DeviceId = 8,
        DeviceType = 16,
        UTCOffset = 32,
        Language = 64,
        DTTRegion = 1024,
    }

    /// <summary>
    /// Holder object for channel enrichment enum
    /// </summary>    
    public partial class KalturaChannelEnrichmentHolder : KalturaOTTObject
    {
        /// <summary>
        /// Enrichment type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement("type")]
        public KalturaChannelEnrichment type { get; set; }
    }
}