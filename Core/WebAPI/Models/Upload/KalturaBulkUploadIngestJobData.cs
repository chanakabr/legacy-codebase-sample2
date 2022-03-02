using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// instructions for upload data type with xml
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadIngestJobData : KalturaBulkUploadJobData
    {
        /// <summary>
        /// Identifies the ingest profile that will handle the ingest of programs
        /// Ingest profiles are created separately using the ingest profile service
        /// </summary>
        [DataMember(Name = "ingestProfileId")]
        [JsonProperty(PropertyName = "ingestProfileId")]
        [XmlElement(ElementName = "ingestProfileId")]
        [SchemeProperty(MinInteger = 1)]
        public int? IngestProfileId { get; set; }

        /// <summary>
        /// By default, after the successful ingest, devices will be notified about changes in epg channels.
        /// This parameter disables this notification.
        /// </summary>
        [DataMember(Name = "disableEpgNotification")]
        [JsonProperty(PropertyName = "disableEpgNotification")]
        [XmlElement(ElementName = "disableEpgNotification")]
        public bool DisableEpgNotification { get; set; } = false;
    }
}
