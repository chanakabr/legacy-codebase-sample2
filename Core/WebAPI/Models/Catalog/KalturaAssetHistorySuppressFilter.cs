using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Kaltura asset image per ratio filter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaAssetHistorySuppressFilter : KalturaFilter<KalturaAssetHistorySuppressFilterOrderBy>, KalturaRelatedObjectFilter
    {
        public override KalturaAssetHistorySuppressFilterOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetHistorySuppressFilterOrderBy.NONE;
        }
    }

    
}