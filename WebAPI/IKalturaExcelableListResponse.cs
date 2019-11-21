using System.Collections.Generic;

namespace WebAPI.App_Start
{
    public interface IKalturaExcelableListResponse : IKalturaExcelStructureManager
    {
        List<IKalturaExcelableObject> GetObjects();
    }
}