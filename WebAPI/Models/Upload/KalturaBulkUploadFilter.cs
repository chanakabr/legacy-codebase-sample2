using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    public enum KalturaBulkUploadOrderBy
    {
        NONE,
        UPDATE_DATE_ASC,
        UPDATE_DATE_DESC,
    }

    /// <summary>
    /// Bulk Upload Filter
    /// </summary>
    public partial class KalturaBulkUploadFilter : KalturaFilter<KalturaBulkUploadOrderBy>
    {
        /// <summary>
        /// File's objectType name (must be type of KalturaOTTObject)
        /// </summary>
        [DataMember(Name = "fileObjectNameEqual")]
        [JsonProperty("fileObjectNameEqual")]
        [XmlElement(ElementName = "fileObjectNameEqual")]
        public string FileObjectNameEqual { get; set; }

        /// <summary>
        /// upload date to search within (search in the last 60 days)
        /// </summary>
        [DataMember(Name = "createDateGreaterThanOrEqual")]
        [JsonProperty("createDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "createDateGreaterThanOrEqual", IsNullable = true)]
        public long? CreateDateGreaterThanOrEqual { get; set; }
        
        /// <summary>
        /// Indicates if to get the BulkUpload list that created by current user or by the entire group.
        /// </summary>
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [DataMember(Name = "userIdEqualCurrent")]
        [JsonProperty("userIdEqualCurrent")]
        [XmlElement(ElementName = "userIdEqualCurrent", IsNullable = true)]
        public bool? UserIdEqualCurrent { get; set; }

        /// <summary>
        /// Indicates if to get the BulkUpload list that are stil in OnGoing process or finished.
        /// </summary>
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [DataMember(Name = "shouldGetOnGoingBulkUploads")]
        [JsonProperty("shouldGetOnGoingBulkUploads")]
        [XmlElement(ElementName = "shouldGetOnGoingBulkUploads")]
        public bool ShouldGetOnGoingBulkUploads { get; set; }

        public override KalturaBulkUploadOrderBy GetDefaultOrderByValue()
        {
            return KalturaBulkUploadOrderBy.NONE;
        }

        internal void Validate()
        {
            if (string.IsNullOrEmpty(FileObjectNameEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "fileObjectNameEqual");
            }
        }
    }
}