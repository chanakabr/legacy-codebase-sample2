using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Clients
{
    public class PricingClient : BaseClient
    {
        public PricingClient()
        {
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