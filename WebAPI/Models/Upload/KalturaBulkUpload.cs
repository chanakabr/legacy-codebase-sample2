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
    public class KalturaBulk : KalturaOTTObject
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
        /// Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaBatchJobStatus? Status { get; set; }

        /// <summary>
        /// Specifies when was the bulk action created. Date and time represented as epoch
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the bulk action last updated. Date and time represented as epoch
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