using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Kaltura Aggregation CountFilter
    /// </summary>
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