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
            "//Template Overview",
            "//Batch upload to OPC is limited to 200 media assets per file",
            "//The media asset information is grouped and color-coded by OPC tabs: Metadata, Availability, Images, Playback Files, Rules",
            "//The first two columns indicate the media asset type. If you are updating an existing media asset, do not modify these columns",
            "//To create a new asset, enter the media asset type name (column A) and an asterisk (*) in column B",
            "//To create a new asset with a specific External ID, enter your chosen ID in column B",
            "//The first row of the media asset table includes the field identifier and field type (example: Name (Text Field))",
            "//Secondary languages are marked with “:“. (example: Name:spanish (Text Field))",
            "//You may edit this template to only include the fields you wish to update, but columns marked with * are mandatory",
            "//Field types information:",
            "//Text fields are strings (example: The Godfather)",
            "//Numeric fields are integers single value is supported (example: 3600). Double integers are also accepted (example 7.9)",
            "//Switch values should be TRUE or FALSE",
            "//Tags should be separated by commas (example: drama, action, family)",
            "//Date&Time (format: dd/mm/yyyy hh:mm:ss)",
            "//Playback file types columns include the file identifier and the field type as it appears in OPC (example: AndroidMain:External Id)",
            "//Image types columns include the image identifier and the field type as it appears in OPC (example: BoxCoverEnglish:Image URL)",
            "//PPV for file types can be updated using the PPV name separated by “;”. (example: [PPV name];[PPVname]. PPV with dates[PPV1];[Start];[End];[PPV2];[Start];[End]…)",
            "//Image columns are generated with no value by default. If you wish to update an image, enter the URL in the appropriate Image URL column",
            "//For rules, provide the rule name as it appears in OPC."
       };

        private static readonly Dictionary<ExcelColumnType, Color> COLUMNS_COLORS = new Dictionary<ExcelColumnType, Color>()
        {
            { ExcelColumnType.Basic, Color.Red },
            { ExcelColumnType.Meta, Color.FromArgb(51, 204, 51) }, // green
            { ExcelColumnType.Tag, Color.FromArgb(51, 204, 51) }, // green
            { ExcelColumnType.AvailabilityMeta, Color.FromArgb(255, 153, 0) }, //orange
            { ExcelColumnType.File,  Color.FromArgb(0, 204, 255) }, // Cyan
            { ExcelColumnType.Image, Color.FromArgb(204, 0, 102) }, // Purple
            { ExcelColumnType.Rule, Color.FromArgb(255, 234, 0) } // Yellow
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