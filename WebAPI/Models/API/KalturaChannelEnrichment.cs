using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
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
        NPVRSupport = 128,
        Catchup = 256,
        Parental = 512,
        DTTRegion = 1024,
        AtHome = 2048
    }

    /// <summary>
    /// Holder object for channel enrichment enum
    /// </summary>    
    public class KalturaChannelEnrichmentHolder : KalturaOTTObject
    {
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement("type")]
        public KalturaChannelEnrichment type { get; set; }
    }
}