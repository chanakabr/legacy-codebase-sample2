using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// Bulk Upload
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadSummary : KalturaOTTObject
    {
        /// <summary>
        /// count of bulk upload in pending status
        /// </summary>
        [DataMember(Name = "pending")]
        [JsonProperty("pending")]
        [XmlElement(ElementName = "pending")]
        public long Pending { get; set; }

        /// <summary>
        /// count of bulk Uploaded in uploaded status
        /// </summary>
        [DataMember(Name = "uploaded")]
        [JsonProperty("uploaded")]
        [XmlElement(ElementName = "uploaded")]
        public long Uploaded { get; set; }

        /// <summary>
        /// count of bulk upload in queued status
        /// </summary>
        [DataMember(Name = "queued")]
        [JsonProperty("queued")]
        [XmlElement(ElementName = "queued")]
        public long Queued { get; set; }

        /// <summary>
        /// count of bulk upload in parsing status
        /// </summary>
        [DataMember(Name = "parsing")]
        [JsonProperty("parsing")]
        [XmlElement(ElementName = "Parsing")]
        public long Parsing { get; set; }


        /// <summary>
        /// count of bulk upload in processing status
        /// </summary>
        [DataMember(Name = "processing")]
        [JsonProperty("processing")]
        [XmlElement(ElementName = "processing")]
        public long Processing { get; set; }

        /// <summary>
        /// count of bulk upload in processed status
        /// </summary>
        [DataMember(Name = "processed")]
        [JsonProperty("processed")]
        [XmlElement(ElementName = "processed")]
        public long Processed { get; set; }

        /// <summary>
        /// count of bulk upload in success status
        /// </summary>
        [DataMember(Name = "success")]
        [JsonProperty("success")]
        [XmlElement(ElementName = "success")]
        public long Success { get; set; }

        /// <summary>
        /// count of bulk upload in partial status
        /// </summary>
        [DataMember(Name = "partial")]
        [JsonProperty("partial")]
        [XmlElement(ElementName = "partial")]
        public long Partial { get; set; }

        /// <summary>
        /// count of bulk upload in failed status
        /// </summary>
        [DataMember(Name = "failed")]
        [JsonProperty("failed")]
        [XmlElement(ElementName = "failed")]
        public long Failed { get; set; }

        /// <summary>
        /// count of bulk upload in fatal status
        /// </summary>
        [DataMember(Name = "fatal")]
        [JsonProperty("fatal")]
        [XmlElement(ElementName = "fatal")]
        public long Fatal { get; set; }

    }
}