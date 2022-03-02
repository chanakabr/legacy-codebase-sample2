using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Define KalturaRelatedObjectFilter
    /// </summary>
    [SchemeBase(typeof(KalturaFilter<KalturaDummyOrderBy>))]
    public interface KalturaRelatedObjectFilter : IKalturaFilter
    {
        // no implementation 
    }
}