using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class BillingInfo
    {
        public string expiry_month { get; set; }

        public string expiry_year { get; set; }

        public string last_four_digits { get; set; }

        public string holder_name { get; set; }

        public string cvc { get; set; }

        public string variant { get; set; }
    }
}
