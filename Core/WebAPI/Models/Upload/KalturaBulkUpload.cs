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
        /// The user who uploaded this bulk
        /// </summary>
        [DataMember(Name = "uploadedByUserId")]
        [JsonProperty("uploadedByUserId ")]
        [XmlElement(ElementName = "uploadedByUserId ")]
        [SchemeProperty(ReadOnly = true)]
        public long UploadedByUserId { get; set; }

        /// <summary>
        /// A list of results
        /// </summary>
        [DataMember(Name = "results")]
        [JsonProperty("results")]
        [XmlArray(ElementName = "results", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaBulkUploadResult> Results { get; set; }

        /// <summary>
        /// A list of errors
        /// </summary>
        [DataMember(Name = "errors")]
        [JsonProperty("error")]
        [XmlArray(ElementName = "error", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaMessage> Errors { get; set; }
    }
}