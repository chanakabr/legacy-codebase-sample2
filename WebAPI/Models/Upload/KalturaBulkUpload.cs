using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// 
    /// </summary>
    public class KalturaBulkUpload : KalturaOTTObject
    {

        /// <summary>
        /// BulkUpload identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaBatchJobStatus? Status { get; set; }

        /// <summary>
        /// File size
        /// </summary>
        [DataMember(Name = "fileSize")]
        [JsonProperty("fileSize")]
        [XmlElement(ElementName = "fileSize", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public float? FileSize { get; set; }

        /// <summary>
        /// The number of entries sent during the bulk upload
        /// </summary>
        [DataMember(Name = "numberOfEntries")]
        [JsonProperty("numberOfEntries")]
        [XmlElement(ElementName = "numberOfEntries")]
        [SchemeProperty(ReadOnly = true)]
        public int NumberOfEntries { get; set; }

        /// <summary>
        /// Name of the bulk upload file
        /// </summary>
        [DataMember(Name = "fileName")]
        [JsonProperty("fileName")]
        [XmlElement(ElementName = "fileName")]
        [SchemeProperty(ReadOnly = true)]
        public string FileName { get; set; }

        /// <summary>
        /// Description of the bulk upload file
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        [SchemeProperty(ReadOnly = true)]
        public string Description { get; set; }

        /// <summary>
        /// Error description for the bulk upload file
        /// </summary>
        [DataMember(Name = "errorDescription")]
        [JsonProperty("errorDescription")]
        [XmlElement(ElementName = "errorDescription")]
        [SchemeProperty(ReadOnly = true)]
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Error number for the bulk upload file
        /// </summary>
        [DataMember(Name = "errorNumber")]
        [JsonProperty("errorNumber")]
        [XmlElement(ElementName = "errorNumber")]
        [SchemeProperty(ReadOnly = true)]
        public int ErrorNumber { get; set; }

        /// <summary>
        /// Identifier of the user who created the bulk upload
        /// </summary>
        [DataMember(Name = "uploadedByUserId")]
        [JsonProperty("uploadedByUserId")]
        [XmlElement(ElementName = "uploadedByUserId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long UploadedByUserId { get; set; }

        /// <summary>
        /// Specifies when was the bulk upload was created. Date and time represented as epoch
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the bulk upload last updated. Date and time represented as epoch
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? UpdateDate { get; set; }
    }

    public enum KalturaBatchJobStatus
    {
        PENDING = 0,
        QUEUED = 1,
        PROCESSING = 2,
        PROCESSED = 3,
        MOVEFILE = 4,
        FINISHED = 5,
        FAILED = 6,
        ABORTED = 7,
        ALMOST_DONE = 8,
        RETRY = 9,
        FATAL = 10,
        DONT_PROCESS = 11,
        FINISHED_PARTIALLY = 12
    }
}