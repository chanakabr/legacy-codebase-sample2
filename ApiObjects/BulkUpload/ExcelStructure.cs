using System.Collections.Generic;
using System.Drawing;

namespace ApiObjects.BulkUpload
{
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