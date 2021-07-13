using ApiObjects.Response;
using ApiObjects.SearchObjects;

namespace ApiLogic.Catalog.CatalogManagement.Repositories
{
    public interface ILabelRepository
    {
        GenericResponse<LabelValue> Add(long groupId, LabelValue labelValue, long updaterId);
        GenericResponse<LabelValue> Update(long groupId, LabelValue labelValue, long updaterId);
        Status Delete(int groupId, long labelId, long updaterId);
        GenericListResponse<LabelValue> List(long groupId);
        void InvalidateCache(long groupId);
    }
}
