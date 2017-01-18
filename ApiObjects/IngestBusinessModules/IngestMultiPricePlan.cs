using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class IngestMultiPricePlan : IngestModule
    {
        public List<KeyValuePair>  Titles { get; set; } // lang, title

        public List<KeyValuePair> Descriptions { get; set; } // lang, description 

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string InternalDiscount { get; set; }

        public string CouponGroup { get; set; }

        public string ProductCode { get; set; }

        public bool? IsRenewable { get; set; }
        
        public string PreviewModule { get; set; }

        public string DomainLimitationModule { get; set; }
                
        public int? GracePeriodMinutes { get; set; }

        public List<string> PricePlansCodes { get; set; }

        public List<string> Channels { get; set; }

        public List<string> FileTypes { get; set; }

        public bool? IsActive { get; set; }

        public int? OrderNumber { get; set; }
    }
}
