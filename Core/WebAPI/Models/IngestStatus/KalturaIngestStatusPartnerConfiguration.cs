using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    [SchemeClass(AnyOf = new [] { "epg", "vod" })]
    public partial class KalturaIngestStatusPartnerConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Defines the epg configuration of the partner.
        /// </summary>
        [DataMember(Name = "epg")]
        [JsonProperty("epg")]
        [XmlElement(ElementName = "epg")]
        [SchemeProperty(IsNullable = true)]
        public KalturaIngestStatusEpgConfiguration Epg { get; set; }

        /// <summary>
        /// Defines the vod configuration of the partner.
        /// </summary>
        [DataMember(Name = "vod")]
        [JsonProperty("vod")]
        [XmlElement(ElementName = "vod")]
        [SchemeProperty(IsNullable = true)]
        public KalturaIngestStatusVodConfiguration Vod { get; set; }
    }
}
