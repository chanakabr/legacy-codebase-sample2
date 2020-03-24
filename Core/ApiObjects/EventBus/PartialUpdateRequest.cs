using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using EventBus.Abstraction;
using Newtonsoft.Json;

namespace ApiObjects.EventBus
{
    public class PartialUpdateRequest : ServiceEvent
    {
        [JsonProperty("assets")]
        public AssetsPartialUpdate Assets { get; set; }
    }
}
