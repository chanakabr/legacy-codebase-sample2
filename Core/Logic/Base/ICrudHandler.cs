using ApiObjects.Base;
using ApiObjects.Response;
using System;

namespace ApiLogic.Base
{
    public interface ICrudHandler<ICrudHandeledObject, IdentifierT>
        where IdentifierT : IConvertible
    {
        Status Delete(ContextData contextData, IdentifierT id);
        GenericResponse<ICrudHandeledObject> Get(ContextData contextData, IdentifierT id);
    }
}