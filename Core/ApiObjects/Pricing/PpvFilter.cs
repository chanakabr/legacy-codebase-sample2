using ApiObjects.SearchObjects;
using System.Collections.Generic;

namespace ApiObjects.Pricing
{
    public class PpvFilter
    {
        public OrderObj OrderBy { get; set; }
    }
    public class PpvByIdInFilter : PpvFilter
    {
        public List<string> IdIn { get; set; }

        public int? CouponGroupIdEqual { get; set; }
    }
}