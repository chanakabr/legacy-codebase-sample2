using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Filtering recordings
    /// </summary>
    [Serializable]
    public class KalturaRecordingFilter : KalturaFilter
    {

        /// <summary>
        /// Recording Statuses
        /// </summary>
        [DataMember(Name = "statusIn")]
        [JsonProperty(PropertyName = "statusIn")]
        [XmlArray(ElementName = "statusIn", IsNullable = true)]        
        public string StatusIn { get; set; }

        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression", IsNullable = true)]
        [ValidationException(SchemaValidationType.FILTER_SUFFIX)]
        public string FilterExpression { get; set; }

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemaValidationType.FILTER_SUFFIX)]
        public KalturaRecordingOrderBy? OrderBy { get; set; }

        public override object GetDefaultOrderByValue()
        {
            return KalturaRecordingOrderBy.START_DATE_DESC;
        }

        public List<KalturaRecordingStatus> ConvertStatusIn()
        {
            List<KalturaRecordingStatus> recordingStatuses = null;
            if (!string.IsNullOrEmpty(StatusIn))
            {
                string[] recordingStatusInrecordingStatuses = StatusIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                recordingStatuses = new List<KalturaRecordingStatus>();
                foreach (string sRecordingStatus in recordingStatusInrecordingStatuses)
                {
                    KalturaRecordingStatus recordingStatus;
                    if (Enum.TryParse<KalturaRecordingStatus>(sRecordingStatus.ToUpper(), out recordingStatus))
                    {
                        recordingStatuses.Add(recordingStatus);
                    }
                    else
                    {
                        throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, string.Format("Filter.StatusIn contains invalid status {0}", sRecordingStatus));
                    }
                }
            }

            return recordingStatuses;

        }

    }
}