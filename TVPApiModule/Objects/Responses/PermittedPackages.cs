using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPApi;
using TVPApiModule.Objects.Responses;

namespace TVPApiModule.Objects
{
    public class PermittedPackages
    {
        public SubscriptionContainer PermittedSubscriptions {get; set;}
        public Media Package { get; set; }
    }
}
