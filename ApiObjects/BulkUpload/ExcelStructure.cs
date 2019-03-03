using System.Collections.Generic;
using System.Drawing;

namespace ApiObjects.BulkUpload
{
    public interface IExcelStructure
    {
        /// <summary>
        /// return the columns of the excel in their display order
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        //Dictionary<string, ExcelColumn> GetExcelColumns(int groupId, Dictionary<string, object> data = null);
        //List<string> GetExcelOverviewInstructions();
        //Dictionary<ExcelColumnType, Color> GetExcelColumnsColors();
        ExcelStructure GetExcelStructure(int groupId, Dictionary<string, object> data = null);
    }

    public class ExcelStructure
    {
        public List<string> OverviewInstructions { get; private set; }
        public Dictionary<ExcelColumnType, Color> ColumnsColors { get; private set; }
        public Dictionary<string, ExcelColumn> ExcelColumns { get; private set; }

        public ExcelStructure(Dictionary<string, ExcelColumn> excelColumns, List<string> overviewInstructions, Dictionary<ExcelColumnType, Color> columnsColors)
        {
            if (excelColumns == null)
            {
                excelColumns = new Dictionary<string, ExcelColumn>();
            }
            ExcelColumns = excelColumns;

            if (overviewInstructions == null)
            {
                overviewInstructions = new List<string>();
            }
            OverviewInstructions = overviewInstructions;

            if (columnsColors == null)
            {
                columnsColors = new Dictionary<ExcelColumnType, Color>();
            }
            ColumnsColors = columnsColors;
        }
    }
}
