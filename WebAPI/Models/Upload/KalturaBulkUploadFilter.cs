using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using TVinciShared;
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
        private const double MIN_RECORD_DAYS_TO_WATCH = 60;

        /// <summary>
        /// File's objectType name (must be type of KalturaOTTObject)
        /// </summary>
        [DataMember(Name = "bulkObjectNameEqual")]
        [JsonProperty("bulkObjectNameEqual")]
        [XmlElement(ElementName = "bulkObjectNameEqual")]
        public string BulkObjectNameEqual { get; set; }

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
        [DataMember(Name = "uploadedByUserIdEqualCurrent")]
        [JsonProperty("uploadedByUserIdEqualCurrent")]
        [XmlElement(ElementName = "uploadedByUserIdEqualCurrent", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool? UploadedByUserIdEqualCurrent { get; set; }

        /// <summary>
        /// BulkUpload Statuses to search\filter
        /// </summary>
        [DataMember(Name = "statusIn")]
        [JsonProperty(PropertyName = "statusIn")]
        [XmlArray(ElementName = "statusIn", IsNullable = true)]
        [SchemeProperty(DynamicType = typeof(KalturaBulkUploadJobStatus))]
        public string StatusIn { get; set; }

        public override KalturaBulkUploadOrderBy GetDefaultOrderByValue()
        {
            return KalturaBulkUploadOrderBy.NONE;
        }

        internal void Validate(out DateTime createDate)
        {
            if (string.IsNullOrEmpty(BulkObjectNameEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bulkObjectNameEqual");
            }

            if (CreateDateGreaterThanOrEqual.HasValue)
            {
                createDate = DateUtils.UtcUnixTimestampSecondsToDateTime(this.CreateDateGreaterThanOrEqual.Value);
                if (createDate.AddDays(MIN_RECORD_DAYS_TO_WATCH) < DateTime.UtcNow)
                {
                    var minCreateDate = DateTime.UtcNow.AddDays(-MIN_RECORD_DAYS_TO_WATCH).ToUtcUnixTimestampSeconds();
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "createDateGreaterThanOrEqual", minCreateDate);
                }
            }
            else
            {
                createDate = DateTime.UtcNow.AddDays(-MIN_RECORD_DAYS_TO_WATCH);
            }
        }
    }
}