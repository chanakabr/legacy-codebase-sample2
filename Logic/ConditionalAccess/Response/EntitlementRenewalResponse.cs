using APILogic.ConditionalAccess.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.ConditionalAccess.Response
{
    public class EntitlementRenewalResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public EntitlementRenewal EntitlementRenewal { get; set; }
    }
}
