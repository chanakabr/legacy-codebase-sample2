using ApiObjects.BulkUpload;

namespace WebAPI.App_Start
{
    public interface IKalturaExcelStructureManager : IKalturaBulkUploadStructureManager
    {
        ExcelStructure GetExcelStructure(int groupId);
    }
}