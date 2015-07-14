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
        public List<PaymentGWBasic> pgw { get; set; }

        public PaymentGWResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            pgw = new List<PaymentGWBasic>();
        }

        public PaymentGWResponse(ApiObjects.Response.Status resp, List<PaymentGWBasic> pgw)
        {
            this.resp = resp;
            this.pgw = pgw;
        }
    }
}
