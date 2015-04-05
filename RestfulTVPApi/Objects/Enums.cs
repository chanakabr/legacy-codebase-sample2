using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects
{
    public class Enums
    {
        public enum PlatformType
        {
            Web,
            STB,
            iPad,
            ConnectedTV,
            Cellular,
            Unknown
        }
        
        public enum Client
        {
            Api,
            Billing,
            ConditionalAccess,
            Domains,
            Notification,
            Pricing,
            Social,
            Users,
            Catalog
        }


    }
}