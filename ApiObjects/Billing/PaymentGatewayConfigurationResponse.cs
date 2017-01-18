using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGatewayConfigurationResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<ApiObjects.KeyValuePair> Configuration { get; set; }
    }
}
