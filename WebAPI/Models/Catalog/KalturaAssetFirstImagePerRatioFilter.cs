using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Kaltura asset first image per ratio filter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaAssetFirstImagePerRatioFilter : KalturaFilter<KalturaAssetFirstImagePerRatioOrderBy>, KalturaRelatedObjectFilter
    {
        public override KalturaAssetFirstImagePerRatioOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetFirstImagePerRatioOrderBy.NONE;
        }
    }

    public enum KalturaAssetFirstImagePerRatioOrderBy
    {
        NONE
    }
}