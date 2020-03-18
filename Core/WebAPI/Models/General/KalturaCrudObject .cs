using ApiLogic.Base;
using System;
using System.Collections.Generic;
using ApiObjects.Response;
using ApiObjects.Base;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Base class that have CRUD actions 
    /// </summary>
    [Serializable]
    public abstract partial class KalturaCrudObject<ICrudHandeledObject, IdentifierT> : KalturaOTTObject
        where IdentifierT : IConvertible
    {
        //TODO SHIR - SET ALL AS VIRTUAL
        internal abstract ICrudHandler<ICrudHandeledObject, IdentifierT> Handler { get; }
        internal virtual void ValidateForAdd() { }
        internal virtual void ValidateForUpdate() { }
        internal abstract void SetId(IdentifierT id);
        internal virtual GenericResponse<ICrudHandeledObject> Add(ContextData contextData) { return null; }
        internal virtual GenericResponse<ICrudHandeledObject> Update(ContextData contextData) { return null; }

        protected override void Init()
        {
            base.Init();
        }
    }
}