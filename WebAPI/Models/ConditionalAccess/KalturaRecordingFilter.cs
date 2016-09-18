using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Filtering recordings
    /// </summary>
    [Serializable]
    public class KalturaRecordingFilter : KalturaFilter<KalturaRecordingOrderBy>
    {

        /// <summary>
        /// Recording Statuses
        /// </summary>
        [DataMember(Name = "statusIn")]
        [JsonProperty(PropertyName = "statusIn")]
        [XmlArray(ElementName = "statusIn", IsNullable = true)]
        [SchemeProperty(DynamicType = typeof(KalturaRecordingStatus))]
        public string StatusIn { get; set; }

        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(MaxLength = 2048)]
        public string FilterExpression { get; set; }

        //public string IdIn
        //{
        //    get;
        //    set;
        //}

        public override KalturaRecordingOrderBy GetDefaultOrderByValue()
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
                }
            }

            return recordingStatuses;

        }

    }
}