using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPApi;

namespace TVPApiModule.Objects
{
    public class PermittedPackages
    {
        public PermittedSubscriptionContainer PermittedSubscriptions {get; set;}
        public Media Package { get; set; }
    }
}
