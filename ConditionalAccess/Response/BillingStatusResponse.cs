using Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess.Response
{
    public class BillingStatusResponse
    {
        public BillingResponse BillingResponse { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
