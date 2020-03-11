using System;
using System.Collections.Generic;

namespace ApiObjects.BulkUpload
{
    public interface IBulkUploadStructureManager
    {
    }

    public interface IExcelStructureManager : IBulkUploadStructureManager
    {
        /// <summary>
        /// return the columns of the excel in their display order
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        ExcelStructure GetExcelStructure(int groupId, Type objectType = null);
    }
}