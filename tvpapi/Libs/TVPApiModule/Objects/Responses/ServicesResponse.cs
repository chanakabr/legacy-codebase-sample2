using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class ServicesResponse
    {
        [JsonProperty(PropertyName = "services")]
        public List<ServiceObject> Services { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public ServicesResponse(List<ApiObjects.Pricing.ServiceObject> services, ApiObjects.Response.Status status)
        {
            if (services != null)
            {
                Services = new List<ServiceObject>();
                foreach (var service in services)
                {
                    Services.Add(new ServiceObject(service));
                }
            }

            if (status != null)
            {
                Status = new Status(status.Code, status.Message);
            }
        }

        public ServicesResponse()
        {
        }
    }
}
