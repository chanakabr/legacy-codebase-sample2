using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class PermittedPackages
    {
        public PermittedSubscriptionContainer permitted_subscriptions { get; set; }

        public Media package { get; set; }
    }
}
