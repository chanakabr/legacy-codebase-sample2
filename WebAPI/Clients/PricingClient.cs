using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Clients.Utils;

namespace WebAPI.Clients
{
    public class PricingClient : BaseClient
    {
        public PricingClient()
        {
            // TODO: Complete member initialization
        }

        protected WebAPI.Pricing.mdoule Pricing
        {
            get
            {
                return (Module as WebAPI.Pricing.mdoule);
            }
        }

    }
}