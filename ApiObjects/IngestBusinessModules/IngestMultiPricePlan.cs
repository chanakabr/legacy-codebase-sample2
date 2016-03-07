using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class IngestMultiPricePlan
    {
        public string Code { get; set; }

        public List<KeyValuePair<string,string>>  Title { get; set; } // lang, title

        public List<KeyValuePair<string, string>> Description { get; set; } // lang, description 

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string InternalDiscount { get; set; }

        public string CouponGroup { get; set; }

        public string ProductCode { get; set; }

        public bool IsRenewable { get; set; }

        public bool SubscriptionOnly { get; set; }
        
        public string PreviewModule { get; set; }

        public string DomainLimitationModule { get; set; }
                
        public int GracePeriodMinutes { get; set; }

        public List<string> PricePlansCodes { get; set; }

        public List<string> Channels { get; set; }

        public List<string> FileTypes { get; set; }

        public eIngestAction Action { get; set; }

        public bool IsActive { get; set; }

        public int OrderNum { get; set; }

        public int NumOfRecPeriods { get; set; }

    }
}
