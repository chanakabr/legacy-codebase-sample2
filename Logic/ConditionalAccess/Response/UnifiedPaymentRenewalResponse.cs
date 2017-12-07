using APILogic.ConditionalAccess.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.ConditionalAccess.Response
{
    public class UnifiedPaymentRenewalResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public UnifiedPaymentRenewal UnifiedPaymentRenewal { get; set; }
    }
}
