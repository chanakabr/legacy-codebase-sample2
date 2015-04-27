using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Clients.Utils;

namespace WebAPI.Clients
{
    public class BillingClient : BaseClient
    {
        public BillingClient()
        {
            
        }

        protected WebAPI.Billing.module Billing
        {
            get
            {
                return (Module as WebAPI.Billing.module);
            }
        }
    }
}