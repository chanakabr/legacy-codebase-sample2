using ApiObjects.Base;
using ApiObjects.BulkUpload;
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

    public class DynamicList : ICrudHandeledObject
    {
        public long Id { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }
        public string Name { get; set; }
        public long UpdaterId { get; set; }
        public DynamicListType Type { get; protected set; }

        public void FillEmpty(DynamicList oldDynamicList)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = oldDynamicList.Name;
            }
        }
    }

    public class UdidDynamicList : DynamicList, IExcelStructureManager, IExcelObject
    {
        public const string UDID_COLUMN = "Udid";

        [ExcelColumn(ExcelColumnType.Basic, UDID_COLUMN, IsMandatory = true)]
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