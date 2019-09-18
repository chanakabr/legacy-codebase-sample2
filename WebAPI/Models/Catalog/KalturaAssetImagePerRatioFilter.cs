using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Kaltura asset image per ratio filter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaAssetImagePerRatioFilter : KalturaFilter<KalturaAssetImagePerRatioOrderBy>, KalturaRelatedObjectFilter
    {
        public override KalturaAssetImagePerRatioOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetImagePerRatioOrderBy.NONE;
        }
    }

    public enum KalturaAssetImagePerRatioOrderBy
    {
        NONE
    }
}