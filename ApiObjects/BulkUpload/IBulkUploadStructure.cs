using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.BulkUpload
{
    public interface IBulkUploadStructure
    {
    }

    public interface IExcelStructure : IBulkUploadStructure
    {
        /// <summary>
        /// return the columns of the excel in their display order
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        ExcelStructure GetExcelStructure(int groupId, Dictionary<string, object> data = null);
    }
}
