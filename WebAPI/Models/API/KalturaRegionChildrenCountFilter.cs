using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Kaltura region children count filter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaRegionChildrenCountFilter : KalturaFilter<KalturaRegionChildrenCountOrderBy>, KalturaRelatedObjectFilter
    {
        public override KalturaRegionChildrenCountOrderBy GetDefaultOrderByValue()
        {
            return KalturaRegionChildrenCountOrderBy.NONE;
        }
    }

    public enum KalturaRegionChildrenCountOrderBy
    {
        NONE
    }
}