using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Kaltura Aggregation CountFilter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public class KalturaAggregationCountFilter : KalturaFilter<KalturaAggregationCountOrderBy>, KalturaRelatedObjectFilter
    {
        internal virtual void Validate()
        {
        }

        public override KalturaAggregationCountOrderBy GetDefaultOrderByValue()
        {
            return KalturaAggregationCountOrderBy.NONE;
        }
    }
}