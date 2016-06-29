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
        CLIENTLOCATION = 1,
        USERID = 2,
        HOUSEHOLDID = 4,
        DEVICEID = 8,
        DEVICETYPE = 16,
        UTCOFFSET = 32,
        LANGUAGE = 64,
        NPVRSUPPORT = 128,
        CATCHUP = 256,
        PARENTAL = 512,
        DTTREGION = 1024,
        ATHOME = 2048
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