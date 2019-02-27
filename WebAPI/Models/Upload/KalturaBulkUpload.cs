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
    /// Bulk Upload
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUpload : KalturaOTTObject 
    {
        /// <summary>
        /// Bulk identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// File Name
        /// </summary>
        [DataMember(Name = "fileName")]
        [JsonProperty("fileName")]
        [XmlElement(ElementName = "fileName")]
        [SchemeProperty(ReadOnly = true)]
        public string FileName { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaBulkUploadJobStatus Status { get; set; }

        /// <summary>
        /// Action
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty("action")]
        [XmlElement(ElementName = "action")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaBulkUploadJobAction Action { get; set; }

        /// <summary>
        /// Total number of objects in file
        /// </summary>
        [DataMember(Name = "numOfObjects")]
        [XmlElement("numOfObjects", IsNullable = true)]
        [JsonProperty("numOfObjects")]
        [SchemeProperty(ReadOnly = true)]
        public int? NumOfObjects { get; set; }

        /// <summary>
        /// Specifies when was the bulk action created. Date and time represented as epoch
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the bulk action last updated. Date and time represented as epoch
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }
        
        /// <summary>
        /// A list of results
        /// </summary>
        [DataMember(Name = "results")]
        [JsonProperty("results")]
        [XmlArray(ElementName = "results", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaBulkUploadResult> Results { get; set; }
    }

    // TODO SHIR - CREATE NEW STATUS BY SPEC
    public enum KalturaBulkUploadJobStatus
    {
        PENDING = 1,
        UPLOADED = 2,
        QUEUED = 3,
        //PROCESSING = 2,
        //PROCESSED = 3,
        //MOVEFILE = 4,
        //FINISHED = 5,
        //FAILED = 6,
        //ABORTED = 7,
        //RETRY = 9,
        //FATAL = 10,
        //DONT_PROCESS = 11,
        //FINISHED_PARTIALLY = 12
    }

    public enum KalturaBulkUploadJobAction
    {
        Upsert = 1,
        Delete = 2
    }
}