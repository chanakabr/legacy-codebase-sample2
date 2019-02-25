using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Excel
{
    public class ExcelColumn
    {
        private const string NAME_SAPARATOR = ":";

        public ExcelColumnType ColumnType { get; private set; }
        public PropertyInfo Property { get; set; }
        public string SystemName { get; private set; }
        public string InnerSystemName { get; set; }
        public string Language { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsUniqueMeta { get; set; }
        public string HelpText { get; set; }

        public ExcelColumn(ExcelColumnType columnType, string systemName)
        {
            this.ColumnType = columnType;
            this.SystemName = systemName;
        }

        /// <summary>
        /// Returns full column name (Unique)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetFullColumnName(this.SystemName, this.Language, this.InnerSystemName, this.IsMandatory);
        }
        
        public static string GetFullColumnName(string systemName, string language = null, string innerSystemName = null, bool isMandatory = false)
        {
            StringBuilder sb = new StringBuilder();

            if (isMandatory)
            {
                sb.Append("*");
            }

            sb.Append(systemName);

            if (!string.IsNullOrEmpty(innerSystemName))
            {
                sb.Append(NAME_SAPARATOR + innerSystemName);
            }

            if (!string.IsNullOrEmpty(language))
            {
                sb.Append(NAME_SAPARATOR + language);
            }

            return sb.ToString();
        }
    }

    public class ExcelColumnAttribute : Attribute
    {
        public ExcelColumnType ColumnType { get; private set; }
        public string SystemName { get; private set; }
        public bool IsMandatory { get; set; }
        public bool IsUniqueMeta { get; set; }

        public ExcelColumnAttribute(ExcelColumnType columnType, string systemName)
        {
            this.ColumnType = columnType;
            this.SystemName = systemName;
        }
    }

    public class ExcelValueAttribute : Attribute
    {

    }

    public enum ExcelColumnType
    {
        Basic,
        Meta,
        Tag,
        File,
        Image,
        Rule
    }
}
