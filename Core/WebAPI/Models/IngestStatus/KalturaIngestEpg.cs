using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestEpg : KalturaOTTObject
    {
        /// <summary>
        /// Unique id of the ingest job in question
        /// </summary>
        [DataMember(Name = "ingestId")]
        [JsonProperty("ingestId")]
        [XmlElement(ElementName = "ingestId")]
        [SchemeProperty(ReadOnly = true, MinLong = 0)]
        public long IngestId { get; set; }

        /// <summary>
        /// The ingested file name without its extention
        /// </summary>
        [DataMember(Name = "ingestName")]
        [JsonProperty("ingestName")]
        [XmlElement(ElementName = "ingestName")]
        [SchemeProperty(MinLength = 0)]
        public string IngestName { get; set; }

        /// <summary>
        /// The ingested file name extention
        /// </summary>
        [DataMember(Name = "ingestFilenameExtension")]
        [JsonProperty("ingestFilenameExtension")]
        [XmlElement(ElementName = "ingestFilenameExtension")]
        [SchemeProperty(MinLength = 0)]
        public string IngestFilenameExtension { get; set; }

        /// <summary>
        /// The ingest job created date and time. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createdDate")]
        [JsonProperty("createdDate")]
        [XmlElement(ElementName = "createdDate")]
        public long CreatedDate { get; set; }

        /// <summary>
        /// The user id of the addFromBulkUpload caller.
        /// </summary>
        [DataMember(Name = "ingestedByUserId")]
        [JsonProperty("ingestedByUserId")]
        [XmlElement(ElementName = "ingestedByUserId")]
        public long IngestedByUserId { get; set; }

        /// <summary>
        /// The ingest job completed date and time. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "completedDate")]
        [JsonProperty("completedDate")]
        [XmlElement(ElementName = "completedDate")]
        [SchemeProperty(IsNullable = true)]
        public long? CompletedDate { get; set; }

        /// <summary>
        /// The ingest profile id that of the ingest job.
        /// </summary>
        [DataMember(Name = "ingestProfileId")]
        [JsonProperty("ingestProfileId")]
        [XmlElement(ElementName = "ingestProfileId")]
        [SchemeProperty(IsNullable = true)]
        public long? IngestProfileId { get; set; }

        /// <summary>
        /// The ingest profile id that of the ingest job.
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public KalturaIngestStatus Status { get; set; }
    }
}


