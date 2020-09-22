using ApiObjects.Base;
using ApiObjects.BulkUpload;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ApiObjects
{
    public enum DynamicListType
    {
        UDID = 1
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class DynamicList : ICrudHandeledObject
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("CreateDate")]
        public long CreateDate { get; set; }

        [JsonProperty("UpdateDate")]
        public long UpdateDate { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("UpdaterId")]
        public long UpdaterId { get; set; }

        [JsonProperty("Type")]
        public DynamicListType Type { get; protected set; }

        public void FillEmpty(DynamicList oldDynamicList)
        {
            this.CreateDate = oldDynamicList.CreateDate;

            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = oldDynamicList.Name;
            }
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class UdidDynamicList : DynamicList, IExcelStructureManager, IExcelObject
    {
        public const string UDID_COLUMN = "Udid";

        [ExcelColumn(ExcelColumnType.Basic, UDID_COLUMN, IsMandatory = true)]
        [JsonIgnore]
        public List<string> Values { get; set; }

        public UdidDynamicList()
        {
            this.Type = DynamicListType.UDID;
        }

        public Dictionary<string, object> GetExcelValues(int groupId)
        {
            // need to implement only if there is a requset from product to return values in excel (format=31)
            throw new NotImplementedException();
        }

        // TODO SHIR - UdidDynamicList.SetExcelValues
        public void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns, IExcelStructureManager structureManager)
        {
            throw new NotImplementedException();
        }

        public ExcelStructure GetExcelStructure(int groupId, Type objectType = null)
        {
            var excelColumns = new Dictionary<string, ExcelColumn>();
            var mandatoryPropertyAndValueMap = new Dictionary<string, object>();
            var systemNameToExcelAttribute = ExcelManager.GetSystemNameToProperyData(objectType);

            if (systemNameToExcelAttribute.ContainsKey(UdidDynamicList.UDID_COLUMN))
            {
                var excelColumn = ExcelManager.GetExcelColumnByAttribute(systemNameToExcelAttribute[UdidDynamicList.UDID_COLUMN], UdidDynamicList.UDID_COLUMN);
                var mediaAssetTypeColumnName = excelColumn.ToString();
                excelColumns.Add(mediaAssetTypeColumnName, excelColumn);
            }


            var excelStructure = new ExcelStructure(excelColumns, null, null, mandatoryPropertyAndValueMap);
            return excelStructure;
        }
    }
}