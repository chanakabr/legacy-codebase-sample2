using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGatewayResponse
    {
        public ApiObjects.Response.Status resp { get; set; }
        public List<PaymentGateway> pgw { get; set; }

        public PaymentGatewayResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            pgw = new List<PaymentGateway>();
        }

        public PaymentGatewayResponse(ApiObjects.Response.Status resp, List<PaymentGateway> pgw)
        {
            this.resp = resp;
            this.pgw = pgw;
        }
    }
}
