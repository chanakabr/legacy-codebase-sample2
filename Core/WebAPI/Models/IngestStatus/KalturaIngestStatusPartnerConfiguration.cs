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
    }
}
