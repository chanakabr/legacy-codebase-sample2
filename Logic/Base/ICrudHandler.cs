using ApiObjects.Base;
using ApiObjects.Response;
using System;

namespace ApiLogic.Base
{
    public interface ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter>
        where IdentifierT : IConvertible
    {
        GenericResponse<ICrudHandeledObject> Add(ContextData contextData, ICrudHandeledObject objectToAdd);
        GenericResponse<ICrudHandeledObject> Update(ContextData contextData, ICrudHandeledObject objectToUpdate);
        Status Delete(ContextData contextData, IdentifierT id);
        GenericResponse<ICrudHandeledObject> Get(ContextData groupId, IdentifierT id);
        GenericListResponse<ICrudHandeledObject> List(ICrudFilter filter);
    }
}