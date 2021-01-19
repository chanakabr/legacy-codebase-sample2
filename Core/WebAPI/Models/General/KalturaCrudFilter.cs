using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace WebAPI.Models.General
{
    public interface IKalturaCrudFilter<ICrudHandeledObject>
    {
        Type RelatedObjectFilterType { get; }
        void Validate(ContextData contextData);
        GenericListResponse<ICrudHandeledObject> List(ContextData contextData, CorePager pager);
    }

    /// <summary>
    /// Base Crud filter
    /// </summary>
    [Serializable]
    public abstract partial class KalturaCrudFilter<KalturaOrderByT, ICrudHandeledObject> : KalturaFilter<KalturaOrderByT>, IKalturaCrudFilter<ICrudHandeledObject>
        where KalturaOrderByT : struct, IComparable, IFormattable, IConvertible
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public virtual Type RelatedObjectFilterType { get { return null;} }
        public abstract void Validate(ContextData contextData);
        public abstract GenericListResponse<ICrudHandeledObject> List(ContextData contextData, CorePager pager);
        
        public KalturaCrudFilter(Dictionary<string, object> parameters = null) : base(parameters)
        {
        }
    }
}