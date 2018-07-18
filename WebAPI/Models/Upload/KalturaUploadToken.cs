using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using WebAPI.Models.General;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Managers.Models;

namespace WebAPI.Models.Upload
{
    public enum KalturaUploadTokenStatus
    {
        PENDING = 0,
        FULL_UPLOAD = 1,
        CLOSED = 2
    }

    public partial class KalturaUploadToken : KalturaOTTObject
    {
        public KalturaUploadToken(UploadToken uploadToken) : base(null)
        {
            Id = uploadToken.UploadTokenId;
            Status = uploadToken.Status;
            FileSize = uploadToken.FileSize;
            CreateDate = uploadToken.CreateDate;
            UpdateDate = uploadToken.UpdateDate;
        }

        /// <summary>
        /// Upload-token identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaUploadTokenStatus? Status { get; set; }

        /// <summary>
        /// File size
        /// </summary>
        [DataMember(Name = "fileSize")]
        [JsonProperty("fileSize")]
        [XmlElement(ElementName = "fileSize", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public float? FileSize { get; set; }

        /// <summary>
        /// Specifies when was the Asset was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the Asset last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? UpdateDate { get; set; }

    }
}