using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    // TODO SHIR - SEE WHAT TO DO WITH DESIRELAIZE
    public class ExcelManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        // col headers
        private const string COLUMN_TYPE = "t";
        private const string COLUMN_SYSTEM_NAME = "n";
        private const string COLUMN_LANGUAGE = "l";
        private const string ITEM_INDEX = "i";

        public static string GetHiddenColumn(ExcelColumnType columnType, string systemName, string language = null, int? itemIndex = null)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                { COLUMN_TYPE, columnType.ToString() },
                { COLUMN_SYSTEM_NAME, systemName }
            };

            if (!string.IsNullOrEmpty(language))
            {
                dic.Add(COLUMN_LANGUAGE, language);
            }

            if (itemIndex.HasValue)
            {
                dic.Add(ITEM_INDEX, itemIndex.Value.ToString());
            }

            return dic.ToJSON();
        }

        public static GenericListResponse<T> Deserialize<T>() where T : class , IKalturaExcelableObject
        {
            // TODO SHIR - Deserialize EXCEL
            return null;
        }
    }
}