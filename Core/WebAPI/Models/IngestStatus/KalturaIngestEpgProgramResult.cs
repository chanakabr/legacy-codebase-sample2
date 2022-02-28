using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestEpgProgramResult : KalturaOTTObject
    {
        /// <summary>
        /// The unique ingested program id
        /// </summary>
        [DataMember(Name = "programId")]
        [JsonProperty("programId")]
        [XmlElement(ElementName = "programId")]
        public long? ProgramId { get; set; }

        /// <summary>
        /// An external program id
        /// </summary>
        [DataMember(Name = "externalProgramId")]
        [JsonProperty("externalProgramId")]
        [XmlElement(ElementName = "externalProgramId")]
        public string ExternalProgramId { get; set; }

        /// <summary>
        /// The id of the linear channel asset that the program belongs to
        /// </summary>
        [DataMember(Name = "linearChannelId")]
        [JsonProperty("linearChannelId")]
        [XmlElement(ElementName = "linearChannelId")]
        public long LinearChannelId { get; set; }

        /// <summary>
        /// The index of the program in the ingested file
        /// </summary>
        [DataMember(Name = "indexInFile")]
        [JsonProperty("indexInFile")]
        [XmlElement(ElementName = "indexInFile")]
        public long IndexInFile { get; set; }

        /// <summary>
        /// Program EPG start date. Date and time represented as epoch
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        public long StartDate { get; set; }

        /// <summary>
        /// Program EPG end date. Date and time represented as epoch
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        public long EndDate { get; set; }

        /// <summary>
        /// The program status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public KalturaIngestEpgProgramStatus Status { get; set; }

        /// <summary>
        /// List of errors. Note: error cause the data in question or the whole ingest to fail
        /// </summary>
        [DataMember(Name = "errors")]
        [JsonProperty("errors")]
        [XmlElement(ElementName = "errors")]
        public List<KalturaEpgIngestErrorMessage> Errors { get; set; }

        /// <summary>
        /// List of warnings. Note: warning cause no failure
        /// </summary>
        [DataMember(Name = "warnings")]
        [JsonProperty("warnings")]
        [XmlElement(ElementName = "warnings")]
        public List<KalturaMessage> Warnings { get; set; }
    }
}