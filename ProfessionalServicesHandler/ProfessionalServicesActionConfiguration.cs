using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfessionalServicesHandler
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ProfessionalServicesActionConfiguration
    {
        [JsonProperty("DllLocation")]
        public string DllLocation
        {
            get;
            set;
        }

        [JsonProperty("Type")]
        public string Type
        {
            get;
            set;
        }
    }
}
