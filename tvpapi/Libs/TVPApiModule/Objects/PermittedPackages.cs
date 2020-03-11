using Core.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;

namespace TVPApiModule.Objects
{
    public class PermittedPackages
    {
        public PermittedSubscriptionContainer PermittedSubscriptions {get; set;}
        public Media Package { get; set; }
    }
}
