using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ProfessionalServicesHandler
{
    class ProfessionalServicesRequest
    {
        [JsonProperty("group_id")]
        public int GroupID
        {
            get;
            set;
        }

        [JsonProperty("the_object")]
        public object TheObject
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
