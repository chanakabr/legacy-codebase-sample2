using System;

namespace ApiObjects.BulkUpload
{
    public class ExcelParserException : Exception
    {
        public string ColumnName { get; private set; }
        public object Value { get; private set; }
        public string MethodName
        {
            get
            {
                if (this.InnerException != null && this.InnerException.TargetSite != null)
                {
                    return this.InnerException.TargetSite.Name;
                }

                return string.Empty;
            }
        }

        public ExcelParserException(Exception innerException, string columnName, object value)
            : base(innerException.Message, innerException)
        {
            this.ColumnName = columnName;
            this.Value = value;
        }
    }
}