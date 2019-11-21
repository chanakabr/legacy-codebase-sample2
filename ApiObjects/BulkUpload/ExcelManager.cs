using System;
using System.Collections.Generic;
using System.Reflection;

namespace ApiObjects.BulkUpload
{
    public class ExcelManager
    {
        public static Dictionary<string, Tuple<ExcelColumnAttribute, PropertyInfo>> GetSystemNameToProperyData(Type type)
        {
            var systemNameToProperty = new Dictionary<string, Tuple<ExcelColumnAttribute, PropertyInfo>>();
            var properties = type.GetProperties();
            foreach (var prop in properties)
            {
                var excelAttribute = prop.GetCustomAttribute<ExcelColumnAttribute>(true);
                if (excelAttribute != null && !systemNameToProperty.ContainsKey(excelAttribute.SystemName))
                {
                    systemNameToProperty.Add(excelAttribute.SystemName, new Tuple<ExcelColumnAttribute, PropertyInfo>(excelAttribute, prop));
                }
            }

            return systemNameToProperty;
        }

        public static ExcelColumn GetExcelColumnByAttribute(Tuple<ExcelColumnAttribute, PropertyInfo> excelProperty, string systemName, string language = null, string helpText = null, string innerSystemName = null)
        {
            var excelColumn = new ExcelColumn(excelProperty.Item1.ColumnType, systemName)
            {
                Property = excelProperty.Item2,
                IsMandatory = excelProperty.Item1.IsMandatory,
                IsUniqueMeta = excelProperty.Item1.IsUniqueMeta,
                Language = language,
                HelpText = helpText,
                InnerSystemName = innerSystemName
            };

            return excelColumn;
        }
    }
}