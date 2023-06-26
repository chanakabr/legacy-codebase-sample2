using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestStatusVodConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Defines whether partner in question enabled core ingest status service.
        /// </summary>
        [DataMember(Name = "isSupported")]
        [JsonProperty("isSupported")]
        [XmlElement(ElementName = "isSupported")]
        [SchemeProperty(IsNullable = true)]
        public bool? IsSupported { get; set; }

        /// <summary>
        /// Defines the time in seconds that the service retain information about ingest status.
        /// </summary>
        [DataMember(Name = "retainingPeriod")]
        [JsonProperty("retainingPeriod")]
        [XmlElement(ElementName = "retainingPeriod")]
        [SchemeProperty(IsNullable = true, MinInteger = 0)]
        public long? RetainingPeriod { get; set; }
    }
}
