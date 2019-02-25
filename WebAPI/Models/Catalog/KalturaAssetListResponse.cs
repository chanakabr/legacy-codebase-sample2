using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Excel;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset wrapper
    /// </summary>
    [Serializable]
    public partial class KalturaAssetListResponse : KalturaListResponse, IKalturaExcelStructure
    {
        private static readonly List<string> OVERVIEW_INSTRUCTIONS = new List<string>()
        {
            "//Templae Overview",
            "//The first row shows (Label) ((identifier)) - (Field Type)",
            "// Label is the friendly field name. It can only be edited via the Operator Console UI",
            "// Identifier is the system name. It can not be edited via this form",
            "// Columns marked in * are required",
            "// Field Types:",
            "// Text fields are strings (example: The Godfather)",
            "// Numeric fields are integers and suppot a single value (example: 3600). Double integers are accepted (example 7.9)",
            "// Switch values should be TRUE or FALSE",
            "// Tags should be seperated by commas (example: drama, action, family)",
            "// Date&Time (format: dd/MM/yyyy hh:mm:ss)",
            "// The help text can be edited via the Operator Console"
        };

        private static readonly Dictionary<ExcelColumnType, Color> COLUMNS_COLORS = new Dictionary<ExcelColumnType, Color>()
        {
            { ExcelColumnType.Basic, Color.Orange },
            { ExcelColumnType.Meta, Color.Green },
            { ExcelColumnType.Tag, Color.Green },
            { ExcelColumnType.File, Color.Blue },
            { ExcelColumnType.Image, Color.Gray },
            { ExcelColumnType.Rule, Color.Yellow }
        };

        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAsset> Objects { get; set; }

        public List<string> GetExcelOverviewInstructions()
        {
            return OVERVIEW_INSTRUCTIONS;
        }

        public Dictionary<ExcelColumnType, Color> GetExcelColumnsColors()
        {
            return COLUMNS_COLORS;
        }

        public Dictionary<string, ExcelColumn> GetExcelColumns(int groupId, Dictionary<string, object> data = null)
        {
            Dictionary<string, ExcelColumn> excelColumns = new Dictionary<string, ExcelColumn>();
            
            if (Objects == null || Objects.Count == 0)
            {
                return excelColumns;
            }

            var duplicates = Objects.GroupBy(x => x.getType()).Select(x => x.Key).ToList();
            if (duplicates.Count > 1)
            {
                return excelColumns;
            }
            int mediaType = duplicates[0];

            data = new Dictionary<string, object>()
            {
                { MediaAsset.MEDIA_TYPE, mediaType }
            };

            excelColumns.TryAddRange(this.Objects[0].GetExcelColumns(groupId, data));

            return excelColumns;
        }

        public List<IKalturaExcelableObject> GetObjects()
        {
            return this.Objects.ToList<IKalturaExcelableObject>();
        }
    }
}