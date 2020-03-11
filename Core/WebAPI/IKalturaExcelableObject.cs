using System.Collections.Generic;

namespace WebAPI.App_Start
{
    public interface IKalturaExcelableObject : IKalturaBulkUploadObject
    {
        Dictionary<string, object> GetExcelValues(int groupId);
    }
}