using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class IngestPricePlan
    {
        public string Code { get; set; }

        public string FullLifeCycle { get; set; }

        public string ViewLifeCycle { get; set; }

        public string MaxViews { get; set; }

        public string PriceCode { get; set; }

        public string CouponGroup { get; set; }

        public bool IsRenewable { get; set; }

        public string RecurringPeriods { get; set; }

        public eIngestAction Action { get; set; }

        public bool IsActive { get; set; }
    }
}
