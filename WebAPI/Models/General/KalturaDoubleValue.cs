using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// A string representation to return an array of doubles
    /// </summary>
    public partial class KalturaDoubleValue : KalturaValue
    {
        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [XmlElement("value")]
        [JsonProperty("value")]
        [ValidationException(SchemeValidationType.NULLABLE)]
        public double value { get; set; }

        internal override Dictionary<string, object> GetExcelValues(int groupId, Dictionary<string, object> data = null)
        {
            Dictionary<string, object> excelValues = new Dictionary<string, object>();

            var baseExcelValues = base.GetExcelValues(groupId, data);
            excelValues.TryAddRange(baseExcelValues);

            if (data != null && data.ContainsKey(TOPIC_TYPE) && data.ContainsKey(TOPIC_SYSTEM_NAME))
            {
                MetaType? topicType = data[TOPIC_TYPE] as MetaType?;
                string topicSystemName = data[TOPIC_SYSTEM_NAME] as string;

                if (topicType.HasValue && !string.IsNullOrEmpty(topicSystemName))
                {
                    var columnType = GetExcelMetaColumnType(topicType.Value);
                    if (columnType.HasValue && columnType.Value == ExcelColumnType.MetaNumber)
                    {
                        var metaHiddenName = ExcelFormatter.GetHiddenColumn(columnType.Value, topicSystemName);
                        excelValues.TryAdd(metaHiddenName, value.ToString());
                    }
                }
            }

            return excelValues;
        }
    }
}