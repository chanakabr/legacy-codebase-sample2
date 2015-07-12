using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGWSettingsResponse
    {
        public ApiObjects.Response.Status resp { get; set; }
        public List<PaymentGW> pgw { get; set; }

        public PaymentGWSettingsResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            pgw = new List<PaymentGW>();
        }

        public PaymentGWSettingsResponse(ApiObjects.Response.Status resp, List<PaymentGW> pgw)
        {
            this.resp = resp;
            this.pgw = pgw;
        }
    }
}
