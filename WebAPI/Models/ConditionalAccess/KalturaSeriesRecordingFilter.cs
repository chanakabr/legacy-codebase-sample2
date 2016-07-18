using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
     /// <summary>
    /// Filtering recordings
    /// </summary>
    [Serializable]
    public class KalturaSeriesRecordingFilter : KalturaFilter<KalturaSeriesRecordingOrderBy>
    {  
        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression", IsNullable = true)]
        [ValidationException(SchemaValidationType.FILTER_SUFFIX)]
        public string FilterExpression { get; set; }

        public override KalturaSeriesRecordingOrderBy GetDefaultOrderByValue()
        {
            return KalturaSeriesRecordingOrderBy.START_DATE_DESC;
        }
    }
}