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
    public abstract partial class KalturaCrudObject<ICrudHandeledObject, IdentifierT> : KalturaOTTObjectSupportNullable
        where IdentifierT : IConvertible
    {
        //TODO SHIR - SET ALL AS VIRTUAL
        internal abstract ICrudHandler<ICrudHandeledObject, IdentifierT> Handler { get; }
        public virtual void ValidateForAdd() { }
        internal virtual void ValidateForUpdate() { }
        internal abstract void SetId(IdentifierT id);
        internal virtual GenericResponse<ICrudHandeledObject> Add(ContextData contextData) { throw new NotImplementedException(); }
        internal virtual GenericResponse<ICrudHandeledObject> Update(ContextData contextData) { throw new NotImplementedException(); }

        protected override void Init()
        {
            base.Init();
        }
    }
}