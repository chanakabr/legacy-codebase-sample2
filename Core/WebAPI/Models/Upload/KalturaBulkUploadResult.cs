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
    /// Bulk Upload Result
    /// </summary>
    [Serializable]
    public abstract partial class KalturaBulkUploadResult : KalturaOTTObject
    {
        /// <summary>
        /// the result ObjectId (assetId, userId etc)
        /// </summary>
        [DataMember(Name = "objectId")]
        [JsonProperty("objectId")]
        [XmlElement(ElementName = "objectId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? ObjectId { get; set; }

        /// <summary>
        /// result index
        /// </summary>
        [DataMember(Name = "index")]
        [JsonProperty("index")]
        [XmlElement(ElementName = "index")]
        [SchemeProperty(ReadOnly = true)]
        public int Index { get; set; }

        /// <summary>
        /// Bulk upload identifier
        /// </summary>
        [DataMember(Name = "bulkUploadId")]
        [JsonProperty("bulkUploadId")]
        [XmlElement(ElementName = "bulkUploadId")]
        [SchemeProperty(ReadOnly = true)]
        public long BulkUploadId { get; set; }

        /// <summary>
        /// status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaBulkUploadResultStatus Status { get; set; }

        /// <summary>
        /// A list of errors
        /// </summary>
        [DataMember(Name = "errors")]
        [JsonProperty("error")]
        [XmlArray(ElementName = "error", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaMessage> Errors { get; set; }

        /// <summary>
        /// A list of warnings
        /// </summary>
        [DataMember(Name = "warnings")]
        [JsonProperty("warnings")]
        [XmlArray(ElementName = "warnings", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaMessage> Warnings { get; set; }
    }
}