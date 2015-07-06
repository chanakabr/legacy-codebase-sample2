using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGWResponse
    {
        public ApiObjects.Response.Status resp { get; set; }
        public List<PaymentGW> pgw { get; set; }

        public PaymentGWResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            pgw = new List<PaymentGW>();
        }

        public PaymentGWResponse(ApiObjects.Response.Status resp, List<PaymentGW> pgw)
        {
            this.resp = resp;
            this.pgw = pgw;
        }
    }
}
