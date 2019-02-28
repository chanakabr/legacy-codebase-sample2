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
    public enum KalturaBulkUploadOrderBy
    {
        NONE,
        UPDATE_DATE,
        STATUS
    }

    /// <summary>
    /// Bulk Upload Filter
    /// </summary>
    public partial class KalturaBulkUploadFilter : KalturaPersistedFilter<KalturaBulkUploadOrderBy>
    {
        /// <summary>
        /// Indicates which Bulk Upload list to return by this KalturaBatchUploadJobStatus.
        /// </summary>
        [DataMember(Name = "statusEqual")]
        [JsonProperty("statusEqual")]
        [XmlElement(ElementName = "statusEqual", IsNullable = true)]
        public KalturaBulkUploadJobStatus? StatusEqual { get; set; }

        public override KalturaBulkUploadOrderBy GetDefaultOrderByValue()
        {
            return KalturaBulkUploadOrderBy.NONE;
        }
    }
}