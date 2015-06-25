using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGWConfigResponse
    {
        public ApiObjects.Response.Status resp { get; set; }
        public List<PaymentGWConfig> pgw { get; set; }

        public PaymentGWConfigResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            pgw = new List<PaymentGWConfig>();
        }

        public PaymentGWConfigResponse(ApiObjects.Response.Status resp, List<PaymentGWConfig> pgw)
        {
            this.resp = resp;
            this.pgw = pgw;
        }
    }
}
