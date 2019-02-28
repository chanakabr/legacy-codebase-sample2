using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ApiObjects.Excel
{
    // TODO SHIR - ADD TO DR
    public interface IKalturaBulkUploadObject
    {
    }

    public interface IKalturaExcelableObject : IKalturaBulkUploadObject
    {
        Dictionary<string, object> GetExcelValues(int groupId);
    }
    
    public interface IKalturaExcelStructure
    {
        List<string> GetExcelOverviewInstructions();

        Dictionary<ExcelColumnType, Color> GetExcelColumnsColors();

        /// <summary>
        /// return the columns of the excel in their display order
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Dictionary<string, ExcelColumn> GetExcelColumns(int groupId, Dictionary<string, object> data = null);

        List<IKalturaExcelableObject> GetObjects();
    }
    
    public class ExcelManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        // formats
        private const string DATE_FORMAT = "dd/MM/yyyy hh:mm:ss";
        private const string SAPARATOR = ":";

        // col headers
        private const string COLUMN_TYPE = "t";
        private const string COLUMN_SYSTEM_NAME = "n";
        private const string COLUMN_LANGUAGE = "l";
        private const string ITEM_INDEX = "i";

        //public static string GetColumnName(string systemName, string innerSystemName = null, string language = null, bool isMandatory = false)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    if (isMandatory)
        //    {
        //        sb.Append("*");
        //    }

        //    sb.Append(systemName);

        //    if (!string.IsNullOrEmpty(innerSystemName))
        //    {
        //        sb.Append(SAPARATOR + innerSystemName);
        //    }

        //    if (!string.IsNullOrEmpty(language))
        //    {
        //        sb.Append(SAPARATOR + language);
        //    }
            
        //    return sb.ToString();
        //}
        
        // TODO SHIR - TALK WITH ARTHUR ABOUT HOW TO GET THE FILE (BY PATH, uploadTokenId, BYTE[] ETC..)
        public static GenericListResponse<IExcelObject> Deserialize(Stream fileStream)
        {
            IExcelStructure m = null;
            //m.GetExcelColumns()
            // TODO SHIR - Deserialize EXCEL
            return null;
        }

        public static Dictionary<string, Tuple<ExcelColumnAttribute, PropertyInfo>> GetSystemNameToProperyData(Type type)
        {
            var systemNameToProperty = new Dictionary<string, Tuple<ExcelColumnAttribute, PropertyInfo>>();
            var properties = type.GetProperties();
            foreach (var prop in properties)
            {
                var customAttribut = prop.GetCustomAttributes(true).FirstOrDefault(x => x is ExcelColumnAttribute);
                var excelAttribute = customAttribut as ExcelColumnAttribute;
                if (excelAttribute != null && !systemNameToProperty.ContainsKey(excelAttribute.SystemName))
                {
                    systemNameToProperty.Add(excelAttribute.SystemName, new Tuple<ExcelColumnAttribute, PropertyInfo>(excelAttribute, prop));
                }
            }

            return systemNameToProperty;
        }

        public static ExcelColumn GetExcelColumnByAttribute(Tuple<ExcelColumnAttribute, PropertyInfo> excelProperty, string systemName, string language = null, string helpText = null, string innerSystemName = null)
        {
            ExcelColumn excelColumn = new ExcelColumn(excelProperty.Item1.ColumnType, systemName)
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