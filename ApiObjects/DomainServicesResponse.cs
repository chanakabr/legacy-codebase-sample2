using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class DomainServicesResponse
    {
        public StatusObject Status { get; set; }

        public List<ServiceObject> Services { get; set; }

        public DomainServicesResponse(StatusObject status, List<ServiceObject> services)
        {
            Status = status;
            Services = services;
        }

        public DomainServicesResponse(int code, List<ServiceObject> services)
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
