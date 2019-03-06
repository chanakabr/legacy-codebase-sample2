using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects.BulkUpload
{
    public interface IBulkUploadObject
    {
        BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, BulkUploadResultStatus status, int index, Status errorStatus);
        string DistributedTask { get; }
        string RoutingKey { get; }
        bool Enqueue(int groupId, long userId, long bulkUploadId, BulkUploadJobAction jobAction, int resultIndex);
    }

    public interface IExcelObject : IBulkUploadObject
    {
        Dictionary<string, object> GetExcelValues(int groupId);
        void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns);
    }
}