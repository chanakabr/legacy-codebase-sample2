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

namespace WebAPI.Models.General
{
    /// <summary>
    /// Container for translation
    /// </summary>
    public partial class KalturaTranslationToken : KalturaOTTObject
    {
        internal const string DEFALUT_LANGUAGE_CODE = "DEFALUT_LANGUAGE_CODE";

        /// <summary>
        /// Language code
        /// </summary>
        [DataMember(Name = "language")]
        [JsonProperty("language")]
        [XmlElement(ElementName = "language")]
        public string Language { get; set; }

        /// <summary>
        /// Translated value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }

        internal override Dictionary<string, object> GetExcelValues(int groupId, Dictionary<string, object> data = null)
        {
            Dictionary<string, object> excelValues = new Dictionary<string, object>();

            var baseExcelValues = base.GetExcelValues(groupId, data);
            excelValues.TryAddRange(baseExcelValues);

            if (data != null && data.ContainsKey(KalturaValue.TOPIC_TYPE) && data.ContainsKey(KalturaValue.TOPIC_SYSTEM_NAME) && data.ContainsKey(DEFALUT_LANGUAGE_CODE))
            {
                MetaType? topicType = data[KalturaValue.TOPIC_TYPE] as MetaType?;
                string topicSystemName = data[KalturaValue.TOPIC_SYSTEM_NAME] as string;
                string defaultLangCode = data[DEFALUT_LANGUAGE_CODE] as string;
                if (topicType.HasValue && !string.IsNullOrEmpty(topicSystemName) && !string.IsNullOrEmpty(defaultLangCode))
                {
                    var columnType = KalturaValue.GetExcelMetaColumnType(topicType.Value);
                    if (columnType.HasValue && columnType.Value == ExcelColumnType.MetaMultilingual)
                    {
                        string tokenHiddenName;
                        if (defaultLangCode.Equals(this.Language))
                        {
                            tokenHiddenName = ExcelFormatter.GetHiddenColumn(columnType.Value, topicSystemName);
                        }
                        else
                        {
                            tokenHiddenName = ExcelFormatter.GetHiddenColumn(columnType.Value, topicSystemName, this.Language);
                        }

                        excelValues.TryAdd(tokenHiddenName, this.Value);
                    }
                }
            }

            return excelValues;
        }
    }
}