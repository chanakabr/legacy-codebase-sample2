using ApiObjects.Base;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    enum KalturaDummyOrderBy
    {
    }

    /// <summary>
    /// Define KalturaRelatedObjectFilter
    /// </summary>
    [SchemeBase(typeof(KalturaFilter<KalturaDummyOrderBy>))]
    public interface KalturaRelatedObjectFilter : IKalturaFilter
    {
        // no implementation 
    }
}