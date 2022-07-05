using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
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