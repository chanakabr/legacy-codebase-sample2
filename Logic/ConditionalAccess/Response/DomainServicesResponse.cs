using ApiObjects;
using ApiObjects.Pricing;
using Core.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess.Response
{
    public class DomainServicesResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<ServiceObject> Services { get; set; }

        public DomainServicesResponse(int code, List<ServiceObject> services)
        {
            Status = new ApiObjects.Response.Status(code);
            Services = services;
        }

        public DomainServicesResponse()
        {
            Status = new ApiObjects.Response.Status();
            Services = new List<ServiceObject>();
        }

        public DomainServicesResponse(int code)
        {
            Status = new ApiObjects.Response.Status(code);
            Services = new List<ServiceObject>();
        }
    }
}
