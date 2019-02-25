using System.Collections.Generic;

namespace ApiObjects.Excel
{
    public interface IExcelStructure
    {
        /// <summary>
        /// returns All columns to display in the Excel by the order they are displayed
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Dictionary<string, ExcelColumn> GetExcelColumns(int groupId, Dictionary<string, object> data = null);
    }
}
