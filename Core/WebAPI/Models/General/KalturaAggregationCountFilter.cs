using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Kaltura Aggregation CountFilter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaAggregationCountFilter : KalturaFilter<KalturaAggregationCountOrderBy>, KalturaRelatedObjectFilter
    {
        internal virtual void Validate()
        {
        }

        public override KalturaAggregationCountOrderBy GetDefaultOrderByValue()
        {
            return KalturaAggregationCountOrderBy.NONE;
        }
    }

    public enum KalturaAggregationCountOrderBy
    {
        NONE
    }
}