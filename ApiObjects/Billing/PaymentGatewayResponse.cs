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
        public List<PaymentGatewayBase> pgw { get; set; }

        public PaymentGatewayResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            pgw = new List<PaymentGatewayBase>();
        }

        public PaymentGatewayResponse(ApiObjects.Response.Status resp, List<PaymentGatewayBase> pgw)
        {
            this.resp = resp;
            this.pgw = pgw;
        }
    }
}
