using System;
using System.Collections.Generic;
using System.Drawing;

namespace ApiObjects.BulkUpload
{
    public class ExcelStructure
    {
        public List<string> OverviewInstructions { get; private set; }
        public Dictionary<ExcelColumnType, Color> ColumnsColors { get; private set; }
        public Dictionary<string, ExcelColumn> ExcelColumns { get; private set; }
        public Dictionary<string, object> MandatoryPropertyAndValueMap { get; private set; }

        public ExcelStructure(Dictionary<string, ExcelColumn> excelColumns, List<string> overviewInstructions, Dictionary<ExcelColumnType, Color> columnsColors, Dictionary<string, object> mandatoryPropertyAndValueMap)
        {
            // AssetStruct metadata keys are case insensitive
            ExcelColumns = excelColumns == null
                ? new Dictionary<string, ExcelColumn>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, ExcelColumn>(excelColumns, StringComparer.OrdinalIgnoreCase);

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

            if (mandatoryPropertyAndValueMap == null)
            {
                mandatoryPropertyAndValueMap = new Dictionary<string, object>();
            }
            MandatoryPropertyAndValueMap = mandatoryPropertyAndValueMap;
        }
    }
}