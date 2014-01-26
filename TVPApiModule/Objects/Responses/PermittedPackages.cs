using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using TVPApiModule.Objects.Responses;

namespace TVPApiModule.Objects
{
    public class PermittedPackages
    {
        public PermittedSubscriptionContainer permitted_subscriptions { get; set; }

        public Media package { get; set; }
    }
}
