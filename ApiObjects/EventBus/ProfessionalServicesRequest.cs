using EventBus.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.EventBus
{
    public class ProfessionalServicesRequest : ServiceEvent
    {
        [JsonProperty("model")]
        public object Model
        {
            get;
            set;
        }

        [JsonProperty("action_implementation")]
        public string ActionImplementation
        {
            get;
            set;
        }
    }
}
