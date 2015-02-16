using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess.Response
{
    public class DomainServicesResponse
    {
        public StatusObject Status { get; set; }

        public List<ConditionalAccess.TvinciPricing.ServiceObject> Services { get; set; }

        public DomainServicesResponse(StatusObject status, List<ConditionalAccess.TvinciPricing.ServiceObject> services)
        {
            Status = status;
            Services = services;
        }

        public DomainServicesResponse(int code, List<ConditionalAccess.TvinciPricing.ServiceObject> services)
        {
            Status = new StatusObject(code);
            Services = services;
        }

        public DomainServicesResponse()
        {
            Status = new StatusObject();
        }
    }
}
