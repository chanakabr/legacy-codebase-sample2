using ApiObjects.Base;
using ApiObjects.Response;

namespace ApiLogic.Base
{
    public interface ICrudHandler<T> where T : ICrudHandeledObject
    {
        GenericResponse<T> Add(int groupId, T objectToAdd);
        GenericResponse<T> Update(int groupId, T objectToUpdate);
        Status Delete(long id);

        // TODO SHIR - FINISH GENERIC LIST METHOD in ICrudHandler
        //GenericListResponse<T> List(U filter, KalturaFilter);
    }
}