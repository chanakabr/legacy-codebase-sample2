using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess.Response
{
    public class DomainServicesResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<ConditionalAccess.TvinciPricing.ServiceObject> Services { get; set; }

        public DomainServicesResponse(int code, List<ConditionalAccess.TvinciPricing.ServiceObject> services)
        {
            Status = new ApiObjects.Response.Status(code);
            Services = services;
        }

        public DomainServicesResponse()
        {
            Status = new ApiObjects.Response.Status();
            Services = new List<TvinciPricing.ServiceObject>();
        }

        public DomainServicesResponse(int code)
        {
            Status = new ApiObjects.Response.Status(code);
            Services = new List<TvinciPricing.ServiceObject>();
        }
    }
}
