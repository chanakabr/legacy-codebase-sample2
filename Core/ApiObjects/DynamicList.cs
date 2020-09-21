using ApiObjects.Base;
using ApiObjects.BulkUpload;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects
{
    public enum DynamicListType
    {
        UDID = 1
    }

    public class DynamicList : ICrudHandeledObject, IExcelObject
    {
        public long Id { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }
        public string Name { get; set; }
        public long UpdaterId { get; set; }
        public DynamicListType Type { get; protected set; }

        public Dictionary<string, object> GetExcelValues(int groupId)
        {
            throw new NotImplementedException();
        }

        public void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns, IExcelStructureManager structureManager)
        {
            throw new NotImplementedException();
        }

        public void FillEmpty(DynamicList oldDynamicList)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = oldDynamicList.Name;
            }
        }
    }

    public class UdidDynamicList : DynamicList
    {
        public List<string> Values { get; set; }
        
        public UdidDynamicList()
        {
            this.Type = DynamicListType.UDID;
        }
    }
}
