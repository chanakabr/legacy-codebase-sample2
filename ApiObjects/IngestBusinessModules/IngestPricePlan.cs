using ApiObjects.IngestBusinessModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class IngestPricePlan : IngestModule
    {
        public string FullLifeCycle { get; set; }

        public string ViewLifeCycle { get; set; }

        public int? MaxViews { get; set; }

        public IngestPriceCode PriceCode { get; set; }

        public bool? IsRenewable { get; set; }

        public int? RecurringPeriods { get; set; }

        public bool? IsActive { get; set; }

        public string Discount { get; set; } 
    }
}
