using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ingest.Clients.ClientManager
{
    public enum ClientType
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