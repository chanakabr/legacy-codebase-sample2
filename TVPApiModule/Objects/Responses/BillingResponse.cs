using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class BillingResponse
    {

        public BillingResponseStatus status { get; set; }

        public string reciept_code { get; set; }

        public string status_description { get; set; }

        public string external_receipt_code { get; set; }
    }
}
