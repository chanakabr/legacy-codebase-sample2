using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;
using WebAPI.Models.Upload;
using WebAPI.Utils;

namespace WebAPI.Managers.Models
{
    public class UploadToken
    {
        /// <summary>
        /// The id of the upload token
        /// </summary>
        [DataMember(Name = "upload_token_id")]
        [JsonProperty("upload_token_id")]
        [XmlElement(ElementName = "upload_token_id")]
        public string UploadTokenId { get; set; }

        /// <summary>
        /// Partner identifier
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int PartnerId { get; set; }

        /// <summary>
        /// Application token status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [Obsolete]
        public KalturaUploadTokenStatus Status { get; set; }

        /// <summary>
        /// File size
        /// </summary>
        [DataMember(Name = "fileSize")]
        [JsonProperty("fileSize")]
        [XmlElement(ElementName = "fileSize", IsNullable = true)]
        public float? FileSize { get; set; }

        /// <summary>
        /// Specifies when was the Asset was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate", IsNullable = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the Asset last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate", IsNullable = true)]
        public long UpdateDate { get; set; }

        public UploadToken()
        {
        }

        public UploadToken(int groupId)
        {
            UploadTokenId = Utils.Utils.Generate32LengthGuid();
            PartnerId = groupId;
            Status = KalturaUploadTokenStatus.PENDING;
            CreateDate = SerializationUtils.GetCurrentUtcTimeInUnixTimestamp();
            UpdateDate = CreateDate;
        }
    }
}