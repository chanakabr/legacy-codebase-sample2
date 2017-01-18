using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Billing
{
    public class PaymentMethodIdResponse
    {
        public int PaymentMethodId { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
