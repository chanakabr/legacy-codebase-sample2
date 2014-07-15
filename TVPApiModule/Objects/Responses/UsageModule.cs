using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UsageModule
    {
        public int object_id { get; set; }

        public string virtual_name { get; set; }

        public int max_number_of_views { get; set; }

        public int view_life_cycle { get; set; }

        public int max_usage_module_life_cycle { get; set; }

        public int ext_discount_id { get; set; }

        public int internal_discount_id { get; set; }

        public int pricing_id { get; set; }

        public int coupon_id { get; set; }

        public int type { get; set; }

        public int subscription_only { get; set; }

        public int is_renew { get; set; }

        public int num_of_rec_periods { get; set; }

        public int device_limit_id { get; set; }

        public bool is_waiver { get; set; }

        public int waived_period { get; set; }
    }
}
