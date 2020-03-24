using System.Collections.Generic;

namespace WebAPI.App_Start
{
    public interface IKalturaExcelableListResponse : IKalturaExcelStructure
    {
        List<IKalturaExcelableObject> GetObjects();
    }
}