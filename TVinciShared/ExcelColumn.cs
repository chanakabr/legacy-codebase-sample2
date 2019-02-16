namespace TVinciShared
{
    public enum ExcelColumnType
    {
        Basic,
        MetaText,
        MetaNumber,
        MetaBool,
        MetaDate,
        MetaMultilingual,
        Tag,
        File,
        Image,
        Rule
    }

    public class KalturaExcelColumn
    {
        public ExcelColumnType ColumnType { get; set; }
        public string HiddenName { get; set; }
        public string FriendlyName { get; set; }
        public string HelpText { get; set; }

        public KalturaExcelColumn(ExcelColumnType excelColumnType, string hiddenName, string friendlyName, string helpText = null)
        {
            ColumnType = excelColumnType;
            HiddenName = hiddenName;
            FriendlyName = friendlyName;
            HelpText = helpText;
        }
    }
}