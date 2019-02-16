using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Array of translated strings
    /// </summary>
    public partial class KalturaMultilingualStringValueArray : KalturaOTTObject
    {
        internal const string TAG_SYSTEM_NAME = "TAG_SYSTEM_NAME";
        
        /// <summary>
        /// List of string values
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaMultilingualStringValue> Objects { get; set; }

        protected override void Init()
        {
            base.Init();
            Objects = new List<KalturaMultilingualStringValue>();
        }

        internal override Dictionary<string, object> GetExcelValues(int groupId, Dictionary<string, object> data = null)
        {
            Dictionary<string, object> excelValues = new Dictionary<string, object>();

            var baseExcelValues = base.GetExcelValues(groupId, data);
            excelValues.TryAddRange(baseExcelValues);

            if (Objects != null && Objects.Count > 0 && data != null && data.ContainsKey(TAG_SYSTEM_NAME))
            {
                string tagSystemName = data[TAG_SYSTEM_NAME] as string;
                if (!string.IsNullOrEmpty(tagSystemName))
                {
                    var tagHiddenName = ExcelFormatter.GetHiddenColumn(ExcelColumnType.Tag, tagSystemName);
                    if (!excelValues.ContainsKey(tagHiddenName))
                    {
                        var defaultLanugageValues = string.Join(",", Objects.Where(x => x.value != null).Select(x => x.value.GetDefaultLanugageValue()).Where(x => x != null));
                        excelValues.TryAdd(tagHiddenName, defaultLanugageValues);
                    }
                }
            }

            return excelValues;
        }
    }
}