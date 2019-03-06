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
        UPDATE_DATE
    }

    /// <summary>
    /// Bulk Upload Filter
    /// </summary>
    public partial class KalturaBulkUploadFilter : KalturaFilter<KalturaBulkUploadOrderBy>
    {
        /// <summary>
        /// upload date to search within.
        /// </summary>
        [DataMember(Name = "uploadedOnEqual")]
        [JsonProperty("uploadedOnEqual")]
        [XmlElement(ElementName = "uploadedOnEqual", IsNullable = true)]
        public long? UploadedOnEqual { get; set; }

        /// <summary>
        /// Date Comparison Type.
        /// </summary>
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [DataMember(Name = "dateComparisonType")]
        [JsonProperty("dateComparisonType")]
        [XmlElement(ElementName = "dateComparisonType", IsNullable = true)]
        public KalturaDateComparisonType? DateComparisonType { get; set; }

        /// <summary>
        /// List of KalturaBulkUploadJobStatus to search within.
        /// </summary>
        [DataMember(Name = "statusIn")]
        [JsonProperty("statusIn")]
        [XmlElement(ElementName = "statusIn", IsNullable = true)]
        public string StatusIn { get; set; }

        /// <summary>
        /// Indicates if to get the BulkUpload list that created by current user or by the entire group.
        /// </summary>
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [DataMember(Name = "userIdEqualCurrent")]
        [JsonProperty("userIdEqualCurrent")]
        [XmlElement(ElementName = "userIdEqualCurrent", IsNullable = true)]
        public bool? UserIdEqualCurrent { get; set; }

        public override KalturaBulkUploadOrderBy GetDefaultOrderByValue()
        {
            return KalturaBulkUploadOrderBy.NONE;
        }

        internal void Validate()
        {
            if (UploadedOnEqual.HasValue && !DateComparisonType.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "dateComparisonType");
            }

            if (DateComparisonType.HasValue && !UploadedOnEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "uploadedOnEqual");
            }
        }
    }
}