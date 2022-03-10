using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// Bulk Upload Filter
    /// </summary>
    public partial class KalturaBulkUploadFilter : KalturaFilter<KalturaBulkUploadOrderBy>
    {
        /// <summary>
        /// bulk objects Type name (must be type of KalturaOTTObject)
        /// </summary>
        [DataMember(Name = "bulkObjectTypeEqual")]
        [JsonProperty("bulkObjectTypeEqual")]
        [XmlElement(ElementName = "bulkObjectTypeEqual")]
        public string BulkObjectTypeEqual { get; set; }

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
        /// Comma separated list of BulkUpload Statuses to search\filter
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
    }
}