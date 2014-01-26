using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class BillingResponse
    {

        public BillingResponseStatus status { get; set; }

        public string recieptCode { get; set; }

        public string statusDescription { get; set; }

        public string externalReceiptCode { get; set; }
    }
}
