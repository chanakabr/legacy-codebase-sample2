using ApiLogic.Base;
using System;

namespace WebAPI.Models.General
{
    // TODO SHIR - CRUD changes
    /// <summary>
    /// Base class that have CRUD actions 
    /// </summary>
    [Serializable]
    public abstract partial class KalturaCrudObject<ICrudHandeledObject, IdentifierT, ICrudFilter> : KalturaOTTObject
        where IdentifierT : IConvertible
    {
        internal abstract ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter> Handler { get; } //  BaseCrudHandler<T>
        internal abstract void ValidateForAdd();
        internal abstract void ValidateForUpdate();
    }
}