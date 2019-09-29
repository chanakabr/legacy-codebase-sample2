using ApiLogic.Base;
using System;
using System.Collections.Generic;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Base class that have CRUD actions 
    /// </summary>
    [Serializable]
    public abstract partial class KalturaCrudObject<ICrudHandeledObject, IdentifierT, ICrudFilter> : KalturaOTTObject
        where IdentifierT : IConvertible
    {
        internal abstract ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter> Handler { get; }
        internal abstract void ValidateForAdd();
        internal abstract void ValidateForUpdate();
        internal abstract void SetId(IdentifierT id);
        
        protected override void Init()
        {
            base.Init();
        }
    }
}