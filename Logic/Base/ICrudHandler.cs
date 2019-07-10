using ApiObjects.Base;
using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiLogic.Base
{
    public interface ICrudHandler<T> where T : ICrudHandeledObject
    {
        GenericResponse<T> Add(int groupId, T objectToAdd, Dictionary<string, object> funcParams = null);
        GenericResponse<T> Update(int groupId, T objectToUpdate);
        Status Delete(long id);

        // TODO SHIR - FINISH GENERIC LIST METHOD in ICrudHandler
        //GenericListResponse<T> List(U filter, KalturaFilter);
    }
}