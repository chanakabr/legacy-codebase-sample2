using ApiObjects.Base;
using ApiObjects.BulkUpload;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects
{
    public class DynamicListMap
    {
    }

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

        //public abstract U GetValues<U>() where U: List<IConvertible>;
        //public abstract void SetValues<T>(List<T> valuesToSet);

        public Dictionary<string, object> GetExcelValues(int groupId)
        {
            throw new NotImplementedException();
        }

        public void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns, IExcelStructureManager structureManager)
        {
            throw new NotImplementedException();
        }
    }

    public class UdidDynamicList : DynamicList
    {
        public List<string> Values { get; set; }
        
        public UdidDynamicList()
        {
            this.Type = DynamicListType.UDID;
        }

        //public override List<string> GetValues()
        //{
        //    return Values;
        //}

        //public override U GetValues<U>()
        //{
        //    return Values;
        //}
    }
}
