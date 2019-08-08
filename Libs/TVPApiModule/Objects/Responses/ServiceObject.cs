using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class ServiceObject
    {
        [JsonProperty(PropertyName = "id")]
        public long ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        public ServiceObject()
        {
        }

        public ServiceObject(ApiObjects.Pricing.ServiceObject service)
        {
            if (service != null)
            {
                ID = service.ID;
                Name = service.Name;
            }
        }

        public ServiceObject(long id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}
