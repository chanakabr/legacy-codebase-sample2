using ApiObjects.BulkUpload;

namespace WebAPI.App_Start
{
    public interface IKalturaExcelStructure : IKalturaBulkUploadStructure
    {
        ExcelStructure GetExcelStructure(int groupId);
    }
}