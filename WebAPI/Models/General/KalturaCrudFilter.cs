using ApiLogic.Base;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace WebAPI.Models.General
{
    public interface IKalturaCrudFilter<ICrudHandeledObject, IdentifierT, ICrudFilter>
        where IdentifierT : IConvertible
    {
        ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter> Handler { get; }
        Type RelatedObjectFilterType { get; }
        void Validate();
    }

    /// <summary>
    /// Base Crud filter
    /// </summary>
    [Serializable]
    public abstract partial class KalturaCrudFilter<KalturaOrderByT, ICrudHandeledObject, IdentifierT, ICrudFilter> : KalturaFilter<KalturaOrderByT>, IKalturaCrudFilter<ICrudHandeledObject, IdentifierT, ICrudFilter>
        where KalturaOrderByT : struct, IComparable, IFormattable, IConvertible
        where IdentifierT : IConvertible
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public virtual Type RelatedObjectFilterType { get { return null;} }
        public abstract ICrudHandler<ICrudHandeledObject, IdentifierT, ICrudFilter> Handler { get; }
        public abstract void Validate();

        //public KalturaCrudFilter(Dictionary<string, object> parameters = null) : base(parameters)
        //{
        //}
    }
}