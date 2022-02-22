using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Kaltura Aggregation CountFilter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaAggregationCountFilter : KalturaFilter<KalturaAggregationCountOrderBy>, KalturaRelatedObjectFilter
    {
        public override KalturaAggregationCountOrderBy GetDefaultOrderByValue()
        {
            return KalturaAggregationCountOrderBy.NONE;
        }
    }
}