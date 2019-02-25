using System.Collections.Generic;

namespace ApiObjects.Excel
{
    public interface IExcelObject
    {
        Dictionary<string, object> GetExcelValues(int groupId);
        void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns);
    }
}